using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundMuteCommand : ICommand<SoundMuteCommand.Request>
	{
		private record Request();

		private readonly AudioOutputController _audioOutputController;
		private readonly IconStatusManager _iconStatusManager;

		public SoundMuteCommand(
			AudioOutputController audioOutputController,
			IconStatusManager iconStatusManager)
		{
			this._audioOutputController = audioOutputController;
			this._iconStatusManager = iconStatusManager;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioOutputController.SetMute(true);
			_iconStatusManager.ShowNoSoundStatus();
			return Task.CompletedTask;
		}
	}
}
