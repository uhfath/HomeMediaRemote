using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcShutdownCommand : ICommand<PcShutdownCommand.Request>
	{
		private readonly ILogger<PcShutdownCommand> _logger;

		private record Request();

		public PcShutdownCommand(
			ILogger<PcShutdownCommand> logger)
		{
			this._logger = logger;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			CommandScheduler.ScheduleAction(() => ShutdownManager.Shutdown(), TimeSpan.FromSeconds(3), _logger);
			return Task.CompletedTask;
		}
	}
}
