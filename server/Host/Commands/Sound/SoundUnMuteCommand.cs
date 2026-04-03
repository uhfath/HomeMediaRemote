namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundUnMuteCommand : ICommand<SoundUnMuteCommand.Request>
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
