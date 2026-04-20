using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;

namespace ControlCatalog.Direct2D1;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Width = 900;
        Height = 600;
        Title = "Direct2D1.Avalonia Sample";

        Content = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 16,
            Children =
            {
                new TextBlock
                {
                    Text = "Direct2D1 + DirectWrite backend",
                    FontSize = 28,
                    FontWeight = FontWeight.Bold
                },
                new TextBlock
                {
                    Text = "This sample is intended to validate the standalone rendering backend wiring.",
                    TextWrapping = TextWrapping.Wrap
                },
                new Border
                {
                    Background = Brushes.WhiteSmoke,
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(20),
                    Child = new StackPanel
                    {
                        Spacing = 12,
                        Children =
                        {
                            new Button
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Content = "Button"
                            },
                            new TextBox
                            {
                                PlaceholderText = "Type here"
                            },
                            new Canvas
                            {
                                Height = 160,
                                Children =
                                {
                                    new Rectangle
                                    {
                                        Width = 180,
                                        Height = 100,
                                        RadiusX = 18,
                                        RadiusY = 18,
                                        Fill = Brushes.CornflowerBlue
                                    },
                                    new Ellipse
                                    {
                                        Width = 80,
                                        Height = 80,
                                        Fill = Brushes.OrangeRed
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        if (Content is StackPanel root &&
            root.Children[2] is Border border &&
            border.Child is StackPanel panel &&
            panel.Children[2] is Canvas canvas)
        {
            Canvas.SetLeft(canvas.Children[0], 20);
            Canvas.SetTop(canvas.Children[0], 20);
            Canvas.SetLeft(canvas.Children[1], 240);
            Canvas.SetTop(canvas.Children[1], 35);
        }
    }
}
