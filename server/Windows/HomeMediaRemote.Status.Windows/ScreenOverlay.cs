using System.Windows.Forms;

namespace HomeMediaRemote.Status.Windows;

/// <summary>
/// Полностью прозрачное, неинтерактивное, topmost-окно,
/// которое рисует произвольный 32bpp-ARGB bitmap поверх всех окон.
/// <para>
///   • Не мерцает — используется <c>UpdateLayeredWindow</c>.<br/>
///   • Click-through (WS_EX_TRANSPARENT) — клики «проваливаются» насквозь.<br/>
///   • Не появляется в Alt+Tab и на панели задач.
/// </para>
/// </summary>
internal sealed class ScreenOverlay : IDisposable
{
	private readonly OverlayForm _overlayForm;
	private bool _isDisposed;

	private void MoveLayered(Point location)
	{
		NativeMethods.SetWindowPos(
			_overlayForm.Handle,
			nint.Zero,
			location.X, location.Y,
			0, 0,
			NativeMethods.SWP_NOSIZE
			| NativeMethods.SWP_NOZORDER
			| NativeMethods.SWP_NOACTIVATE);
	}

	/// <summary>
	/// Обновляет содержимое layered-окна атомарно (без
	/// промежуточной перерисовки → без мерцания).
	/// </summary>
	private void UpdateLayered(Bitmap bitmap, Point location)
	{
		var screenDc = nint.Zero;
		var memDc = nint.Zero;
		var hBitmap = nint.Zero;
		var prev = nint.Zero;

		try
		{
			screenDc = NativeMethods.GetDC(nint.Zero);
			if (screenDc == nint.Zero)
			{
				throw new InvalidOperationException("GetDC failed.");
			}

			memDc = NativeMethods.CreateCompatibleDC(screenDc);
			if (memDc == nint.Zero)
			{
				throw new InvalidOperationException("CreateCompatibleDC failed.");
			}

			hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
			prev = NativeMethods.SelectObject(memDc, hBitmap);

			var dst = new NativeMethods.POINT(location.X, location.Y);
			var size = new NativeMethods.SIZE(bitmap.Width, bitmap.Height);
			var src = new NativeMethods.POINT(0, 0);
			var blend = new NativeMethods.BLENDFUNCTION
			{
				BlendOp = NativeMethods.AC_SRC_OVER,
				BlendFlags = 0,
				SourceConstantAlpha = 255,
				AlphaFormat = NativeMethods.AC_SRC_ALPHA
			};

			NativeMethods.UpdateLayeredWindow(
				_overlayForm.Handle, screenDc,
				ref dst, ref size,
				memDc, ref src,
				0, ref blend, NativeMethods.ULW_ALPHA);
		}
		finally
		{
			// Освобождаем только то, что было реально создано
			if (prev != nint.Zero)
				NativeMethods.SelectObject(memDc, prev);

			if (hBitmap != nint.Zero)
				NativeMethods.DeleteObject(hBitmap);

			if (memDc != nint.Zero)
				NativeMethods.DeleteDC(memDc);

			if (screenDc != nint.Zero)
				NativeMethods.ReleaseDC(nint.Zero, screenDc);
		}
	}

	private void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_overlayForm.Close();
				_overlayForm.Dispose();
			}

			_isDisposed = true;
		}
	}

	public ScreenOverlay(Bitmap bitmap, Point location)
	{
		_overlayForm = new OverlayForm
		{
			Bounds = new Rectangle(location, bitmap.Size),
			Visible = true,
		};

		UpdateLayered(bitmap, location);
	}

	public int Width => _overlayForm.Width;
	public int Height => _overlayForm.Height;

	public void MoveTo(Point location)
	{
		_overlayForm.Location = location;
		MoveLayered(location);
	}

	// ── вложенная форма ──────────────────────────────────────

	private sealed class OverlayForm : Form
	{
		// Extended window styles
		private const int WS_EX_LAYERED = 0x0008_0000;
		private const int WS_EX_TRANSPARENT = 0x0000_0020; // click-through
		private const int WS_EX_TOOLWINDOW = 0x0000_0080; // нет в Alt+Tab
		private const int WS_EX_TOPMOST = 0x0000_0008;
		private const int WS_EX_NOACTIVATE = 0x0800_0000; // не крадёт фокус

		public OverlayForm()
		{
			FormBorderStyle = FormBorderStyle.None;
			ShowInTaskbar = false;
			TopMost = true;
			StartPosition = FormStartPosition.Manual;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.ExStyle |= WS_EX_LAYERED
							| WS_EX_TRANSPARENT
							| WS_EX_TOOLWINDOW
							| WS_EX_TOPMOST
							| WS_EX_NOACTIVATE;
				return cp;
			}
		}

		// Отключаем стандартную отрисовку — всё делает UpdateLayeredWindow.
		protected override void OnPaintBackground(PaintEventArgs e) { }
		protected override void OnPaint(PaintEventArgs e) { }
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
