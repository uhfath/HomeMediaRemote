using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicOnCommand : ICommand<MicOnCommand.Request>
	{
		private record Request();

		private readonly AudioInputController _audioInputController;
		private readonly IconStatusManager _iconStatusManager;

		public MicOnCommand(
			AudioInputController audioInputController,
			IconStatusManager iconStatusManager)
		{
			this._audioInputController = audioInputController;
			this._iconStatusManager = iconStatusManager;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioInputController.SetMute(false);
			_iconStatusManager.HideMicOffStatus();

			return Task.CompletedTask;
		}
	}
}
