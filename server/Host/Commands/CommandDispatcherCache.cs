using System.Collections.Concurrent;
using System.Reflection;

namespace HomeMediaRemote.Host.Commands
{
	internal class CommandDispatcherCache
	{
		private readonly ConcurrentDictionary<string, CommandEntry> _commandTypes = new(StringComparer.OrdinalIgnoreCase);

		private CommandEntry CreateCommandEntry(object request)
		{
			var requestType = request.GetType();
			var commandType = typeof(ICommand<>).MakeGenericType(requestType);
			var executeMethod = commandType.GetMethod(nameof(ICommand<object>.ExecuteAsync));
			return new CommandEntry(commandType, executeMethod!);
		}

		public CommandEntry GetCommand(string commandType, object request) =>
			_commandTypes.GetOrAdd(commandType, _ => CreateCommandEntry(request));

		public record CommandEntry(Type CommandType, MethodInfo CommandMethod);
	}
}
