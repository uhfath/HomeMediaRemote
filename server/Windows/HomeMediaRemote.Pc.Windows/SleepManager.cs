using System.ComponentModel;
using System.Runtime.InteropServices;

namespace HomeMediaRemote.Pc.Windows
{
	/// <summary>
	/// Перевод компьютера в режим сна / гибернации через PowrProf.dll.
	/// </summary>
	public static class SleepManager
	{
		[DllImport("PowrProf.dll", SetLastError = true)]
		private static extern bool SetSuspendState(
			bool hibernate, bool forceCritical, bool disableWakeEvent);

		/// <summary>Переводит компьютер в спящий режим (S3 Sleep).</summary>
		public static void Sleep()
		{
			if (!SetSuspendState(hibernate: false, forceCritical: false, disableWakeEvent: false))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		/// <summary>Переводит компьютер в гибернацию (S4 Hibernate).</summary>
		public static void Hibernate()
		{
			if (!SetSuspendState(hibernate: true, forceCritical: false, disableWakeEvent: false))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}
}
