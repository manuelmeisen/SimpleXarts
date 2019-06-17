using SkiaSharp;
using System.Drawing;
namespace SimpleXarts.Converters
{
    internal static class DrawingColorExtensions
    {
        internal static SKColor ToSKColor(this Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }
    }
}
