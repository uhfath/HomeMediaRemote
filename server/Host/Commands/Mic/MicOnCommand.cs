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
		private readonly IconStatusManager _iconStatusManager;

		public MicOnCommand(
			AudioInputController audioInputController,
			IconStatusManager iconStatusManager)
		{
			this._audioInputController = audioInputController;
			this._iconStatusManager = iconStatusManager;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioInputController.SetMute(false);
			_iconStatusManager.HideMicOffStatus();
		}
	}
}
