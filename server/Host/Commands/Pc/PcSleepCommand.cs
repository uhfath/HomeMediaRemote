using HomeMediaRemote.Pc.Windows;

namespace HomeMediaRemote.Host.Commands.Pc
{
	internal class PcSleepCommand : ICommand<PcSleepCommand.Request>
	{
		private record Request();

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			SleepManager.Sleep();
			return Task.CompletedTask;
		}
	}
}
