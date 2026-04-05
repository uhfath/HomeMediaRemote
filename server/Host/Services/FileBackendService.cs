using HomeMediaRemote.Host.Commands;
using HomeMediaRemote.Host.Commands.Media;
using HomeMediaRemote.Host.Commands.Mic;
using HomeMediaRemote.Host.Commands.Pc;
using HomeMediaRemote.Host.Commands.Sound;

namespace HomeMediaRemote.Host.Services
{
	internal class FileBackendService : IHostedService
	{
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

			{ "pc-lock", typeof(PcLockCommand) },
			{ "pc-restart", typeof(PcRestartCommand) },
			{ "pc-shutdown", typeof(PcShutdownCommand) },
			{ "pc-sleep", typeof(PcSleepCommand) },

			{ "sound-mute", typeof(SoundMuteCommand) },
			{ "sound-unmute", typeof(SoundUnMuteCommand) },
			{ "sound-vol_down", typeof(SoundVolDownCommand) },
			{ "sound-vol_up", typeof(SoundVolUpCommand) },
		};

		public static IEnumerable<string> FileNames = CommandLineCommandMaps.Keys;

		public Task StartAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public static bool CreateFileCommand(string command)
		{
			if (CommandLineCommandMaps.ContainsKey(command))
			{
				using var _ = File.Create(command);
				return true;
			}

			return false;
		}

		public static IServiceCollection AddFileBackendServices(IServiceCollection services)
		{
			services
				.AddHostedService<FileBackendService>();
			;

			return services;
		}
	}
}
