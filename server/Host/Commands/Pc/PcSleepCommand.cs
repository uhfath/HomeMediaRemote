using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcSleepCommand : ICommand<PcSleepCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		public readonly ILogger<PcSleepCommand> _logger;

		public PcSleepCommand(
			ILogger<PcSleepCommand> logger)
		{
			this._logger = logger;
		}

		void ICommand<Request>.Execute(Request request)
		{
			CommandScheduler.ScheduleAction(() => SleepManager.Sleep(), TimeSpan.FromSeconds(3), _logger);
		}
	}
}
