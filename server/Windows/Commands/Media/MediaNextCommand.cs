

namespace HomeMediaRemote.Windows.Commands.Media
{
	internal class MediaNextCommand : ICommand<MediaNextCommand.MediaNextRequest>
	{
		private record MediaNextRequest
		{
			public int Step { get; init; }
		}

		Task ICommand<MediaNextRequest>.ExecuteAsync(MediaNextRequest request, CancellationToken cancellationToken)
		{
			Console.WriteLine("STEP: {0}", request.Step);
			return Task.CompletedTask;
		}
	}
}
