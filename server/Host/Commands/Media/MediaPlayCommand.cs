using HomeMediaRemote.Media.Windows;

namespace HomeMediaRemote.Host.Commands.Media
{
	internal class MediaPlayCommand : ICommand<MediaPlayCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		void ICommand<Request>.Execute(Request request)
		{
			MediaController.Play();
		}
	}
}
