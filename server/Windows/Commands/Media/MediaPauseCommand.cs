namespace HomeMediaRemote.Windows.Commands.Media
{
	internal class MediaPauseCommand : ICommand<MediaPauseCommand.Request>
	{
		private record Request
		{
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
