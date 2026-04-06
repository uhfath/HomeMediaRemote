using System.Collections.Concurrent;
using System.Reflection;

namespace HomeMediaRemote.Host.Commands
{
	internal class CommandDispatcher
	{
		private readonly ConcurrentDictionary<string, CommandEntry> _commandTypes = new(StringComparer.OrdinalIgnoreCase);
		private readonly IServiceProvider _serviceProvider;

		private CommandEntry CreateCommandEntry(object request)
		{
			var requestType = request.GetType();
			var commandType = typeof(ICommand<>).MakeGenericType(requestType);
			var executeMethod = commandType.GetMethod(nameof(ICommand<object>.Execute));
			return new CommandEntry(commandType, executeMethod!);
		}

		private CommandEntry GetCommand(string commandType, object request) =>
			_commandTypes.GetOrAdd(commandType, _ => CreateCommandEntry(request));

		public CommandDispatcher(
			IServiceProvider serviceProvider)
		{
			this._serviceProvider = serviceProvider;
		}

		public void Execute(string commandType, object request)
		{
			var commandEntry = GetCommand(commandType, request);
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

		public static Type GetCommandInterfaceType(Type commandType)
		{
			var commandInterfaceType = commandType
				.GetInterfaces()
				.Where(i => i.IsGenericType)
				.Where(i => i.GetGenericTypeDefinition() == typeof(ICommand<>))
				.FirstOrDefault()
			;

			if (commandInterfaceType == null)
			{
				throw new InvalidOperationException($"Команда '{commandType.FullName}' не реализует интерфейс 'ICommand<T>'.");
			}

			return commandInterfaceType;
		}

		public static Type GetCommandRequestType(Type commandType)
		{
			var commandInterfaceType = GetCommandInterfaceType(commandType);
			var requestType = commandInterfaceType.GetGenericArguments()[0];

			return requestType;
		}

		private record CommandEntry(Type CommandType, MethodInfo CommandMethod);
	}
}
