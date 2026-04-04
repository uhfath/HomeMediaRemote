using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcRestartCommand : ICommand<PcRestartCommand.Request>
	{
		private record Request();

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			ShutdownManager.Restart();
			return Task.CompletedTask;
		}
	}
}
