using System;
using System.Runtime.InteropServices;

namespace HomeMediaRemote.Media.Windows
{
	/// <summary>
	/// Коды виртуальных медиа-клавиш Windows.
	/// https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
	/// </summary>
	internal static class VirtualKey
	{
		public const byte MediaPlayPause = 0xB3; // VK_MEDIA_PLAY_PAUSE (toggle)
		public const byte MediaNextTrack = 0xB0; // VK_MEDIA_NEXT_TRACK
		public const byte MediaPrevTrack = 0xB1; // VK_MEDIA_PREV_TRACK
		public const byte MediaStop = 0xB2; // VK_MEDIA_STOP
	}

	/// <summary>
	/// Низкоуровневая обёртка над WinAPI для отправки нажатий клавиш.
	/// </summary>
	internal static class NativeMethods
	{
		// --- Способ 1: keybd_event (простой, для медиа-клавиш работает отлично) ---

		private const uint KEYEVENTF_KEYDOWN = 0x0000;
		private const uint KEYEVENTF_KEYUP = 0x0002;

		[DllImport("user32.dll", SetLastError = true)]
		private static extern void keybd_event(
			byte bVk,
			byte bScan,
			uint dwFlags,
			nuint dwExtraInfo);

		/// <summary>
		/// Эмулирует нажатие и отпускание виртуальной клавиши.
		/// </summary>
		public static void PressKey(byte virtualKeyCode)
		{
			keybd_event(virtualKeyCode, 0, KEYEVENTF_KEYDOWN, nuint.Zero);
			keybd_event(virtualKeyCode, 0, KEYEVENTF_KEYUP, nuint.Zero);
		}

		// --- Способ 2: SendMessage + WM_APPCOMMAND (позволяет отдельно Play и Pause) ---

		private const int WM_APPCOMMAND = 0x0319;

		// APPCOMMAND коды (значение сдвигается на 16 бит в lParam)
		// https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-appcommand
		public const int APPCOMMAND_MEDIA_PLAY = 46;
		public const int APPCOMMAND_MEDIA_PAUSE = 47;
		public const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;
		public const int APPCOMMAND_MEDIA_NEXTTRACK = 11;
		public const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 12;
		public const int APPCOMMAND_MEDIA_STOP = 13;

		[DllImport("user32.dll", SetLastError = true)]
		private static extern nint SendMessageW(
			nint hWnd,
			uint msg,
			nint wParam,
			nint lParam);

		[DllImport("user32.dll")]
		private static extern nint GetForegroundWindow();

		/// <summary>
		/// Отправляет WM_APPCOMMAND текущему активному окну (или shell).
		/// </summary>
		public static void SendAppCommand(int appCommand)
		{
			// Отправляем shell-окну (0xFFFF = HWND_BROADCAST тоже работает,
			// но GetForegroundWindow надёжнее для медиа-команд)
			nint hwnd = GetForegroundWindow();
			nint lParam = appCommand << 16;
			SendMessageW(hwnd, WM_APPCOMMAND, hwnd, lParam);
		}
	}

	/// <summary>
	/// Высокоуровневый контроллер медиа-воспроизведения.
	/// Не требует внешних библиотек — только WinAPI через P/Invoke.
	/// </summary>
	public static class MediaController
	{
		// ────────────────────────────────────────────
		//  Основные методы (через keybd_event)
		//  Работают глобально, как нажатие клавиши
		//  на мультимедийной клавиатуре.
		// ────────────────────────────────────────────

		/// <summary>
		/// Play / Pause — переключение (toggle).
		/// Аналог клавиши ⏯ на клавиатуре.
		/// </summary>
		public static void PlayPauseToggle()
		{
			NativeMethods.PressKey(VirtualKey.MediaPlayPause);
		}

		/// <summary>
		/// Следующий трек. Аналог клавиши ⏭.
		/// </summary>
		public static void NextTrack()
		{
			NativeMethods.PressKey(VirtualKey.MediaNextTrack);
		}

		/// <summary>
		/// Предыдущий трек. Аналог клавиши ⏮.
		/// </summary>
		public static void PreviousTrack()
		{
			NativeMethods.PressKey(VirtualKey.MediaPrevTrack);
		}

		/// <summary>
		/// Остановить воспроизведение. Аналог клавиши ⏹.
		/// </summary>
		public static void Stop()
		{
			NativeMethods.PressKey(VirtualKey.MediaStop);
		}

		// ────────────────────────────────────────────
		//  Дополнительные методы (через WM_APPCOMMAND)
		//  Позволяют отдельно вызвать Play и Pause,
		//  а не только toggle.
		// ────────────────────────────────────────────

		/// <summary>
		/// Только Play (без toggle). Через WM_APPCOMMAND.
		/// </summary>
		public static void Play()
		{
			NativeMethods.SendAppCommand(NativeMethods.APPCOMMAND_MEDIA_PLAY);
		}

		/// <summary>
		/// Только Pause (без toggle). Через WM_APPCOMMAND.
		/// </summary>
		public static void Pause()
		{
			NativeMethods.SendAppCommand(NativeMethods.APPCOMMAND_MEDIA_PAUSE);
		}
	}
}
