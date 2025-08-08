using System;
using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;

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

    [DllImport("d3d11.dll", SetLastError = true)]
    private static extern uint CreateDirect3D11DeviceFromDXGIDevice(
        IntPtr dxgiDevice,
        out IntPtr graphicsDevice);

    private enum D3D_DRIVER_TYPE
    {
        D3D_DRIVER_TYPE_HARDWARE = 1,
    }

    private enum D3D_FEATURE_LEVEL
    {
        D3D_FEATURE_LEVEL_11_0 = 0xb000,
    }

    public static IDirect3DDevice CreateDevice()
    {
        IntPtr d3d11DevicePtr = IntPtr.Zero;
        IntPtr d3d11ImmediateContextPtr = IntPtr.Zero;
        IntPtr dxgiDevicePtr = IntPtr.Zero;
        IntPtr inspectableDevicePtr = IntPtr.Zero;

        try
        {
            int hr = D3D11CreateDevice(
                IntPtr.Zero,
                D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
                IntPtr.Zero,
                0x20, // D3D11_CREATE_DEVICE_BGRA_SUPPORT
                IntPtr.Zero,
                0,
                7, // D3D11_SDK_VERSION
                out d3d11DevicePtr,
                out _,
                out d3d11ImmediateContextPtr);
            if (hr != 0) Marshal.ThrowExceptionForHR(hr);

            Guid dxgiDeviceGuid = new Guid("770aae78-f26f-4dba-a829-253c83d1b387"); // IDXGIDevice
            hr = Marshal.QueryInterface(d3d11DevicePtr, ref dxgiDeviceGuid, out dxgiDevicePtr);
            if (hr != 0) Marshal.ThrowExceptionForHR(hr);

            uint hr2 = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevicePtr, out inspectableDevicePtr);
            if (hr2 != 0) Marshal.ThrowExceptionForHR((int)hr2);

            return Marshal.GetObjectForIUnknown(inspectableDevicePtr) as IDirect3DDevice;
        }
        finally
        {
            if (inspectableDevicePtr != IntPtr.Zero) Marshal.Release(inspectableDevicePtr);
            if (dxgiDevicePtr != IntPtr.Zero) Marshal.Release(dxgiDevicePtr);
            if (d3d11ImmediateContextPtr != IntPtr.Zero) Marshal.Release(d3d11ImmediateContextPtr);
            if (d3d11DevicePtr != IntPtr.Zero) Marshal.Release(d3d11DevicePtr);
        }
    }
}
