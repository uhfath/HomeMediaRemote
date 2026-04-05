using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicOffCommand : ICommand<MicOffCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		private readonly AudioInputController _audioInputController;

		public MicOffCommand(
			AudioInputController audioInputController)
		{
			this._audioInputController = audioInputController;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioInputController.SetMute(true);
		}
	}
}
