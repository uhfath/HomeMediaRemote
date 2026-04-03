using HomeMediaRemote.Media.Windows;

namespace HomeMediaRemote.Host.Commands.Media
{
	internal class MediaNextCommand : ICommand<MediaNextCommand.Request>
	{
		private record Request();

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			MediaController.NextTrack();
			return Task.CompletedTask;
		}
	}
}
