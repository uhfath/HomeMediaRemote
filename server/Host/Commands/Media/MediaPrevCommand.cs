namespace HomeMediaRemote.Host.Commands.Media
{
	internal class MediaPrevCommand : ICommand<MediaPrevCommand.MediaPrevRequest>
	{
		private record MediaPrevRequest
		{
		}

		Task ICommand<MediaPrevRequest>.ExecuteAsync(MediaPrevRequest request, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
