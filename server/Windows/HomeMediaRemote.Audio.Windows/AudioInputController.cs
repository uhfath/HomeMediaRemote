using System.Runtime.InteropServices;

namespace HomeMediaRemote.Audio.Windows
{
	/// <summary>
	/// Управление текущим активным устройством ввода (микрофоном):
	/// mute/unmute и уровень чувствительности (громкости записи).
	/// </summary>
	public sealed class AudioInputController : IDisposable
	{
		private IAudioEndpointVolume _ep;

		public AudioInputController()
		{
			_ep = EndpointVolumeFactory.Create(EDataFlow.eCapture);
		}

		// ── Mute / Unmute ───────────────────────────────────

		/// <summary>Текущее состояние: true — микрофон отключён.</summary>
		public bool IsMuted
		{
			get
			{
				ThrowIfDisposed();
				Marshal.ThrowExceptionForHR(_ep.GetMute(out var muted));
				return muted;
			}
		}

		/// <summary>Включить (false) или выключить (true) микрофон.</summary>
		public void SetMute(bool mute)
		{
			ThrowIfDisposed();
			var ctx = Guid.Empty;
			Marshal.ThrowExceptionForHR(_ep.SetMute(mute, ref ctx));
		}

		// ── Чувствительность (громкость записи) ─────────────

		/// <summary>Текущий уровень: 0.0 … 1.0.</summary>
		public float GetVolume()
		{
			ThrowIfDisposed();
			Marshal.ThrowExceptionForHR(_ep.GetMasterVolumeLevelScalar(out var level));
			return level;
		}

		/// <summary>
		/// Установить чувствительность микрофона.
		/// </summary>
		/// <param name="level">Значение от 0.0 (тишина) до 1.0 (максимум).</param>
		public void SetVolume(float level)
		{
			ThrowIfDisposed();
			if (level < 0f || level > 1f)
				throw new ArgumentOutOfRangeException(nameof(level),
					"Допустимый диапазон: 0.0 … 1.0");
			var ctx = Guid.Empty;
			Marshal.ThrowExceptionForHR(_ep.SetMasterVolumeLevelScalar(level, ref ctx));
		}

		// ── Dispose ─────────────────────────────────────────

		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed && _ep != null)
			{
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
