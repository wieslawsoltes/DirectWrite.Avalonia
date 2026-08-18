using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using Avalonia.Vulkan;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Win32.Avalonia;

internal static unsafe class VulkanSupport
{
    private static readonly nint s_vulkanLibrary = NativeLibrary.Load("vulkan-1.dll");
    private static readonly delegate* unmanaged[Cdecl]<nint, byte*, nint> s_vkGetInstanceProcAddr =
        (delegate* unmanaged[Cdecl]<nint, byte*, nint>)NativeLibrary.GetExport(s_vulkanLibrary, "vkGetInstanceProcAddr");

    public static VulkanPlatformGraphics? TryInitialize(VulkanOptions options) =>
        VulkanPlatformGraphics.TryCreate(options ?? new(), new VulkanPlatformSpecificOptions
        {
            RequiredInstanceExtensions = { "VK_KHR_win32_surface" },
            GetProcAddressDelegate = VkGetInstanceProcAddr,
            DeviceCheckSurfaceFactory = instance => CreateHwndSurface(OffscreenParentWindow.Handle, instance),
            PlatformFeatures = new Dictionary<Type, object>
            {
                [typeof(IVulkanKhrSurfacePlatformSurfaceFactory)] = new VulkanSurfaceFactory()
            }
        });

    private sealed class VulkanSurfaceFactory : IVulkanKhrSurfacePlatformSurfaceFactory
    {
        public bool CanRenderToSurface(IVulkanPlatformGraphicsContext context, IPlatformRenderSurface surface)
            => surface is INativePlatformHandleSurface handle && handle.HandleDescriptor == "HWND";

        public IVulkanKhrSurfacePlatformSurface CreateSurface(IVulkanPlatformGraphicsContext context, IPlatformRenderSurface handle)
            => new HwndVulkanSurface((INativePlatformHandleSurface)handle);
    }

    private sealed class HwndVulkanSurface(INativePlatformHandleSurface handle) : IVulkanKhrSurfacePlatformSurface
    {
        private readonly INativePlatformHandleSurface _handle = handle;

        public void Dispose()
        {
        }

        public double Scaling => _handle.Scaling;

        public PixelSize Size => _handle.Size;

        public ulong CreateSurface(IVulkanPlatformGraphicsContext context)
            => CreateHwndSurface(_handle.Handle, context.Instance);
    }

    private static ulong CreateHwndSurface(nint window, IVulkanInstance instance)
    {
        var vulkanWin32 = new Win32VulkanInterface(instance);
        var createInfo = new VkWin32SurfaceCreateInfoKhr
        {
            sType = VkWin32SurfaceCreateInfoKhr.VkStructureTypeWin32SurfaceCreateInfoKhr,
            hinstance = Marshal.GetHINSTANCE(typeof(VulkanSupport).Module),
            hwnd = window,
        };

        VulkanException.ThrowOnError(
            "vkCreateWin32SurfaceKHR",
            vulkanWin32.vkCreateWin32SurfaceKHR(instance.Handle, ref createInfo, nint.Zero, out var surface));
        return surface;
    }

    private static nint VkGetInstanceProcAddr(nint instance, string name)
    {
        var bytes = Encoding.UTF8.GetBytes(name + '\0');
        fixed (byte* namePointer = bytes)
        {
            return s_vkGetInstanceProcAddr(instance, namePointer);
        }
    }
}