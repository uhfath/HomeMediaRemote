using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcRestartCommand : ICommand<PcRestartCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		private readonly ILogger<PcRestartCommand> _logger;

		public PcRestartCommand(
			ILogger<PcRestartCommand> logger)
		{
			this._logger = logger;
		}

		void ICommand<Request>.Execute(Request request)
		{
			CommandScheduler.ScheduleAction(() => ShutdownManager.Restart(), TimeSpan.FromSeconds(3), _logger);
		}
	}
}
