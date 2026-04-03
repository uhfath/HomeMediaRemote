namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundVolDownCommand : ICommand<SoundVolDownCommand.Request>
	{
		private record Request
		{
			public int Step { get; init; }
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			Console.WriteLine("STEP: {0}", request.Step);
			return Task.CompletedTask;
		}
	}
}
