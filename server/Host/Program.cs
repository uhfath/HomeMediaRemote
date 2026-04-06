using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Host.Commands;
using HomeMediaRemote.Host.Services;
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

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AllocConsole();

		private const int ATTACH_PARENT_PROCESS = -1;

        private static async Task<bool> ProcessCommandLineAsync(string[] args, CancellationToken cancellationToken)
        {
			var argsDict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

			for (var i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith('-'))
				{
					var key = args[i].TrimStart('-');
					var value = (string?)null;

					// ≈сли следующий аргумент не начинаетс€ с '-', считаем его значением
					if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
					{
						value = args[++i];
					}

					argsDict[key] = value;
				}
			}

            if (argsDict.ContainsKey("help") || argsDict.ContainsKey("h") || argsDict.ContainsKey("?"))
            {
				Console.WriteLine();
				Console.WriteLine("Parameter: 'command'");
				var commands = string.Join(", ", PipeBackendService.CommandNames.Select(c => $"'{c}'"));
				Console.WriteLine("Commands: {0}", commands);

				return true;
            }

			if (argsDict.TryGetValue("command", out var command))
            {
				if (command is not null)
				{
					if (!await PipeBackendService.ExecuteCommandAsync(command, cancellationToken))
					{
						Console.Error.WriteLine("Unknown command.");
					}
				}
				else
				{
					Console.Error.WriteLine("Empty command.");
				}

				return true;
			}

			return false;
		}

		private static async Task Main(string[] args)
        {
			var isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);
			if (isDevelopment && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				if (!AttachConsole(ATTACH_PARENT_PROCESS))
				{
					// ≈сли не удалось прикрепить (например, запущено не из CMD), создаЄм новую
					AllocConsole();
				}
			}

			if (await ProcessCommandLineAsync(args, default))
			{
				return;
			}

			var builder = WebApplication.CreateBuilder(args);

			builder.Services
                .AddSingleton<CommandDispatcher>()
                .AddHostedService<MicState>()
                .AddHostedService<SoundState>()
            ;

            builder.Services
                .AddMediaServices()
                .AddPcServices()
				.AddAudioServices()
                .AddStatusServices()
            ;

			HttpBackendService.AddHttpBackendServices(builder.Services);
			PipeBackendService.AddPipeBackendServices(builder.Services);

            builder.Services.AddAuthorization();

            var app = builder.Build();
            app.UseAuthorization();

			HttpBackendService.UseHttpBackendServices(app);

			await app.RunAsync();
        }
    }
}
