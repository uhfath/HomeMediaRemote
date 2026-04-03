using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicOnCommand : ICommand<MicOnCommand.Request>
	{
		private record Request();

		private readonly AudioInputController _audioInputController;

		public MicOnCommand(
			AudioInputController audioInputController)
		{
			this._audioInputController = audioInputController;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioInputController.SetMute(false);
			return Task.CompletedTask;
		}
	}
}
