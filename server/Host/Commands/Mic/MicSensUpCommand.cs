using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicSensUpCommand : ICommand<MicSensUpCommand.Request>
	{
		private record Request
		{
			public int Step { get; init; }
		}

		private readonly AudioInputController _audioInputController;

		public MicSensUpCommand(
			AudioInputController audioInputController)
		{
			this._audioInputController = audioInputController;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioInputController.SetVolumeUp(request.Step);
			return Task.CompletedTask;
		}
	}
}
