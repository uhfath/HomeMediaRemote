using HomeMediaRemote.Media.Windows;

namespace HomeMediaRemote.Host.Commands.Media
{
	internal class MediaPrevCommand : ICommand<MediaPrevCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		void ICommand<Request>.Execute(Request request)
		{
			MediaController.PreviousTrack();
		}
	}
}
