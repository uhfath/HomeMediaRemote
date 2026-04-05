using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcShutdownCommand : ICommand<PcShutdownCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		private readonly ILogger<PcShutdownCommand> _logger;

		public PcShutdownCommand(
			ILogger<PcShutdownCommand> logger)
		{
			this._logger = logger;
		}

		void ICommand<Request>.Execute(Request request)
		{
			CommandScheduler.ScheduleAction(() => ShutdownManager.Shutdown(), TimeSpan.FromSeconds(3), _logger);
		}
	}
}
