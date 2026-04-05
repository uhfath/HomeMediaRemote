using System.Collections.Concurrent;

namespace HomeMediaRemote.Status.Windows
{
	public class ScreenOverlayManager : IDisposable
	{
		private const int OverlaySpacing = 10;

		private readonly ConcurrentDictionary<string, IReadOnlyList<ScreenOverlay>> _screenOverlays = new();
		private readonly List<GridLayout> _screenGrids = new();
		private readonly FormManager _formManager;
		private bool _isDisposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					foreach (var overlays in _screenOverlays.Values)
					{
						foreach(var overlay in overlays)
						{
							_formManager.Invoke(overlay.Dispose);
						}
					}

				}

				_isDisposed = true;
			}
		}

		private IReadOnlyList<ScreenOverlay> BuildScreenOverlays(string key, Bitmap bitmap)
		{
			var overlays = new List<ScreenOverlay>();
			foreach (var grid in _screenGrids)
			{
				var location = grid.GetNextTileLocation(bitmap.Size);
				var overlay = _formManager.Invoke(() => new ScreenOverlay(bitmap, location));
				grid.AddTile(key, bitmap.Size, p => _formManager.Invoke(() => overlay.MoveTo(p)));
				overlays.Add(overlay);
			}

			return overlays;
		}

		public ScreenOverlayManager(
			FormManager formManager)
		{
			this._formManager = formManager;

			_screenGrids.AddRange(Screen.AllScreens
				.Select(s => new GridLayout(s.Bounds, new Point(OverlaySpacing, OverlaySpacing)))
			);
		}

		public void AddScreenOverlay(string key, Bitmap bitmap)
		{
			_screenOverlays.AddOrUpdate(key, k => BuildScreenOverlays(k, bitmap), (_, o) => o);
		}

		public void RemoveScreenOverlay(string key)
		{
			if (_screenOverlays.TryRemove(key, out var overlays))
			{
				foreach (var overlay in overlays)
				{
					_formManager.Invoke(overlay.Dispose);
				}

				foreach (var grid in _screenGrids)
				{
					grid.RemoveTile(key);
				}
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
