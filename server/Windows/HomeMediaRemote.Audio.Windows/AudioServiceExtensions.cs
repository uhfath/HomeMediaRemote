using Microsoft.Extensions.DependencyInjection;

namespace HomeMediaRemote.Audio.Windows
{
	public static class AudioServiceExtensions
	{
		public static IServiceCollection AddAudioServices(this IServiceCollection services)
		{
			services
				.AddScoped<AudioInputController>()
				.AddScoped<AudioOutputController>()
			;

			return services;
		}
	}
}
