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
		private readonly ICommand<MicOnCommand.Request> _micOnCommand;
		private readonly IconStatusManager _iconStatusManager;

		public MicOffCommand(
			AudioInputController audioInputController,
			ICommand<MicOnCommand.Request> micOnCommand,
			IconStatusManager iconStatusManager)
		{
			this._audioInputController = audioInputController;
			this._micOnCommand = micOnCommand;
			this._iconStatusManager = iconStatusManager;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioInputController.SetMute(true);
			_iconStatusManager.ShowMicOffStatus(() => _micOnCommand.Execute(MicOnCommand.Request.Empty));
		}
	}
}
