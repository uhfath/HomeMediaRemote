using HomeMediaRemote.Media.Windows;

namespace HomeMediaRemote.Host.Commands.Media
{
	internal class MediaNextCommand : ICommand<MediaNextCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		void ICommand<Request>.Execute(Request request)
		{
			MediaController.NextTrack();
		}
	}
}
