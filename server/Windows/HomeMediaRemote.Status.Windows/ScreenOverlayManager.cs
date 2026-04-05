using System.Collections.Concurrent;

namespace HomeMediaRemote.Status.Windows
{
	public class ScreenOverlayManager : IDisposable
	{
		private const int OverlaySpacing = 10;

		private readonly ConcurrentDictionary<string, IReadOnlyList<ScreenOverlay>> _screenOverlays = new();
		private readonly List<GridLayout> _screenGrids = new();

		private Form _marshalForm = null!;
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
							_marshalForm.Invoke(overlay.Dispose);
						}
					}

					_marshalForm.Invoke(Application.ExitThread);
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
				var overlay = _marshalForm.Invoke(() => new ScreenOverlay(bitmap, location));
				grid.AddTile(key, bitmap.Size, p => _marshalForm.Invoke(() => overlay.MoveTo(p)));
				overlays.Add(overlay);
			}

			return overlays;
		}

		public ScreenOverlayManager()
		{
			var isFormReady = new ManualResetEventSlim();

			var uiThread = new Thread(() =>
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				// Невидимая форма — единственная цель: иметь HWND,
				// через который Invoke перебрасывает делегаты в этот поток.
				_marshalForm = new Form
				{
					FormBorderStyle = FormBorderStyle.None,
					ShowInTaskbar = false,
					Size = Size.Empty
				};
				
				_ = _marshalForm.Handle;
				isFormReady.Set();

				Application.Run();
			});

			uiThread.SetApartmentState(ApartmentState.STA); // WinForms требует STA
			uiThread.IsBackground = true;
			uiThread.Start();

			_screenGrids.AddRange(Screen.AllScreens
				.Select(s => new GridLayout(s.Bounds, new Point(OverlaySpacing, OverlaySpacing)))
			);

			isFormReady.Wait(); // ждём, пока HWND готов
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
					_marshalForm.Invoke(overlay.Dispose);
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
