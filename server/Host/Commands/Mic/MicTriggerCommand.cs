using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicTriggerCommand : ICommand<MicTriggerCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		private readonly AudioInputController _audioInputController;

		public MicTriggerCommand(
			AudioInputController audioInputController)
		{
			this._audioInputController = audioInputController;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioInputController.TriggerMute();
		}
	}
}
