using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicSensUpCommand : ICommand<MicSensUpCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
			public int Step { get; init; }
		}

		private readonly AudioInputController _audioInputController;

		public MicSensUpCommand(
			AudioInputController audioInputController)
		{
			this._audioInputController = audioInputController;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioInputController.SetVolumeUp(request.Step);
		}
	}
}
