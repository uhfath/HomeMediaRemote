using System.Runtime.InteropServices;

namespace HomeMediaRemote.Audio.Windows
{
	/// <summary>
	/// Управление текущим активным устройством вывода (колонки / наушники):
	/// mute/unmute, уровень громкости, подписка на внешние изменения.
	/// </summary>
	public sealed class AudioOutputController : IDisposable
	{
		private const float VolumeStep = 0.05f;

		/// <summary>
		/// GUID, который мы передаём во все «свои» вызовы.
		/// Позволяет отличить собственные изменения от внешних.
		/// </summary>
		private static readonly Guid OwnEventContext =
			new("A1B2C3D4-1234-5678-9ABC-DEF012345678"); // замените на свой при желании

		private IAudioEndpointVolume _ep;

		// ── Callback state ──────────────────────────────────
		private AudioVolumeCallback? _callback;

		public AudioOutputController()
		{
			_ep = EndpointVolumeFactory.Create(EDataFlow.eRender);
		}

		// ── Подписка на изменения ───────────────────────────

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
			ThrowIfDisposed();
			if (_callback != null)
				throw new InvalidOperationException("Уже подписаны. Сначала вызовите Unsubscribe().");

			_callback = new AudioVolumeCallback(OwnEventContext, handler);

			int hr = _ep.RegisterControlChangeNotify(_callback);    // CLR создаёт CCW автоматически
			if (hr < 0)
			{
				_callback = null;
				Marshal.ThrowExceptionForHR(hr);
			}
		}

		/// <summary>Отписаться от уведомлений.</summary>
		public void Unsubscribe()
		{
			ThrowIfDisposed();
			UnsubscribeInternal();
		}

		private void UnsubscribeInternal()
		{
			if (_callback != null)
			{
				_ep.UnregisterControlChangeNotify(_callback);
				_callback = null;
			}
		}

		// ── Mute / Unmute ───────────────────────────────────

		/// <summary>Текущее состояние: true — звук выключен.</summary>
		public bool IsMuted
		{
			get
			{
				ThrowIfDisposed();
				Marshal.ThrowExceptionForHR(_ep.GetMute(out var muted));
				return muted;
			}
		}

		/// <summary>Включить (false) или выключить (true) звук.</summary>
		public void SetMute(bool mute)
		{
			ThrowIfDisposed();
			var ctx = OwnEventContext;                       // ← наш GUID
			Marshal.ThrowExceptionForHR(_ep.SetMute(mute, ref ctx));
		}

		// ── Громкость ───────────────────────────────────────

		/// <summary>Текущий уровень громкости: 0.0 … 1.0.</summary>
		public float GetVolume()
		{
			ThrowIfDisposed();
			Marshal.ThrowExceptionForHR(_ep.GetMasterVolumeLevelScalar(out var level));
			return level;
		}

		/// <summary>
		/// Установить громкость.
		/// </summary>
		/// <param name="level">Значение от 0.0 (тишина) до 1.0 (максимум).</param>
		public void SetVolume(float level)
		{
			ThrowIfDisposed();
			if (level < 0f || level > 1f)
				throw new ArgumentOutOfRangeException(nameof(level),
					"Допустимый диапазон: 0.0 … 1.0");
			var ctx = OwnEventContext;                       // ← наш GUID
			Marshal.ThrowExceptionForHR(_ep.SetMasterVolumeLevelScalar(level, ref ctx));
		}

		/// <summary>
		/// Увеличить громкость на шаг 'VolumeStep'
		/// </summary>
		public void SetVolumeUp(int step)
		{
			SetVolume(Math.Min(1.0f, GetVolume() + VolumeStep * step));
		}

		/// <summary>
		/// Уменьшить громкость на шаг 'VolumeStep'
		/// </summary>
		public void SetVolumeDown(int step)
		{
			SetVolume(Math.Max(0.0f, GetVolume() - VolumeStep * step));
		}

		// ── Dispose ─────────────────────────────────────────

		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed && _ep != null)
			{
				UnsubscribeInternal();           // ← сначала отписываемся
				Marshal.ReleaseComObject(_ep);
				_ep = null!;
				_disposed = true;
			}
		}

		private void ThrowIfDisposed()
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
		}
	}
}
