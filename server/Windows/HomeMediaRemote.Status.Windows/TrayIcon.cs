namespace HomeMediaRemote.Status.Windows;

/// <summary>
/// Управление иконкой в системном трее (notification area).
/// </summary>
internal sealed class TrayIcon : IDisposable
{
	private readonly NotifyIcon _notifyIcon;
	private readonly Icon _icon;
	private readonly EventHandler _iconAction;
	private bool _isDisposed;

	/// <summary>
	/// Bitmap → управляемый Icon.
	/// GetHicon() возвращает неуправляемый HICON — оборачиваем
	/// через Clone(), затем сразу освобождаем дескриптор.
	/// </summary>
	private static Icon BitmapToManagedIcon(Bitmap bitmap)
	{
		var hIcon = bitmap.GetHicon();
		try
		{
			using var tmp = Icon.FromHandle(hIcon);   // не владеет hIcon
			return (Icon)tmp.Clone();                  // полностью управляемая копия
		}
		finally
		{
			NativeMethods.DestroyIcon(hIcon);
		}
	}

	private void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_notifyIcon.Click -= _iconAction;
				_notifyIcon.Visible = false;
				_notifyIcon.Dispose();

				_icon.Dispose();
			}

			_isDisposed = true;
		}
	}

	public TrayIcon(Bitmap bitmap, string tooltip, Action iconAction)
	{
		_icon = BitmapToManagedIcon(bitmap);
		_iconAction = (_, __) =>
		{
			iconAction();
		};

		_notifyIcon = new NotifyIcon
		{
			Icon = _icon,
			Text = tooltip,
			Visible = true,
		};

		_notifyIcon.Click += _iconAction;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
