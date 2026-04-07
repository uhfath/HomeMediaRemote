using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicOnCommand : ICommand<MicOnCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		private readonly AudioInputController _audioInputController;

		public MicOnCommand(
			AudioInputController audioInputController)
		{
			this._audioInputController = audioInputController;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioInputController.Mute = false;
		}
	}
}
