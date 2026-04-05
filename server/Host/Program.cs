using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Host.Commands;
using HomeMediaRemote.Host.Commands.Media;
using HomeMediaRemote.Host.Commands.Mic;
using HomeMediaRemote.Host.Commands.Pc;
using HomeMediaRemote.Host.Commands.Sound;
using HomeMediaRemote.Host.States;
using HomeMediaRemote.Media.Windows;
using HomeMediaRemote.Pc.Windows;
using HomeMediaRemote.Status.Windows;
using System.Runtime.InteropServices;

namespace HomeMediaRemote.Host
{
    internal class Program
    {
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AttachConsole(int dwProcessId);
		private const int ATTACH_PARENT_PROCESS = -1;

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

            if (builder.Environment.IsDevelopment())
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
            }

            builder.Services
                .AddSingleton<CommandDispatcherCache>()
                .AddScoped<CommandDispatcher>()
                .AddHostedService<MicState>()
                .AddHostedService<SoundState>()
            ;

            builder.Services
                .AddMediaServices()
                .AddPcServices()
				.AddAudioServices()
                .AddStatusServices()
            ;

            foreach (var commandMap in CommandsMap)
            {
                builder.Services.AddTransient(GetCommandInterfaceType(commandMap.Value), commandMap.Value);
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
					commandDispatcher.Execute(commandMap.Key, request);
				});
			}

			app.Run();
        }
    }
}
