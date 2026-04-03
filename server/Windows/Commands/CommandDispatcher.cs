namespace HomeMediaRemote.Windows.Commands
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

		public async Task ExecuteAsync(string commandType, object request, CancellationToken cancellationToken = default)
		{
			var commandEntry = _commandDispatcherCache.GetOrCreateCommand(commandType, request);
			var command = _serviceProvider.GetRequiredService(commandEntry.CommandType);
			var result = commandEntry.CommandMethod.Invoke(command, [request, cancellationToken]);

			if (result is Task taskResult)
			{
				await taskResult;
			}

			if (result is ValueTask valueTaskResult)
			{
				await valueTaskResult;
			}
		}
	}
}
