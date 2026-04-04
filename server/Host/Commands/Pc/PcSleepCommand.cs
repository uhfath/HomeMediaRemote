using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcSleepCommand : ICommand<PcSleepCommand.Request>
	{
		private readonly ILogger<PcSleepCommand> _logger;

		private record Request();

		public PcSleepCommand(
			ILogger<PcSleepCommand> logger)
		{
			this._logger = logger;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			CommandScheduler.ScheduleAction(() => SleepManager.Sleep(), TimeSpan.FromSeconds(3), _logger);
			return Task.CompletedTask;
		}
	}
}
