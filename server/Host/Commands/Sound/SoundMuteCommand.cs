using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundMuteCommand : ICommand<SoundMuteCommand.Request>
	{
		private record Request();

		private readonly AudioOutputController _audioOutputController;

		public SoundMuteCommand(
			AudioOutputController audioOutputController)
		{
			this._audioOutputController = audioOutputController;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioOutputController.SetMute(true);
			return Task.CompletedTask;
		}
	}
}
