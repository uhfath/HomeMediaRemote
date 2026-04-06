using HomeMediaRemote.Host.Commands;
using HomeMediaRemote.Host.Commands.Media;
using HomeMediaRemote.Host.Commands.Mic;
using HomeMediaRemote.Host.Commands.Pc;
using HomeMediaRemote.Host.Commands.Sound;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Threading;

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

		private readonly ConcurrentDictionary<string, Task> _processingTasks = new(StringComparer.OrdinalIgnoreCase);
		private readonly CommandDispatcher _commandDispatcher;
		private readonly ILogger<FileBackendService> _logger;
		private FileSystemWatcher _fileSystemWatcher = null!;

		private Task ExecuteCommand(string commandName)
		{
			return Task.Run(() =>
			{
				try
				{
					var commandMap = CommandLineCommandMaps[commandName];
					var requestType = CommandDispatcher.GetCommandRequestType(commandMap);
					var request = Activator.CreateInstance(requestType);
					_commandDispatcher.Execute(commandName, request!);
					File.Delete(commandName);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "File command processing error.");
				}
				finally
				{
					_processingTasks.TryRemove(commandName, out _);
				}
			});
		}

		private void OnFileCreatedEvent(object sender, FileSystemEventArgs eventArgs)
		{
			if (eventArgs.Name is null || !CommandLineCommandMaps.ContainsKey(eventArgs.Name))
			{
				return;
			}

			_processingTasks.AddOrUpdate(eventArgs.Name, ExecuteCommand, (_, c) => c);
		}

		public FileBackendService(
			CommandDispatcher commandDispatcher,
			ILogger<FileBackendService> logger)
		{
			this._commandDispatcher = commandDispatcher;
			this._logger = logger;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_fileSystemWatcher = new FileSystemWatcher(Directory.GetCurrentDirectory())
			{
				Filter = "*",
				NotifyFilter = NotifyFilters.FileName,
				EnableRaisingEvents = true,
			};

			_fileSystemWatcher.Created += OnFileCreatedEvent;

			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			foreach (var task in _processingTasks)
			{
				await task.Value;
			}

			_processingTasks.Clear();

			_fileSystemWatcher.Created -= OnFileCreatedEvent;
			_fileSystemWatcher.EnableRaisingEvents = false;
			_fileSystemWatcher.Dispose();
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
