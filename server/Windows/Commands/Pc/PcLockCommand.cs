namespace HomeMediaRemote.Windows.Commands.Pc
{
	internal class PcLockCommand : ICommand<PcLockCommand.Request>
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
