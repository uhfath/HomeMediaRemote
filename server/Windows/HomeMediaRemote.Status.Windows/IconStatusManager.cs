namespace HomeMediaRemote.Status.Windows
{
	public class IconStatusManager
	{
		private const double OverlayScreenSizePercent = 0.1;

		private const string MicOffOverlayStatusKey = "mic_off_screen";
		private const string MicOffOverlayIconPath = "mic_off_screen.svg";
		
		private const string MicOffTrayStatusKey = "mic_off_tray";
		private const string MicOffTrayIconPath = "mic_off_tray.svg";

		private const string NoSoundOverlayStatusKey = "no_sound_screen";
		private const string NoSoundOverlayIconPath = "no_sound_screen.svg";

		private const string NoSoundTrayStatusKey = "no_sound_tray";
		private const string NoSoundTrayIconPath = "no_sound_tray.svg";

		private readonly ScreenOverlayManager _screenOverlayManager;
		private readonly IconManager _iconManager;

		private static Size CalculateOverlaySize(Screen monitor)
		{
			var minSide = Math.Min(monitor.Bounds.Width, monitor.Bounds.Height);
			var target = (int)Math.Round(minSide * OverlayScreenSizePercent);

			return new Size(target, target);
		}

		private static Size CalculateTraySize()
		{
			var dpi = NativeMethods.GetDpiForSystem();
			var width = NativeMethods.GetSystemMetricsForDpi(NativeMethods.SM_CXSMICON, dpi);
			var height = NativeMethods.GetSystemMetricsForDpi(NativeMethods.SM_CYSMICON, dpi);

			return new Size(width, height);
		}

		public IconStatusManager(
			ScreenOverlayManager screenOverlayManager,
			IconManager iconManager)
		{
			this._screenOverlayManager = screenOverlayManager;
			this._iconManager = iconManager;
		}

		public void ShowMicOffStatus()
		{
			if (Screen.PrimaryScreen is null)
			{
				throw new InvalidOperationException("Primary screen not detected.");
			}

			var overlayIconSize = CalculateOverlaySize(Screen.PrimaryScreen);
			var overlayIcon = _iconManager.GetIcon(MicOffOverlayIconPath, overlayIconSize, true);
			_screenOverlayManager.AddScreenOverlay(MicOffOverlayStatusKey, overlayIcon);
		}

		public void HideMicOffStatus()
		{
			_screenOverlayManager.RemoveScreenOverlay(MicOffOverlayStatusKey);
		}

		public void ShowNoSoundStatus()
		{
			if (Screen.PrimaryScreen is null)
			{
				throw new InvalidOperationException("Primary screen not detected.");
			}

			var overlayIconSize = CalculateOverlaySize(Screen.PrimaryScreen);
			var overlayIcon = _iconManager.GetIcon(NoSoundOverlayIconPath, overlayIconSize, true);
			_screenOverlayManager.AddScreenOverlay(NoSoundOverlayStatusKey, overlayIcon);
		}

		public void HideNoSoundStatus()
		{
			_screenOverlayManager.RemoveScreenOverlay(NoSoundOverlayStatusKey);
		}
	}
}
