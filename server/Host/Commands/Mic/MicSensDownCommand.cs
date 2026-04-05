using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicSensDownCommand : ICommand<MicSensDownCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
			public int Step { get; init; }
		}

		private readonly AudioInputController _audioInputController;

		public MicSensDownCommand(
			AudioInputController audioInputController)
		{
			this._audioInputController = audioInputController;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioInputController.SetVolumeDown(request.Step);
		}
	}
}
