using System.Runtime.InteropServices;

namespace HomeMediaRemote.Status.Windows;

/// <summary>
/// Минимальный набор Win32-функций для layered-окна и иконок.
/// </summary>
internal static class NativeMethods
{
	// ── константы ────────────────────────────────────────────
	public const byte AC_SRC_OVER = 0;
	public const byte AC_SRC_ALPHA = 1;
	public const int ULW_ALPHA = 0x00000002;

	public const int SM_CXSMICON = 49;
	public const int SM_CYSMICON = 50;

	public const uint SWP_NOSIZE = 0x0001;
	public const uint SWP_NOZORDER = 0x0004;
	public const uint SWP_NOACTIVATE = 0x0010;

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT(int x, int y)
	{
		public int X = x, Y = y;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SIZE(int cx, int cy)
	{
		public int CX = cx, CY = cy;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct BLENDFUNCTION
	{
		public byte BlendOp;
		public byte BlendFlags;
		public byte SourceConstantAlpha;
		public byte AlphaFormat;
	}

	[DllImport("gdi32.dll", SetLastError = true)]
	public static extern nint CreateCompatibleDC(nint hDC);

	[DllImport("gdi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeleteDC(nint hdc);

	[DllImport("gdi32.dll", SetLastError = true)]
	public static extern nint SelectObject(nint hDC, nint hObject);

	[DllImport("gdi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeleteObject(nint hObject);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern nint GetDC(nint hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int ReleaseDC(nint hWnd, nint hDC);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool UpdateLayeredWindow(
		nint hwnd,
		nint hdcDst,
		ref POINT pptDst,
		ref SIZE psize,
		nint hdcSrc,
		ref POINT pptSrc,
		int crKey,
		ref BLENDFUNCTION pblend,
		int dwFlags);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DestroyIcon(nint hIcon);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int GetSystemMetricsForDpi(int nIndex, uint dpi);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern uint GetDpiForSystem();

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool SetWindowPos(
		nint hWnd, nint hWndInsertAfter,
		int X, int Y, int cx, int cy,
		uint uFlags);
}
