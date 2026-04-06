using HomeMediaRemote.Host.Commands;
using HomeMediaRemote.Host.Commands.Media;
using HomeMediaRemote.Host.Commands.Mic;
using HomeMediaRemote.Host.Commands.Pc;
using HomeMediaRemote.Host.Commands.Sound;
using System.IO.Pipes;

namespace HomeMediaRemote.Host.Services
{
	internal class PipeBackendService : BackgroundService
	{
		private const string PipeName = "home-media-commands";
		private static readonly TimeSpan PipeConnectionTimeout = TimeSpan.FromSeconds(10);

		private static readonly IReadOnlyDictionary<string, Type> CommandLineCommandMaps = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
		{
			{ "media-next", typeof(MediaNextCommand) },
			{ "media-pause", typeof(MediaPauseCommand) },
			{ "media-play", typeof(MediaPlayCommand) },
			{ "media-prev", typeof(MediaPrevCommand) },

			{ "mic-off", typeof(MicOffCommand) },
			{ "mic-on", typeof(MicOnCommand) },
			{ "mic-sens_down", typeof(MicSensDownCommand) },
			{ "mic-sens_up", typeof(MicSensUpCommand) },
			{ "mic-trigger", typeof(MicTriggerCommand) },

			{ "pc-lock", typeof(PcLockCommand) },
			{ "pc-restart", typeof(PcRestartCommand) },
			{ "pc-shutdown", typeof(PcShutdownCommand) },
			{ "pc-sleep", typeof(PcSleepCommand) },

			{ "sound-mute", typeof(SoundMuteCommand) },
			{ "sound-trigger", typeof(SoundTriggerCommand) },
			{ "sound-unmute", typeof(SoundUnMuteCommand) },
			{ "sound-vol_down", typeof(SoundVolDownCommand) },
			{ "sound-vol_up", typeof(SoundVolUpCommand) },
		};

		private readonly CommandDispatcher _commandDispatcher;
		
		public static IEnumerable<string> CommandNames = CommandLineCommandMaps.Keys;

		private void ExecuteCommand(string command)
		{
			var commandMap = CommandLineCommandMaps[command];
			var requestType = CommandDispatcher.GetCommandRequestType(commandMap);
			var request = Activator.CreateInstance(requestType);
			_commandDispatcher.Execute(command, request!);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				await using var pipe = new NamedPipeServerStream(PipeName, PipeDirection.In);
				await pipe.WaitForConnectionAsync(stoppingToken);

				using var reader = new StreamReader(pipe);
				var command = await reader.ReadLineAsync(stoppingToken);
				if (!string.IsNullOrWhiteSpace(command))
				{
					if (CommandLineCommandMaps.ContainsKey(command))
					{
						ExecuteCommand(command);
					}
				}
			}
		}

		public PipeBackendService(
			CommandDispatcher commandDispatcher)
		{
			this._commandDispatcher = commandDispatcher;
		}

		public static async Task<bool> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default)
		{
			if (CommandLineCommandMaps.ContainsKey(command))
			{
				await using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
				await pipe.ConnectAsync(PipeConnectionTimeout, cancellationToken);

				await using var writer = new StreamWriter(pipe);
				await writer.WriteLineAsync(command.AsMemory(), cancellationToken);

				return true;
			}

			return false;
		}

		public static IServiceCollection AddPipeBackendServices(IServiceCollection services)
		{
			services
				.AddHostedService<PipeBackendService>();
			;

			return services;
		}
	}
}
