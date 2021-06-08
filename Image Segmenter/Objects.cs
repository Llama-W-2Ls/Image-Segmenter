using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageAnalyzer.Segmenting
{
    class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public int[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new int[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }

    public class Pixel
    {
        public int x, y;
        public Color color;

        public Cluster ParentCluster;

        public Pixel(int _x, int _y, Color _color)
        {
            x = _x;
            y = _y;
            color = _color;
        }

        public Int2 ToPos()
        {
            return new Int2(x, y);
        }
    }

    public class Cluster
    {
        public Color Color;
        public Dictionary<Int2, Pixel> Pixels;

        public Cluster(Color color, Dictionary<Int2, Pixel> pixels)
        {
            Color = color;
            Pixels = pixels;
        }
    }

    public class Int2
    {
        public int x, y;

        public Int2(int X, int Y)
        {
            x = X;
            y = Y;
        }
    }
}
