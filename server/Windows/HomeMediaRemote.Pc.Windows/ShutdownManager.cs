using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace HomeMediaRemote.Pc.Windows
{
	/// <summary>
	/// Выключение и перезагрузка через ExitWindowsEx.
	/// Автоматически запрашивает привилегию SeShutdownPrivilege.
	/// </summary>
	public static class ShutdownManager
	{
		// ── флаги ExitWindowsEx ──────────────────────────────────
		private const uint EWX_POWEROFF = 0x00000008;
		private const uint EWX_REBOOT = 0x00000002;
		private const uint EWX_FORCE = 0x00000004;   // закрыть приложения принудительно

		// ── привилегии ───────────────────────────────────────────
		private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
		private const uint TOKEN_QUERY = 0x0008;
		private const uint SE_PRIVILEGE_ENABLED = 0x00000002;
		private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

		#region Native structs

		[StructLayout(LayoutKind.Sequential)]
		private struct LUID
		{
			public uint LowPart;
			public int HighPart;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct LUID_AND_ATTRIBUTES
		{
			public LUID Luid;
			public uint Attributes;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct TOKEN_PRIVILEGES
		{
			public uint PrivilegeCount;
			public LUID_AND_ATTRIBUTES Privileges;     // одна запись — достаточно
		}

		#endregion

		#region P/Invoke

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool OpenProcessToken(
			nint processHandle, uint desiredAccess, out nint tokenHandle);

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool LookupPrivilegeValue(
			string? systemName, string name, out LUID luid);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool AdjustTokenPrivileges(
			nint tokenHandle, bool disableAll,
			ref TOKEN_PRIVILEGES newState, uint bufferLength,
			nint previousState, nint returnLength);

		[DllImport("kernel32.dll")]
		private static extern nint GetCurrentProcess();

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(nint handle);

		#endregion

		/// <summary>Запрашивает SeShutdownPrivilege для текущего процесса.</summary>
		private static void AcquireShutdownPrivilege()
		{
			if (!OpenProcessToken(GetCurrentProcess(),
					TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out nint token))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			try
			{
				if (!LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, out LUID luid))
					throw new Win32Exception(Marshal.GetLastWin32Error());

				var tp = new TOKEN_PRIVILEGES
				{
					PrivilegeCount = 1,
					Privileges = new LUID_AND_ATTRIBUTES
					{
						Luid = luid,
						Attributes = SE_PRIVILEGE_ENABLED
					}
				};

				bool ok = AdjustTokenPrivileges(token, false, ref tp, 0, nint.Zero, nint.Zero);
				int error = Marshal.GetLastWin32Error();

				if (!ok || error != 0)                          // 1300 = ERROR_NOT_ALL_ASSIGNED
					throw new Win32Exception(error,
						"Не удалось получить привилегию SeShutdownPrivilege. " +
						"Запустите приложение от имени администратора.");
			}
			finally
			{
				CloseHandle(token);
			}
		}

		/// <summary>Выключает компьютер.</summary>
		/// <param name="force">true — принудительно закрыть все приложения.</param>
		public static void Shutdown(bool force = false)
		{
			AcquireShutdownPrivilege();

			uint flags = EWX_POWEROFF | (force ? EWX_FORCE : 0u);
			if (!ExitWindowsEx(flags, 0))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		/// <summary>Перезагружает компьютер.</summary>
		/// <param name="force">true — принудительно закрыть все приложения.</param>
		public static void Restart(bool force = false)
		{
			AcquireShutdownPrivilege();

			uint flags = EWX_REBOOT | (force ? EWX_FORCE : 0u);
			if (!ExitWindowsEx(flags, 0))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}
}
