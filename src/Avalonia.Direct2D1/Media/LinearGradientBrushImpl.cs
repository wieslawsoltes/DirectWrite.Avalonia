using System.Linq;
using Avalonia.Media;

namespace Avalonia.Direct2D1.Media
{
    internal class LinearGradientBrushImpl : BrushImpl
    {
        public LinearGradientBrushImpl(
            ILinearGradientBrush brush,
            Avalonia.Direct2D1.Interop.Direct2D1.RenderTarget target,
            Rect destinationRect)
        {
            if (brush.GradientStops.Count == 0)
            {
                return;
            }

            var gradientStops = brush.GradientStops.Select(s => new Avalonia.Direct2D1.Interop.Direct2D1.GradientStop
            {
                Color = s.Color.ToDirect2D(),
                Position = (float)s.Offset
            }).ToArray();

            var startPoint = brush.StartPoint.ToPixels(destinationRect);
            var endPoint = brush.EndPoint.ToPixels(destinationRect);

            using (var stops = new Avalonia.Direct2D1.Interop.Direct2D1.GradientStopCollection(
                target,
                gradientStops,
                brush.SpreadMethod.ToDirect2D()))
            {
                PlatformBrush = new Avalonia.Direct2D1.Interop.Direct2D1.LinearGradientBrush(
                    target,
                    new Avalonia.Direct2D1.Interop.Direct2D1.LinearGradientBrushProperties
                    {
                        StartPoint = startPoint.ToInterop(),
                        EndPoint = endPoint.ToInterop()
                    },
                    new Avalonia.Direct2D1.Interop.Direct2D1.BrushProperties
                    {
                        Opacity = (float)brush.Opacity,
                        Transform = PrimitiveExtensions.Matrix3x2Identity,
                    },
                    stops);
            }
        }
    }
}
