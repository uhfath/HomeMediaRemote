namespace HomeMediaRemote.Host.Commands
{
	internal class CommandDispatcher
	{
		private readonly CommandDispatcherCache _commandDispatcherCache;
		private readonly IServiceProvider _serviceProvider;

		public CommandDispatcher(
			CommandDispatcherCache commandDispatcherCache,
			IServiceProvider serviceProvider)
		{
			this._commandDispatcherCache = commandDispatcherCache;
			this._serviceProvider = serviceProvider;
		}

		public void Execute(string commandType, object request)
		{
			var commandEntry = _commandDispatcherCache.GetCommand(commandType, request);
			var command = _serviceProvider.GetRequiredService(commandEntry.CommandType);
			var result = commandEntry.CommandMethod.Invoke(command, [request]);

			//if (result is Task taskResult)
			//{
			//	await taskResult;
			//}

			//if (result is ValueTask valueTaskResult)
			//{
			//	await valueTaskResult;
			//}
		}
	}
}
