using System.ComponentModel;
using System.Runtime.InteropServices;

namespace HomeMediaRemote.Pc.Windows
{
	/// <summary>
	/// Блокировка текущей учётной записи Windows (Win+L).
	/// </summary>
	public static class SessionManager
	{
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool LockWorkStation();

		/// <summary>Блокирует рабочую станцию.</summary>
		public static void Lock()
		{
			if (!LockWorkStation())
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}
}
