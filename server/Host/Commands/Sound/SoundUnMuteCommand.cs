using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundUnMuteCommand : ICommand<SoundUnMuteCommand.Request>
	{
		private record Request();

		private readonly AudioOutputController _audioOutputController;

		public SoundUnMuteCommand(
			AudioOutputController audioOutputController)
		{
			this._audioOutputController = audioOutputController;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioOutputController.SetMute(false);
			return Task.CompletedTask;
		}
	}
}
