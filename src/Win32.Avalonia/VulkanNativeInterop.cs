using System.Runtime.InteropServices;
using Avalonia.Vulkan;

namespace Win32.Avalonia;

internal unsafe sealed class Win32VulkanInterface
{
    private readonly delegate* unmanaged[Cdecl]<nint, VkWin32SurfaceCreateInfoKhr*, nint, ulong*, int> _vkCreateWin32SurfaceKhr;

    public Win32VulkanInterface(IVulkanInstance instance)
    {
        var proc = instance.GetInstanceProcAddress(instance.Handle, "vkCreateWin32SurfaceKHR");
        if (proc == nint.Zero)
        {
            throw new InvalidOperationException("vkCreateWin32SurfaceKHR is unavailable.");
        }

        _vkCreateWin32SurfaceKhr = (delegate* unmanaged[Cdecl]<nint, VkWin32SurfaceCreateInfoKhr*, nint, ulong*, int>)proc;
    }

    public int vkCreateWin32SurfaceKHR(nint instance, ref VkWin32SurfaceCreateInfoKhr createInfo, nint allocator, out ulong surface)
    {
        fixed (VkWin32SurfaceCreateInfoKhr* createInfoPointer = &createInfo)
        fixed (ulong* surfacePointer = &surface)
        {
            return _vkCreateWin32SurfaceKhr(instance, createInfoPointer, allocator, surfacePointer);
        }
    }
}

internal struct VkWin32SurfaceCreateInfoKhr
{
    public const uint VkStructureTypeWin32SurfaceCreateInfoKhr = 1000009000;

    public uint sType;
    public nint pNext;
    public uint flags;
    public nint hinstance;
    public nint hwnd;
}