using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicOffCommand : ICommand<MicOffCommand.Request>
	{
		private record Request();

		private readonly AudioInputController _audioInputController;

		public MicOffCommand(
			AudioInputController audioInputController)
		{
			this._audioInputController = audioInputController;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioInputController.SetMute(true);
			return Task.CompletedTask;
		}
	}
}
