using System.Collections.Concurrent;

namespace HomeMediaRemote.Status.Windows
{
	public class IconManager : IDisposable
	{
		private readonly ConcurrentDictionary<string, Bitmap> _icons = new();
		private bool _isDisposed;

		private static string BuildIconCacheKey(string svgPath, Size size, bool addShadow) =>
			$"{svgPath}_{size}_{addShadow}";

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					foreach (var icon in _icons.Values)
					{
						icon.Dispose();
					}
				}

				_isDisposed = true;
			}
		}

		private static Bitmap BuildBitmap(string svgPath, Size size, bool addShadow)
		{
			return SvgRenderer.RenderToBitmap(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, svgPath)), size, addShadow);
		}

		public Bitmap GetIcon(string svgPath, Size size, bool addShadow)
		{
			return _icons.GetOrAdd(BuildIconCacheKey(svgPath, size, addShadow), _ => BuildBitmap(svgPath, size, addShadow));
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
