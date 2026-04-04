using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcRestartCommand : ICommand<PcRestartCommand.Request>
	{
		private readonly ILogger<PcRestartCommand> _logger;

		private record Request();

		public PcRestartCommand(
			ILogger<PcRestartCommand> logger)
		{
			this._logger = logger;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			CommandScheduler.ScheduleAction(() => ShutdownManager.Restart(), TimeSpan.FromSeconds(3), _logger);
			return Task.CompletedTask;
		}
	}
}
