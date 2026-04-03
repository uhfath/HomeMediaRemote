using System.Runtime.InteropServices;

namespace HomeMediaRemote.Audio.Windows
{
	/// <summary>
	/// Управление текущим активным устройством вывода (колонки / наушники):
	/// mute/unmute и уровень громкости.
	/// </summary>
	public sealed class AudioOutputController : IDisposable
	{
		private const float VolumeStep = 0.05f;

		private IAudioEndpointVolume _ep;

		public AudioOutputController()
		{
			_ep = EndpointVolumeFactory.Create(EDataFlow.eRender);
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
			var ctx = Guid.Empty;
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
			var ctx = Guid.Empty;
			Marshal.ThrowExceptionForHR(_ep.SetMasterVolumeLevelScalar(level, ref ctx));
		}

		/// <summary>
		/// Увеличить громкость на шаг 'VolumeStep'
		/// </summary>
		/// <param name="step">шаг</param>
		public void SetVolumeUp(int step)
		{
			SetVolume(Math.Min(1.0f, GetVolume() + VolumeStep * step));
		}

		/// <summary>
		/// Уменьшить громкость на шаг 'VolumeStep'
		/// </summary>
		/// <param name="step">шаг</param>
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
