using System.Drawing;
using System.Drawing.Drawing2D;

namespace ClaudeForInventor
{
    internal static class AppIcon
    {
        private static readonly Color Petal = Color.FromArgb(205, 105, 78);
        private static readonly Color Bg    = Color.FromArgb(22,  22,  22);

        internal static Bitmap Make(int size)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Bg);

            float outerR = size * 0.44f;
            float innerR = size * 0.09f;
            float petalW = size * 0.155f;

            using var brush = new SolidBrush(Petal);
            g.TranslateTransform(size / 2f, size / 2f);
            for (int i = 0; i < 12; i++)
            {
                g.RotateTransform(30f);
                g.FillEllipse(brush, -petalW / 2f, -outerR, petalW, outerR - innerR);
            }
            g.ResetTransform();

            // Dark centre hole
            using var holeBrush = new SolidBrush(Bg);
            float hx = size / 2f - innerR;
            g.FillEllipse(holeBrush, hx, hx, innerR * 2f, innerR * 2f);

            return bmp;
        }
    }
}
