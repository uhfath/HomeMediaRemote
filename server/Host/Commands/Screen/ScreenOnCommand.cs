namespace HomeMediaRemote.Host.Commands.Screen
{
	internal class ScreenOnCommand : ICommand<ScreenOnCommand.Request>
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
