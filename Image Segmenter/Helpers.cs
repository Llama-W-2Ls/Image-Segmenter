using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace ImageAnalyzer.Segmenting
{
    static class Extensions
    {
        /// <summary>
        /// Outputs % difference between two colors
        /// </summary>
        public static float CompareTo(this Color e1, Color e2)
        {
            long rmean = (e1.R + (long)e2.R) / 2;
            long r = e1.R - (long)e2.R;
            long g = e1.G - (long)e2.G;
            long b = e1.B - (long)e2.B;

            return (int)Math.Sqrt
            (
                (((512 + rmean) * r * r) >> 8)
                + 4 * g * g +
                (((767 - rmean) * b * b) >> 8)
            ) / 7.64f;
        }

        public static DirectBitmap ToBitmap(this Image image)
        {
            var bitmap = new DirectBitmap(image.Width, image.Height);
            var oldBitmap = new Bitmap(image);

            unsafe
            {
                BitmapData bitmapData = oldBitmap.LockBits
                (
                    new Rectangle(0, 0, oldBitmap.Width, oldBitmap.Height),
                    ImageLockMode.ReadWrite,
                    oldBitmap.PixelFormat
                );

                int bytesPerPixel = Image.GetPixelFormatSize(oldBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);

                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];
                        int oldAlpha = currentLine[x + 3];

                        var color = Color.FromArgb(oldAlpha, oldRed, oldGreen, oldBlue);

                        bitmap.SetPixel(x / bytesPerPixel, y, color);
                    }
                }

                oldBitmap.UnlockBits(bitmapData);
            }

            return bitmap;
        }

        public static void AddRange(this Dictionary<Int2, Pixel> dict, Dictionary<Int2, Pixel> range)
        {
            foreach (var pair in range)
            {
                if (!dict.ContainsKey(pair.Key))
                    dict.Add(pair.Key, pair.Value);
            }
        }
    }

    class Int2Comparer : IEqualityComparer<Int2>
    {
        public bool Equals(Int2 v1, Int2 v2)
        {
            return v1.x == v2.x && v1.y == v2.y;
        }

        public int GetHashCode(Int2 obj)
        {
            return obj.x * 31 + obj.y;
        }
    }
}
