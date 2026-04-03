namespace HomeMediaRemote.Windows.Commands.Mic
{
	internal class MicOnCommand : ICommand<MicOnCommand.Request>
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
