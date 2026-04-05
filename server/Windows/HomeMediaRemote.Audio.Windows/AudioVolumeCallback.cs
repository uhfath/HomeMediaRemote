using System.Runtime.InteropServices;

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

		/// <param name="ownContext">
		/// GUID, который мы передаём в SetMute / SetVolume.
		/// Если входящее уведомление содержит этот GUID — значит мы сами его вызвали.
		/// </param>
		/// <param name="handler">Делегат, вызываемый при уведомлении.</param>
		public AudioVolumeCallback(Guid ownContext, Action<AudioVolumeChangedEventArgs> handler)
		{
			_ownContext = ownContext;
			_handler = handler ?? throw new ArgumentNullException(nameof(handler));
		}

		public int OnNotify(nint pNotifyData)
		{
			if (pNotifyData == nint.Zero)
				return 0; // S_OK

			var data = Marshal.PtrToStructure<AUDIO_VOLUME_NOTIFICATION_DATA>(pNotifyData);

			bool isExternal = data.guidEventContext != _ownContext;

			var args = new AudioVolumeChangedEventArgs(
				data.bMuted,
				data.fMasterVolume,
				isExternal);

			try
			{
				_handler(args);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.ToString());
			}

			return 0; // S_OK
		}
	}
}
