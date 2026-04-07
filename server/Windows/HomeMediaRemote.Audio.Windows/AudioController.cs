using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace HomeMediaRemote.Audio.Windows
{
	/// <summary>
	/// Управление текущим активным устройством:
	/// mute/unmute, уровень громкости, подписка на внешние изменения.
	///
	/// При смене устройства по умолчанию в ОС — автоматически переключается
	/// на новое устройство, сохраняя подписку на изменения громкости.
	/// </summary>
	public abstract class AudioController : IDisposable
	{
		private const ERole CurrentRole = ERole.eMultimedia;

		private const float VolumeStep = 0.05f;

		/// <summary>
		/// GUID, который мы передаём во все «свои» вызовы.
		/// Позволяет отличить собственные изменения от внешних.
		/// </summary>
		private readonly Guid _ownEventContext;

		/// <summary>
		/// Тип устройства (ввод/вывод)
		/// </summary>
		private readonly EDataFlow _eDataFlow;

		// ── Синхронизация ───────────────────────────────────
		//
		//  Read-lock  — операции, которые ИСПОЛЬЗУЮТ _ep
		//               (GetVolume, SetVolume, SetMute, …).
		//               Могут выполняться параллельно между собой.
		//
		//  Write-lock — операции, которые МЕНЯЮТ _ep / _callback
		//               (SwitchEndpoint, Subscribe, Unsubscribe, Dispose).
		//               Ждут завершения всех read и блокируют новые.
		// ─────────────────────────────────────────────────────

		private readonly ReaderWriterLockSlim _rwLock = new();
		private readonly ILogger<AudioController> _logger;

		// ── COM-объекты ─────────────────────────────────────
		private IMMDeviceEnumerator _enumerator;
		private DeviceNotificationClient _notificationClient;
		private IAudioEndpointVolume? _endpoint;

		// ── Volume callback state ───────────────────────────
		private AudioVolumeCallback? _callback;
		private Action<AudioVolumeChangedEventArgs>? _subscribedHandler;

		// ── Публичные события ───────────────────────────────

		/// <summary>
		/// Срабатывает после переключения на новое устройство по умолчанию.
		/// </summary>
		public event EventHandler? DefaultDeviceChanged;

		// ── Конструктор ─────────────────────────────────────

		protected AudioController(
			Guid ownEventContext,
			EDataFlow eDataFlow,
			ILogger<AudioController> logger)
		{
			this._ownEventContext = ownEventContext;
			this._eDataFlow = eDataFlow;
			this._logger = logger;

			_enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorCom();

			try
			{
				_endpoint = EndpointVolumeFactory.Create(_eDataFlow);
			}
			catch (COMException ex)
			{
				// Устройство вывода отсутствует — _ep останется null.
				_logger.LogError(ex, "Аудио устройство отсутствует.");
			}

			_notificationClient = new DeviceNotificationClient(this);
			Marshal.ThrowExceptionForHR(_enumerator.RegisterEndpointNotificationCallback(_notificationClient));
		}

		// ══════════════════════════════════════════════════════
		//  Переключение устройства
		// ══════════════════════════════════════════════════════

		private void SwitchEndpoint()
		{
			// 1. Создаём новый endpoint ВНЕ блокировки
			IAudioEndpointVolume? newEp = null;

			try
			{
				newEp = EndpointVolumeFactory.Create(EDataFlow.eRender, _enumerator);
			}
			catch (COMException ex)
			{
				// Нового устройства нет (все отключены).
				_logger.LogError(ex, "Аудио устройство отсутствует.");
			}

			IAudioEndpointVolume? oldEp;
			AudioVolumeCallback? oldCallback;

			// 2. Write-lock: подменяем endpoint и callback
			_rwLock.EnterWriteLock();

			try
			{
				if (_disposed)
				{
					if (newEp != null)
					{
						Marshal.ReleaseComObject(newEp);
					}

					return;
				}

				oldEp = _endpoint;
				oldCallback = _callback;

				_endpoint = newEp;
				_callback = null;

				if (_subscribedHandler != null && _endpoint != null)
				{
					_callback = new AudioVolumeCallback(_ownEventContext, _subscribedHandler, _logger);
					var hr = _endpoint.RegisterControlChangeNotify(_callback);
					if (hr < 0)
					{
						_callback = null;
					}

					_callback!.TriggerNotify(GetMutedCore(), GetVolumeCore(), true);
				}
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}

			// 3. Очистка старого endpoint ВНЕ блокировки,
			//    чтобы избежать deadlock: UnregisterControlChangeNotify
			//    может ждать завершения OnNotify, а тот через
			//    обработчик пользователя может запросить read-lock.
			if (oldCallback != null && oldEp != null)
			{
				try
				{
					oldEp.UnregisterControlChangeNotify(oldCallback);
				}
				catch (COMException ex)
				{
					/* endpoint мог быть уже отключён */
					_logger.LogError(ex, "Аудио устройство отключено.");
				}
			}

			if (oldEp != null)
			{
				try
				{
					Marshal.ReleaseComObject(oldEp);
				}
				catch (COMException ex)
				{
					_logger.LogError(ex, "Аудио устройство отключено.");
				}
			}

			// 4. Уведомляем подписчиков (без блокировки)
			DefaultDeviceChanged?.Invoke(this, EventArgs.Empty);
		}

		// ══════════════════════════════════════════════════════
		//  Подписка на изменения громкости          [write-lock]
		// ══════════════════════════════════════════════════════

		/// <summary>
		/// Подписаться на уведомления об изменении громкости / mute.
		/// Коллбэк будет вызван при ЛЮБОМ изменении (включая собственные),
		/// но <see cref="AudioVolumeChangedEventArgs.IsExternalChange"/>
		/// позволяет отличить внешние изменения.
		/// </summary>
		/// <param name="handler">Делегат-обработчик.</param>
		/// <exception cref="InvalidOperationException">Уже подписаны.</exception>
		public void Subscribe(Action<AudioVolumeChangedEventArgs> handler)
		{
			ArgumentNullException.ThrowIfNull(handler);

			_rwLock.EnterWriteLock();

			try
			{
				ThrowIfDisposed();
				if (_callback != null)
				{
					throw new InvalidOperationException("Уже подписаны. Сначала вызовите Unsubscribe().");
				}

				_subscribedHandler = handler;

				if (_endpoint == null)
				{
					return; // handler сохранён; подпишемся при появлении устройства
				}

				_callback = new AudioVolumeCallback(_ownEventContext, _subscribedHandler, _logger);
				var hr = _endpoint.RegisterControlChangeNotify(_callback);
				if (hr < 0)
				{
					_callback = null;
					_subscribedHandler = null;
					Marshal.ThrowExceptionForHR(hr);
				}

				_callback!.TriggerNotify(GetMutedCore(), GetVolumeCore(), true);
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
		}

		/// <summary>Отписаться от уведомлений.</summary>
		public void Unsubscribe()
		{
			_rwLock.EnterWriteLock();

			try
			{
				ThrowIfDisposed();
				UnsubscribeInternal();
				_subscribedHandler = null;
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}
		}

		private void UnsubscribeInternal()
		{
			// Вызывается только под write-lock
			if (_callback != null && _endpoint != null)
			{
				_endpoint.UnregisterControlChangeNotify(_callback);
				_callback = null;
			}
		}

		// ══════════════════════════════════════════════════════
		//  Mute / Unmute                             [read-lock]
		// ══════════════════════════════════════════════════════

		/// <summary>Включить (false) или выключить (true) звук либо текущее состояние.</summary>
		public bool Mute
		{
			get
			{
				_rwLock.EnterReadLock();

				try
				{
					ThrowIfDisposed();
					ThrowIfNoDevice();
					return GetMutedCore();
				}
				finally
				{
					_rwLock.ExitReadLock();
				}
			}

			set
			{
				_rwLock.EnterReadLock();

				try
				{
					ThrowIfDisposed();
					ThrowIfNoDevice();
					SetMutedCore(value);
				}
				finally
				{
					_rwLock.ExitReadLock();
				}
			}
		}

		public void TriggerMute()
		{
			_rwLock.EnterReadLock();

			try
			{
				ThrowIfDisposed();
				ThrowIfNoDevice();
				SetMutedCore(!GetMutedCore());
			}
			finally
			{
				_rwLock.ExitReadLock();
			}
		}

		// ══════════════════════════════════════════════════════
		//  Громкость                                 [read-lock]
		// ══════════════════════════════════════════════════════

		/// <summary>Установка уровня громкости 0.0 … 1.0 либо текущий уровень громкости.</summary>
		public float Volume
		{
			get
			{
				_rwLock.EnterReadLock();

				try
				{
					ThrowIfDisposed();
					ThrowIfNoDevice();
					return GetVolumeCore();
				}
				finally
				{
					_rwLock.ExitReadLock();
				}
			}

			set
			{
				_rwLock.EnterReadLock();

				try
				{
					ThrowIfDisposed();
					ThrowIfNoDevice();
					SetVolumeCore(value);
				}
				finally
				{
					_rwLock.ExitReadLock();
				}
			}
		}

		/// <summary>
		/// Увеличить громкость на шаг.
		/// Get + Set выполняются атомарно под одним read-lock,
		/// поэтому подмена устройства между ними невозможна.
		/// </summary>
		public void SetVolumeUp(int step)
		{
			_rwLock.EnterReadLock();

			try
			{
				ThrowIfDisposed();
				ThrowIfNoDevice();
				SetVolumeCore(Math.Min(1.0f, GetVolumeCore() + VolumeStep * step));
			}
			finally
			{
				_rwLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Уменьшить громкость на шаг.
		/// </summary>
		public void SetVolumeDown(int step)
		{
			_rwLock.EnterReadLock();

			try
			{
				ThrowIfDisposed();
				ThrowIfNoDevice();
				SetVolumeCore(Math.Max(0.0f, GetVolumeCore() - VolumeStep * step));
			}
			finally
			{
				_rwLock.ExitReadLock();
			}
		}

		// ── Unlocked «ядро» (вызывается только под read-lock) ──

		private float GetVolumeCore()
		{
			Marshal.ThrowExceptionForHR(_endpoint!.GetMasterVolumeLevelScalar(out var level));
			return level;
		}

		private void SetVolumeCore(float level)
		{
			if (level < 0f || level > 1f)
			{
				throw new ArgumentOutOfRangeException(nameof(level), "Допустимый диапазон: 0.0 … 1.0");
			}

			var ctx = _ownEventContext;
			Marshal.ThrowExceptionForHR(_endpoint!.SetMasterVolumeLevelScalar(level, ref ctx));
		}

		private bool GetMutedCore()
		{
			Marshal.ThrowExceptionForHR(_endpoint!.GetMute(out var muted));
			return muted;
		}

		private void SetMutedCore(bool muted)
		{
			var ctx = _ownEventContext;
			Marshal.ThrowExceptionForHR(_endpoint!.SetMute(muted, ref ctx));
		}

		// ══════════════════════════════════════════════════════
		//  Dispose                                  [write-lock]
		// ══════════════════════════════════════════════════════

		private bool _disposed;

		public void Dispose()
		{
			_rwLock.EnterWriteLock();

			try
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;

				UnsubscribeInternal();
				_subscribedHandler = null;

				if (_endpoint != null)
				{
					try
					{
						Marshal.ReleaseComObject(_endpoint);
					}
					catch (COMException ex)
					{
						_logger.LogError(ex, "Аудио устройство отключено.");
					}

					_endpoint = null;
				}
			}
			finally
			{
				_rwLock.ExitWriteLock();
			}

			// Отписка от уведомлений ОС — вне write-lock
			if (_notificationClient != null)
			{
				try
				{
					_enumerator.UnregisterEndpointNotificationCallback(_notificationClient);
				}
				catch (COMException ex)
				{
					_logger.LogError(ex, "Аудио устройство отключено.");
				}

				_notificationClient = null!;
			}

			if (_enumerator != null)
			{
				try
				{
					Marshal.ReleaseComObject(_enumerator);
				}
				catch (COMException ex)
				{
					_logger.LogError(ex, "Аудио устройство отключено.");
				}

				_enumerator = null!;
			}

			_rwLock.Dispose();
		}

		// ── Guards ──────────────────────────────────────────

		private void ThrowIfDisposed()
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
		}

		private void ThrowIfNoDevice()
		{
			if (_endpoint == null)
			{
				throw new InvalidOperationException("Устройство звука по умолчанию отсутствует.");
			}
		}

		// ══════════════════════════════════════════════════════
		//  IMMNotificationClient (вложенный класс)
		// ══════════════════════════════════════════════════════

		[ComVisible(true)]
		private sealed class DeviceNotificationClient : IMMNotificationClient
		{
			private readonly AudioController _owner;

			public DeviceNotificationClient(AudioController owner)
			{
				_owner = owner;
			}

			public int OnDefaultDeviceChanged(EDataFlow flow, ERole role, string? defaultDeviceId)
			{
				if (flow == _owner._eDataFlow && role == CurrentRole)
				{
					_owner._logger.LogInformation("Default device changed: {DeviceId}", defaultDeviceId);

					// ══════════════════════════════════════════════════
					//  НЕЛЬЗЯ вызывать аудио-API внутри этого коллбэка.
					//  Откладываем всю работу на пул потоков.
					// ══════════════════════════════════════════════════
					Task.Run(() =>
					{
						try
						{
							_owner.SwitchEndpoint();
						}
						catch (Exception ex)
						{
							_owner._logger.LogError(ex, "[AudioController] SwitchEndpoint failed");
						}
					});
				}

				return 0;
			}

			public int OnDeviceStateChanged(string deviceId, int newState) => 0;
			public int OnDeviceAdded(string deviceId) => 0;
			public int OnDeviceRemoved(string deviceId) => 0;
			public int OnPropertyValueChanged(string deviceId, PROPERTYKEY key) => 0;
		}
	}
}
