

namespace HomeMediaRemote.Windows.Commands.Media
{
	internal class MediaPlayCommand : ICommand<MediaPlayCommand.MediaPlayRequest>
	{
		private record MediaPlayRequest
		{
			public int Step { get; init; }
		}

		Task ICommand<MediaPlayRequest>.ExecuteAsync(MediaPlayRequest request, CancellationToken cancellationToken)
		{
			Console.WriteLine("STEP: {0}", request.Step);
			return Task.CompletedTask;
		}
	}
}
