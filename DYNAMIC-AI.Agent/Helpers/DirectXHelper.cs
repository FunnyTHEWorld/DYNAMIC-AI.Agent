using System;
using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;
using WinRT;

namespace DYNAMIC_AI.Agent.Helpers;

public static class DirectXHelper
{
    [DllImport("d3d11.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int D3D11CreateDevice(
        IntPtr pAdapter,
        D3D_DRIVER_TYPE DriverType,
        IntPtr Software,
        uint Flags,
        IntPtr pFeatureLevels,
        uint FeatureLevels,
        uint SDKVersion,
        out IntPtr ppDevice,
        out D3D_FEATURE_LEVEL pFeatureLevel,
        out IntPtr ppImmediateContext);

    private enum D3D_DRIVER_TYPE
    {
        D3D_DRIVER_TYPE_UNKNOWN = 0,
        D3D_DRIVER_TYPE_HARDWARE = 1,
        D3D_DRIVER_TYPE_REFERENCE = 2,
        D3D_DRIVER_TYPE_NULL = 3,
        D3D_DRIVER_TYPE_SOFTWARE = 4,
        D3D_DRIVER_TYPE_WARP = 5
    }

    private enum D3D_FEATURE_LEVEL
    {
        D3D_FEATURE_LEVEL_9_1 = 0x9100,
        D3D_FEATURE_LEVEL_9_2 = 0x9200,
        D3D_FEATURE_LEVEL_9_3 = 0x9300,
        D3D_FEATURE_LEVEL_10_0 = 0xa000,
        D3D_FEATURE_LEVEL_10_1 = 0xa100,
        D3D_FEATURE_LEVEL_11_0 = 0xb000,
        D3D_FEATURE_LEVEL_11_1 = 0xb100
    }

    [ComImport]
    [Guid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    private interface IDirect3DDxgiInterfaceAccess
    {
        IntPtr GetInterface([In] ref Guid iid);
    }

    public static IDirect3DDevice CreateDevice()
    {
        int hr = D3D11CreateDevice(
            IntPtr.Zero,
            D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
            IntPtr.Zero,
            0x20, // D3D11_CREATE_DEVICE_BGRA_SUPPORT
            IntPtr.Zero,
            0,
            7, // D3D11_SDK_VERSION
            out IntPtr d3d11DevicePtr,
            out _,
            out IntPtr d3d11ImmediateContextPtr);

        if (hr != 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        var d3d11Device = Marshal.GetObjectForIUnknown(d3d11DevicePtr);
        Marshal.Release(d3d11DevicePtr);
        Marshal.Release(d3d11ImmediateContextPtr);

        var dxgiDeviceAccess = (IDirect3DDxgiInterfaceAccess)d3d11Device;
        var inspectable = dxgiDeviceAccess.GetInterface(new Guid("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90")); // IInspectable

        try
        {
            return Marshal.GetObjectForIUnknown(inspectable) as IDirect3DDevice;
        }
        finally
        {
            Marshal.Release(inspectable);
        }
    }
}
