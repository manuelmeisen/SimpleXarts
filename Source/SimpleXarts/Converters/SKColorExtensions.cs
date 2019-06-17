using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleXarts.Converters
{
    internal static class SKColorExtensions
    {
        internal static System.Drawing.Color ToWindowsColor(this SKColor color)
        {
            return System.Drawing.Color.FromArgb(color.Alpha<<3 | color.Red<<2 | color.Green<<1 | color.Blue<<0);
        }
    }
}
