using Microsoft.Extensions.DependencyInjection;

namespace HomeMediaRemote.Audio.Windows
{
	public static class AudioServiceExtensions
	{
		public static IServiceCollection AddAudioServices(this IServiceCollection services)
		{
			services
				.AddSingleton<AudioInputController>()
				.AddSingleton<AudioOutputController>()
			;

			return services;
		}
	}
}
