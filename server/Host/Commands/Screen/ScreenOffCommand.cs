namespace HomeMediaRemote.Host.Commands.Screen
{
	internal class ScreenOffCommand : ICommand<ScreenOffCommand.Request>
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
