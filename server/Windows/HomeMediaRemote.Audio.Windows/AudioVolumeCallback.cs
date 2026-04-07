using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HomeMediaRemote.Audio.Windows
{
	/// <summary>
	/// Данные, передаваемые подписчику при изменении громкости / mute.
	/// </summary>
	public sealed class AudioVolumeChangedEventArgs : EventArgs
	{
		/// <summary>Звук заглушён?</summary>
		public bool IsMuted { get; }

		/// <summary>Уровень громкости 0.0 … 1.0.</summary>
		public float MasterVolume { get; }

		/// <summary>
		/// true — изменение произошло извне (клавиатура, микшер ОС и т.д.),
		/// false — изменение инициировано нашим кодом.
		/// </summary>
		public bool IsExternalChange { get; }

		public AudioVolumeChangedEventArgs(bool isMuted, float masterVolume, bool isExternal)
		{
			IsMuted = isMuted;
			MasterVolume = masterVolume;
			IsExternalChange = isExternal;
		}
	}

	/// <summary>
	/// Managed-реализация COM-интерфейса IAudioEndpointVolumeCallback.
	/// COM будет вызывать OnNotify при каждом изменении громкости/mute.
	/// </summary>
	[ComVisible(true)]
	internal sealed class AudioVolumeCallback : IAudioEndpointVolumeCallback
	{
		private readonly Guid _ownContext;
		private readonly Action<AudioVolumeChangedEventArgs> _handler;
		private readonly ILogger<AudioController> _logger;

		/// <param name="ownContext">
		/// GUID, который мы передаём в SetMute / SetVolume.
		/// Если входящее уведомление содержит этот GUID — значит мы сами его вызвали.
		/// </param>
		/// <param name="handler">Делегат, вызываемый при уведомлении.</param>
		public AudioVolumeCallback(Guid ownContext, Action<AudioVolumeChangedEventArgs> handler, ILogger<AudioController> logger)
		{
			_ownContext = ownContext;
			_handler = handler ?? throw new ArgumentNullException(nameof(handler));
			_logger = logger;
		}

		public void TriggerNotify(bool isMuted, float volume, bool isExternal)
		{
			try
			{
				var args = new AudioVolumeChangedEventArgs(
					isMuted,
					volume,
					isExternal);

				_handler(args);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Callback error.");
			}
		}

		public int OnNotify(nint pNotifyData)
		{
			if (pNotifyData == nint.Zero)
			{
				return 0; // S_OK
			}

			var data = Marshal.PtrToStructure<AUDIO_VOLUME_NOTIFICATION_DATA>(pNotifyData);

			var isExternal = data.guidEventContext != _ownContext;
			TriggerNotify(data.bMuted, data.fMasterVolume, isExternal);

			return 0; // S_OK
		}
	}
}
