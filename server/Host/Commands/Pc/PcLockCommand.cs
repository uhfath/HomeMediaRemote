using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcLockCommand : ICommand<PcLockCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		void ICommand<Request>.Execute(Request request)
		{
			SessionManager.Lock();
		}
	}
}
