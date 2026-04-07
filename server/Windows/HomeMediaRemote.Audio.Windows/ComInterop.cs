using System;
using System.Runtime.InteropServices;

namespace HomeMediaRemote.Audio.Windows
{
	// ──────────────────────────────────────────────────────────
	//  Перечисления
	// ──────────────────────────────────────────────────────────

	public enum EDataFlow
	{
		eRender = 0,   // Устройство вывода (колонки, наушники)
		eCapture = 1,   // Устройство ввода  (микрофон)
		eAll = 2
	}

	internal enum ERole
	{
		eConsole = 0,
		eMultimedia = 1,
		eCommunications = 2
	}

	// ──────────────────────────────────────────────────────────
	//  PROPERTYKEY (для IMMNotificationClient.OnPropertyValueChanged)
	// ──────────────────────────────────────────────────────────

	[StructLayout(LayoutKind.Sequential)]
	internal struct PROPERTYKEY
	{
		public Guid fmtid;
		public int pid;
	}

	// ──────────────────────────────────────────────────────────
	//  COM-класс MMDeviceEnumerator
	// ──────────────────────────────────────────────────────────

	[ComImport]
	[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
	internal class MMDeviceEnumeratorCom { }

	// ──────────────────────────────────────────────────────────
	//  IMMDeviceEnumerator  (полный vtable)
	// ──────────────────────────────────────────────────────────

	[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMMDeviceEnumerator
	{
		// 0
		[PreserveSig]
		int EnumAudioEndpoints(
			EDataFlow dataFlow,
			int dwStateMask,
			out nint ppDevices);

		// 1
		[PreserveSig]
		int GetDefaultAudioEndpoint(
			EDataFlow dataFlow,
			ERole role,
			out IMMDevice ppEndpoint);

		// 2
		[PreserveSig]
		int GetDevice(
			[MarshalAs(UnmanagedType.LPWStr)] string pwstrId,
			out IMMDevice ppDevice);

		// 3
		[PreserveSig]
		int RegisterEndpointNotificationCallback(
			IMMNotificationClient pClient);

		// 4
		[PreserveSig]
		int UnregisterEndpointNotificationCallback(
			IMMNotificationClient pClient);
	}

	// ──────────────────────────────────────────────────────────
	//  IMMNotificationClient
	//  Порядок методов строго соответствует COM vtable.
	// ──────────────────────────────────────────────────────────

	[ComVisible(true)]
	[Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMMNotificationClient
	{
		// 0
		[PreserveSig]
		int OnDeviceStateChanged(
			[MarshalAs(UnmanagedType.LPWStr)] string deviceId,
			int newState);

		// 1
		[PreserveSig]
		int OnDeviceAdded(
			[MarshalAs(UnmanagedType.LPWStr)] string deviceId);

		// 2
		[PreserveSig]
		int OnDeviceRemoved(
			[MarshalAs(UnmanagedType.LPWStr)] string deviceId);

		// 3
		[PreserveSig]
		int OnDefaultDeviceChanged(
			EDataFlow flow,
			ERole role,
			[MarshalAs(UnmanagedType.LPWStr)] string? defaultDeviceId);

		// 4
		[PreserveSig]
		int OnPropertyValueChanged(
			[MarshalAs(UnmanagedType.LPWStr)] string deviceId,
			PROPERTYKEY key);
	}

	// ──────────────────────────────────────────────────────────
	//  IMMDevice
	// ──────────────────────────────────────────────────────────

	[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMMDevice
	{
		[PreserveSig]
		int Activate(
			ref Guid iid,
			int dwClsCtx,
			nint pActivationParams,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
	}

	// ──────────────────────────────────────────────────────────
	//  IAudioEndpointVolume
	//  Порядок методов СТРОГО соответствует vtable COM-интерфейса.
	// ──────────────────────────────────────────────────────────

	[Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioEndpointVolume
	{
		// 0
		[PreserveSig]
		int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);

		// 1
		[PreserveSig]
		int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);

		// 2
		[PreserveSig]
		int GetChannelCount(out int pnChannelCount);

		// 3
		[PreserveSig]
		int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);

		// 4
		[PreserveSig]
		int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);

		// 5
		[PreserveSig]
		int GetMasterVolumeLevel(out float pfLevelDB);

		// 6
		[PreserveSig]
		int GetMasterVolumeLevelScalar(out float pfLevel);

		// 7
		[PreserveSig]
		int SetChannelVolumeLevel(int nChannel, float fLevelDB, ref Guid pguidEventContext);

		// 8
		[PreserveSig]
		int SetChannelVolumeLevelScalar(int nChannel, float fLevel, ref Guid pguidEventContext);

		// 9
		[PreserveSig]
		int GetChannelVolumeLevel(int nChannel, out float pfLevelDB);

		// 10
		[PreserveSig]
		int GetChannelVolumeLevelScalar(int nChannel, out float pfLevel);

		// 11
		[PreserveSig]
		int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);

		// 12
		[PreserveSig]
		int GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);

		// 13
		[PreserveSig]
		int GetVolumeStepInfo(out int pnStep, out int pnStepCount);

		// 14
		[PreserveSig]
		int VolumeStepUp(ref Guid pguidEventContext);

		// 15
		[PreserveSig]
		int VolumeStepDown(ref Guid pguidEventContext);

		// 16
		[PreserveSig]
		int QueryHardwareSupport(out int pdwHardwareSupportMask);

		// 17
		[PreserveSig]
		int GetVolumeRange(
			out float pflVolumeMindB,
			out float pflVolumeMaxdB,
			out float pflVolumeIncrementdB);
	}

	// ──────────────────────────────────────────────────────────
	//  AUDIO_VOLUME_NOTIFICATION_DATA
	// ──────────────────────────────────────────────────────────

	[StructLayout(LayoutKind.Sequential)]
	internal struct AUDIO_VOLUME_NOTIFICATION_DATA
	{
		public Guid guidEventContext;
		[MarshalAs(UnmanagedType.Bool)]
		public bool bMuted;
		public float fMasterVolume;
		public uint nChannels;
		public float afChannelVolumes;
	}

	// ──────────────────────────────────────────────────────────
	//  IAudioEndpointVolumeCallback
	// ──────────────────────────────────────────────────────────

	[ComVisible(true)]
	[Guid("657804FA-D6AD-4496-8A60-352752AF4F89")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioEndpointVolumeCallback
	{
		[PreserveSig]
		int OnNotify(nint pNotifyData);
	}

	// ──────────────────────────────────────────────────────────
	//  Фабричный хелпер: получает IAudioEndpointVolume
	//  для нужного типа устройства
	// ──────────────────────────────────────────────────────────

	internal static class EndpointVolumeFactory
	{
		private static readonly Guid IID_IAudioEndpointVolume =
			new("5CDF2C82-841E-4546-9722-0CF74078229A");

		private const int CLSCTX_ALL = 0x17;   // INPROC_SERVER | INPROC_HANDLER | LOCAL_SERVER | REMOTE_SERVER

		/// <summary>Создаёт собственный enumerator, использует и освобождает его.</summary>
		internal static IAudioEndpointVolume Create(EDataFlow flow)
		{
			var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorCom();
			try
			{
				return Create(flow, enumerator);
			}
			finally
			{
				Marshal.ReleaseComObject(enumerator);
			}
		}

		/// <summary>Использует переданный enumerator без освобождения.</summary>
		internal static IAudioEndpointVolume Create(EDataFlow flow, IMMDeviceEnumerator enumerator)
		{
			Marshal.ThrowExceptionForHR(enumerator.GetDefaultAudioEndpoint(flow, ERole.eMultimedia, out var device));

			try
			{
				var iid = IID_IAudioEndpointVolume;
				Marshal.ThrowExceptionForHR(device.Activate(ref iid, CLSCTX_ALL, nint.Zero, out var activated));
				return (IAudioEndpointVolume)activated;
			}
			finally
			{
				Marshal.ReleaseComObject(device);
			}
		}
	}
}
