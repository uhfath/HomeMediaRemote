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
		private readonly TrayIconManager _trayIconManager;
		private readonly IconManager _iconManager;

		private static Size CalculateOverlayIconSize(Screen monitor)
		{
			var minSide = Math.Min(monitor.Bounds.Width, monitor.Bounds.Height);
			var target = (int)Math.Round(minSide * OverlayScreenSizePercent);

			return new Size(target, target);
		}

		private static Size CalculateTrayIconSize()
		{
			var dpi = NativeMethods.GetDpiForSystem();
			var width = NativeMethods.GetSystemMetricsForDpi(NativeMethods.SM_CXSMICON, dpi);
			var height = NativeMethods.GetSystemMetricsForDpi(NativeMethods.SM_CYSMICON, dpi);

			return new Size(width, height);
		}

		public IconStatusManager(
			ScreenOverlayManager screenOverlayManager,
			TrayIconManager trayIconManager,
			IconManager iconManager)
		{
			this._screenOverlayManager = screenOverlayManager;
			this._trayIconManager = trayIconManager;
			this._iconManager = iconManager;
		}

		public void ShowMicOffStatus(Action clickAction)
		{
			if (Screen.PrimaryScreen is null)
			{
				throw new InvalidOperationException("Primary screen not detected.");
			}

			var overlayIconSize = CalculateOverlayIconSize(Screen.PrimaryScreen);
			var overlayIcon = _iconManager.GetIcon(MicOffOverlayIconPath, overlayIconSize, true);
			_screenOverlayManager.AddScreenOverlay(MicOffOverlayStatusKey, overlayIcon);

			var trayIconSize = CalculateTrayIconSize();
			var trayIcon = _iconManager.GetIcon(MicOffTrayIconPath, trayIconSize, false);
			_trayIconManager.AddTrayIcon(MicOffTrayStatusKey, trayIcon, "Микрофон отключен", clickAction);
		}

		public void HideMicOffStatus()
		{
			_screenOverlayManager.RemoveScreenOverlay(MicOffOverlayStatusKey);
			_trayIconManager.RemoveTrayIcon(MicOffTrayStatusKey);
		}

		public void ShowNoSoundStatus(Action clickAction)
		{
			if (Screen.PrimaryScreen is null)
			{
				throw new InvalidOperationException("Primary screen not detected.");
			}

			var overlayIconSize = CalculateOverlayIconSize(Screen.PrimaryScreen);
			var overlayIcon = _iconManager.GetIcon(NoSoundOverlayIconPath, overlayIconSize, true);
			_screenOverlayManager.AddScreenOverlay(NoSoundOverlayStatusKey, overlayIcon);

			var trayIconSize = CalculateTrayIconSize();
			var trayIcon = _iconManager.GetIcon(NoSoundTrayIconPath, trayIconSize, false);
			_trayIconManager.AddTrayIcon(NoSoundTrayStatusKey, trayIcon, "Звук отключен", clickAction);
		}

		public void HideNoSoundStatus()
		{
			_screenOverlayManager.RemoveScreenOverlay(NoSoundOverlayStatusKey);
			_trayIconManager.RemoveTrayIcon(NoSoundTrayStatusKey);
		}
	}
}
