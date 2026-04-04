using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcShutdownCommand : ICommand<PcShutdownCommand.Request>
	{
		private record Request();

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			ShutdownManager.Shutdown();
			return Task.CompletedTask;
		}
	}
}
