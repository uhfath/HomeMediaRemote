

namespace HomeMediaRemote.Windows.Commands.Media
{
	internal class MediaPauseCommand : ICommand<MediaPauseCommand.MediaPauseRequest>
	{
		private record MediaPauseRequest
		{
			public int Step { get; init; }
		}

		Task ICommand<MediaPauseRequest>.ExecuteAsync(MediaPauseRequest request, CancellationToken cancellationToken)
		{
			Console.WriteLine("STEP: {0}", request.Step);
			return Task.CompletedTask;
		}
	}
}
