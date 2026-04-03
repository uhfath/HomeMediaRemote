namespace HomeMediaRemote.Windows.Commands.Pc
{
	internal class PcRestartCommand : ICommand<PcRestartCommand.Request>
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
