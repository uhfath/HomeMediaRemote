using HomeMediaRemote.Host.Commands;
using HomeMediaRemote.Host.Commands.Media;
using HomeMediaRemote.Host.Commands.Mic;
using HomeMediaRemote.Host.Commands.Pc;
using HomeMediaRemote.Host.Commands.Sound;

namespace HomeMediaRemote.Host.Services
{
	internal class HttpBackendService
	{
		private static readonly IReadOnlyDictionary<string, Type> CommandsMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
		{
			{ "/media/next", typeof(MediaNextCommand) },
			{ "/media/pause", typeof(MediaPauseCommand) },
			{ "/media/play", typeof(MediaPlayCommand) },
			{ "/media/prev", typeof(MediaPrevCommand) },

			{ "/mic/off", typeof(MicOffCommand) },
			{ "/mic/on", typeof(MicOnCommand) },
			{ "/mic/sens_down", typeof(MicSensDownCommand) },
			{ "/mic/sens_up", typeof(MicSensUpCommand) },

			{ "/pc/lock", typeof(PcLockCommand) },
			{ "/pc/restart", typeof(PcRestartCommand) },
			{ "/pc/shutdown", typeof(PcShutdownCommand) },
			{ "/pc/sleep", typeof(PcSleepCommand) },

			{ "/sound/mute", typeof(SoundMuteCommand) },
			{ "/sound/unmute", typeof(SoundUnMuteCommand) },
			{ "/sound/vol_down", typeof(SoundVolDownCommand) },
			{ "/sound/vol_up", typeof(SoundVolUpCommand) },
		};

		public static IServiceCollection AddHttpBackendServices(IServiceCollection services)
		{
			foreach (var commandMap in CommandsMap)
			{
				services.AddTransient(CommandDispatcher.GetCommandInterfaceType(commandMap.Value), commandMap.Value);
			}

			return services;
		}

		public static IEndpointRouteBuilder UseHttpBackendServices(IEndpointRouteBuilder endpointRouteBuilder)
		{
            var groupBuilder = endpointRouteBuilder.MapGroup("/api");
            foreach (var commandMap in CommandsMap)
            {
				groupBuilder.MapPost(commandMap.Key, async (
                    CommandDispatcher commandDispatcher,
                    HttpRequest httpRequest,
                    CancellationToken cancellationToken) =>
				{
                    var requestType = CommandDispatcher.GetCommandRequestType(commandMap.Value);
                    var request = await httpRequest.ReadFromJsonAsync(requestType, cancellationToken) ?? throw new InvalidOperationException("Пустой запрос для исполнения.");
					commandDispatcher.Execute(commandMap.Key, request);
				});
			}

			return endpointRouteBuilder;
		}
	}
}
