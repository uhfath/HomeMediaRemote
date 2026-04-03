namespace HomeMediaRemote.Windows.Commands.Pc
{
	internal class PcSleepCommand : ICommand<PcSleepCommand.Request>
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
