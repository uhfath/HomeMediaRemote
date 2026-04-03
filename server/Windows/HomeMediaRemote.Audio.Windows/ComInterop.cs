using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HomeMediaRemote.Audio.Windows
{
	// ──────────────────────────────────────────────────────────
	//  Перечисления
	// ──────────────────────────────────────────────────────────

	internal enum EDataFlow
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
	//  COM-класс MMDeviceEnumerator
	// ──────────────────────────────────────────────────────────

	[ComImport]
	[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
	internal class MMDeviceEnumeratorCom { }

	// ──────────────────────────────────────────────────────────
	//  IMMDeviceEnumerator
	// ──────────────────────────────────────────────────────────

	[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMMDeviceEnumerator
	{
		[PreserveSig]
		int EnumAudioEndpoints(
			EDataFlow dataFlow,
			int dwStateMask,
			out nint ppDevices);

		[PreserveSig]
		int GetDefaultAudioEndpoint(
			EDataFlow dataFlow,
			ERole role,
			out IMMDevice ppEndpoint);
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
	//  Переставлять/удалять методы нельзя!
	// ──────────────────────────────────────────────────────────

	[Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioEndpointVolume
	{
		// 0
		[PreserveSig]
		int RegisterControlChangeNotify(nint pNotify);

		// 1
		[PreserveSig]
		int UnregisterControlChangeNotify(nint pNotify);

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
	//  Фабричный хелпер: получает IAudioEndpointVolume
	//  для нужного типа устройства
	// ──────────────────────────────────────────────────────────

	internal static class EndpointVolumeFactory
	{
		private static readonly Guid IID_IAudioEndpointVolume =
			new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");

		private const int CLSCTX_ALL = 0x17;   // INPROC_SERVER | INPROC_HANDLER | LOCAL_SERVER | REMOTE_SERVER

		/// <summary>
		/// Возвращает COM-объект IAudioEndpointVolume для устройства по умолчанию.
		/// </summary>
		/// <exception cref="COMException">
		/// 0x80070490 (E_NOTFOUND) — устройство данного типа отсутствует.
		/// </exception>
		[SupportedOSPlatform("windows")]
		internal static IAudioEndpointVolume Create(EDataFlow flow)
		{
			var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorCom();
			try
			{
				Marshal.ThrowExceptionForHR(
					enumerator.GetDefaultAudioEndpoint(flow, ERole.eMultimedia, out var device));
				try
				{
					var iid = IID_IAudioEndpointVolume;
					Marshal.ThrowExceptionForHR(
						device.Activate(ref iid, CLSCTX_ALL, nint.Zero, out var activated));
					return (IAudioEndpointVolume)activated;
				}
				finally
				{
					Marshal.ReleaseComObject(device);
				}
			}
			finally
			{
				Marshal.ReleaseComObject(enumerator);
			}
		}
	}
}
