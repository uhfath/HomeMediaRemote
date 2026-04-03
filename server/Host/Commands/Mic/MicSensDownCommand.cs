using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicSensDownCommand : ICommand<MicSensDownCommand.Request>
	{
		private record Request
		{
			public int Step { get; init; }
		}

		private readonly AudioInputController _audioInputController;

		public MicSensDownCommand(
			AudioInputController audioInputController)
		{
			this._audioInputController = audioInputController;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioInputController.SetVolumeDown(request.Step);
			return Task.CompletedTask;
		}
	}
}
