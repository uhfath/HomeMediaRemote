using HomeMediaRemote.Media.Windows;

namespace HomeMediaRemote.Host.Commands.Media
{
	internal class MediaPauseCommand : ICommand<MediaPauseCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		void ICommand<Request>.Execute(Request request)
		{
			MediaController.Pause();
		}
	}
}
