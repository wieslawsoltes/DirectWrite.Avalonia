using System.Threading;
using Avalonia.Controls;

namespace Avalonia.Direct2D1.UnitTests;

internal static class Direct2D1TestServices
{
    private static int s_initialized;

    public static void Initialize()
    {
        if (Interlocked.Exchange(ref s_initialized, 1) == 1)
        {
            return;
        }

        AppBuilder.Configure<Application>()
            .UseStandardRuntimePlatformSubsystem()
            .UseDirect2D1()
            .UseWindowingSubsystem(() => { }, "Test")
            .SetupWithoutStarting();
    }
}
