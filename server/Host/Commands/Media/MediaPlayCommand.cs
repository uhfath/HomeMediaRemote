namespace HomeMediaRemote.Host.Commands.Media
{
	internal class MediaPlayCommand : ICommand<MediaPlayCommand.Request>
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
