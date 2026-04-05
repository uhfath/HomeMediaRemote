using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.Commands.Mic
{
	internal class MicOffCommand : ICommand<MicOffCommand.Request>
	{
		private record Request();

		private readonly AudioInputController _audioInputController;
		private readonly IconStatusManager _iconStatusManager;

		public MicOffCommand(
			AudioInputController audioInputController,
			IconStatusManager iconStatusManager)
		{
			this._audioInputController = audioInputController;
			this._iconStatusManager = iconStatusManager;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioInputController.SetMute(true);
			_iconStatusManager.ShowMicOffStatus();

			return Task.CompletedTask;
		}
	}
}
