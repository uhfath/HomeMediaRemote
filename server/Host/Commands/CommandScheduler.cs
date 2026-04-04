namespace HomeMediaRemote.Host.Commands
{
	internal static class CommandScheduler
	{
		private static async Task ExecuteDelayed(Action action, TimeSpan delay, ILogger logger)
		{
			try
			{
				await Task.Delay(delay);
				action();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Delayed action error.");
			}
		}

		public static void ScheduleAction(Action action, TimeSpan delay, ILogger logger)
		{
			_ = ExecuteDelayed(action, delay, logger);
			return;
		}
	}
}
