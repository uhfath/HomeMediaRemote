namespace HomeMediaRemote.Windows.Commands
{
	internal class CommandDispatcher
	{
		private readonly IServiceProvider _serviceProvider;

		public CommandDispatcher(
			IServiceProvider serviceProvider)
		{
			this._serviceProvider = serviceProvider;
		}

		public async Task ExecuteAsync(object request, CancellationToken cancellationToken = default)
		{
			var requestType = request.GetType();
			var commandType = typeof(ICommand<>).MakeGenericType(requestType);
			var executeMethod = commandType.GetMethod(nameof(ICommand<object>.ExecuteAsync));

			var command = _serviceProvider.GetRequiredService(commandType);
			var result = executeMethod!.Invoke(command, [request, cancellationToken]);

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
