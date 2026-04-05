using System.Collections.Concurrent;

namespace HomeMediaRemote.Status.Windows
{
	public class TrayIconManager : IDisposable
	{
		private readonly ConcurrentDictionary<string, TrayIcon> _trayIcons = new();
		private readonly FormManager _formManager;
		private bool _isDisposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					foreach (var icon in _trayIcons.Values)
					{
						_formManager.Invoke(icon.Dispose);
					}
				}

				_isDisposed = true;
			}
		}

		private TrayIcon BuildTrayIcon(Bitmap bitmap, string tooltip, Action clickAction)
		{
			return _formManager.Invoke(() => new TrayIcon(bitmap, tooltip, clickAction));
		}

		public TrayIconManager(
			FormManager formManager)
		{
			this._formManager = formManager;
		}

		public void AddTrayIcon(string key, Bitmap bitmap, string tooltip, Action clickAction)
		{
			_trayIcons.AddOrUpdate(key, k => BuildTrayIcon(bitmap, tooltip, clickAction), (_, o) => o);
		}

		public void RemoveTrayIcon(string key)
		{
			if (_trayIcons.TryRemove(key, out var icon))
			{
				_formManager.Invoke(icon.Dispose);
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
