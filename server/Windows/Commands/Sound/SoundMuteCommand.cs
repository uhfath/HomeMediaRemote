namespace HomeMediaRemote.Windows.Commands.Sound
{
	internal class SoundMuteCommand : ICommand<SoundMuteCommand.Request>
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
