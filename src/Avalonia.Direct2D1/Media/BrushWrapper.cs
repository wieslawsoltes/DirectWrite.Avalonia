using Avalonia.Media;
namespace Avalonia.Direct2D1.Media
{
    internal class BrushWrapper
    {
        public BrushWrapper(IBrush brush)
        {
            Brush = brush;
        }

        public IBrush Brush { get; private set; }
    }
}
