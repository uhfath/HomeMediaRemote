namespace HomeMediaRemote.Windows.Commands.Mic
{
	internal class MicOffCommand : ICommand<MicOffCommand.Request>
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
