using Microsoft.Extensions.DependencyInjection;

namespace HomeMediaRemote.Status.Windows
{
	public static class StatusServiceExtensions
	{
		public static IServiceCollection AddStatusServices(this IServiceCollection services)
		{
			services
				.AddOptions<ScreenOverlayStateOptions>()
				.BindConfiguration("ScreenOverlayState")
			;

			services
				.AddSingleton<ScreenOverlayManager>()
				.AddSingleton<IconManager>()
				.AddSingleton<FormManager>()
				.AddSingleton<TrayIconManager>()
				.AddTransient<IconStatusManager>()
			;

			return services;
		}
	}
}
