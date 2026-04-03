

namespace HomeMediaRemote.Windows.Commands.Media
{
	internal class MediaPrevCommand : ICommand<MediaPrevCommand.MediaPrevRequest>
	{
		private record MediaPrevRequest
		{
			public int Step { get; init; }
		}

		Task ICommand<MediaPrevRequest>.ExecuteAsync(MediaPrevRequest request, CancellationToken cancellationToken)
		{
			Console.WriteLine("STEP: {0}", request.Step);
			return Task.CompletedTask;
		}
	}
}
