using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcLockCommand : ICommand<PcLockCommand.Request>
	{
		private record Request();

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			SessionManager.Lock();
			return Task.CompletedTask;
		}
	}
}
