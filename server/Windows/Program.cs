using HomeMediaRemote.Windows.Commands;
using HomeMediaRemote.Windows.Commands.Media;

namespace HomeMediaRemote.Windows
{
    internal class Program
    {
        private static readonly Dictionary<string, Type> CommandsMap = new(StringComparer.OrdinalIgnoreCase)
		{
            { "/media/next", typeof(MediaNextCommand) },
            { "/media/pause", typeof(MediaPauseCommand) },
            { "/media/play", typeof(MediaPlayCommand) },
            { "/media/prev", typeof(MediaPrevCommand) },
        };

        private static Type GetCommandInterfaceType(Type commandType)
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

        private static Type GetCommandRequestType(Type commandType)
        {
            var commandInterfaceType = GetCommandInterfaceType(commandType);
			var requestType = commandInterfaceType.GetGenericArguments()[0];

            return requestType;
		}

        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<CommandDispatcher>();

            foreach (var commandMap in CommandsMap)
            {
                builder.Services.AddScoped(GetCommandInterfaceType(commandMap.Value), commandMap.Value);
            }

            builder.Services.AddAuthorization();

            var app = builder.Build();
            app.UseAuthorization();

            var groupBuilder = app.MapGroup("/api");

            foreach (var commandMap in CommandsMap)
            {
				groupBuilder.MapPost(commandMap.Key, async (
                    CommandDispatcher commandDispatcher,
                    HttpRequest httpRequest,
                    CancellationToken cancellationToken) =>
				{
                    var requestType = GetCommandRequestType(commandMap.Value);
                    var request = await httpRequest.ReadFromJsonAsync(requestType, cancellationToken) ?? throw new InvalidOperationException("Пустой запрос для исполнения.");
					await commandDispatcher.ExecuteAsync(request, cancellationToken);
				});
			}

			app.Run();
        }
    }
}
