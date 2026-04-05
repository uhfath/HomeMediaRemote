using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundMuteCommand : ICommand<SoundMuteCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		private readonly AudioOutputController _audioOutputController;
		private readonly ICommand<SoundUnMuteCommand.Request> _soundUnMuteCommand;
		private readonly IconStatusManager _iconStatusManager;

		public SoundMuteCommand(
			AudioOutputController audioOutputController,
			ICommand<SoundUnMuteCommand.Request> soundUnMuteCommand,
			IconStatusManager iconStatusManager)
		{
			this._audioOutputController = audioOutputController;
			this._soundUnMuteCommand = soundUnMuteCommand;
			this._iconStatusManager = iconStatusManager;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioOutputController.SetMute(true);
			_iconStatusManager.ShowNoSoundStatus(() => _soundUnMuteCommand.Execute(SoundUnMuteCommand.Request.Empty));
		}
	}
}
