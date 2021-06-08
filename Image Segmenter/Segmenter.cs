using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace ImageAnalyzer.Segmenting
{
    public class Segmenter
    {
        readonly DirectBitmap bitmap;
        Pixel[,] Pixels;

        public List<Cluster> Clusters = new List<Cluster>();
        readonly Dictionary<Int2, bool> ScannedPixels = new Dictionary<Int2, bool>();

        public float Tolerance;

        #region Debug Properties

        /// <summary>
        /// Milliseconds taken to process image
        /// </summary>
        public float ProcessingTimeTaken { get { return watch.ElapsedMilliseconds; } }
        readonly Stopwatch watch = new Stopwatch();

        #endregion

        public Segmenter(Image image, float tolerance = 10)
        {
            watch.Start();

            bitmap = image.ToBitmap();
            Tolerance = tolerance;

            GetPixels();
            FindClusters();
            CombineClusters();

            watch.Stop();
        }

        void GetPixels()
        {
            Pixels = new Pixel[bitmap.Width, bitmap.Height];

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Pixels[x, y] = new Pixel(x, y, bitmap.GetPixel(x, y));
                }
            }
        }

        #region Segmenting - Clusters

        void FindClusters()
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var pix = Pixels[x, y];

                    if (pix.ParentCluster != null)
                        continue;

                    var cluster = new Cluster(pix.color, new Dictionary<Int2, Pixel>(new Int2Comparer()) { { pix.ToPos(), pix } });
                    Clusters.Add(cluster);
                    pix.ParentCluster = cluster;

                    GetNeighbours(pix);
                }
            }
        }

        void GetNeighbours(Pixel pixel)
        {
            var neighbours = new List<Int2>()
            {
                new Int2(pixel.x + 1, pixel.y), // right
                new Int2(pixel.x + 1, pixel.y + 1), // right down
                new Int2(pixel.x, pixel.y + 1), // down
                new Int2(pixel.x - 1, pixel.y + 1), // left down
                new Int2(pixel.x - 1, pixel.y) // left
            };

            foreach (var neighbour in neighbours)
            {
                if (ScannedPixels.ContainsKey(neighbour))
                    continue;

                ScannedPixels.Add(neighbour, false);

                if (neighbour.x >= Pixels.GetLength(0) ||
                    neighbour.y >= Pixels.GetLength(1) ||
                    neighbour.x < 0 ||
                    neighbour.y < 0)
                    continue;

                var pix = Pixels[neighbour.x, neighbour.y];

                if (pix.ParentCluster != null)
                    continue;

                if (pix.color.CompareTo(pixel.color) > Tolerance)
                    continue;

                if (pixel.ParentCluster.Pixels.ContainsKey(pix.ToPos()))
                    continue;

                pix.ParentCluster = pixel.ParentCluster;
                pixel.ParentCluster.Pixels.Add(pix.ToPos(), pix);

                GetNeighbours(pix);
            }
        }

        void CombineClusters()
        {
            for (int i = 0; i < Clusters.Count; i++)
            {
                for (int j = 0; j < Clusters.Count; j++)
                {
                    if (Clusters[i].Color.CompareTo(Clusters[j].Color) > Tolerance)
                        continue;

                    if (Clusters[i] == Clusters[j])
                        continue;

                    bool Combined = false;

                    foreach (var pair in Clusters[i].Pixels)
                    {
                        var pixel = pair.Key;

                        var neighbours = new Int2[]
                        {
                            new Int2(pixel.x + 1, pixel.y), // right
                            new Int2(pixel.x + 1, pixel.y + 1), // right down
                            new Int2(pixel.x, pixel.y + 1), // down
                            new Int2(pixel.x - 1, pixel.y + 1), // left down
                            new Int2(pixel.x - 1, pixel.y), // left
                            new Int2(pixel.x - 1, pixel.y - 1), // left up
                            new Int2(pixel.x, pixel.y - 1), // up
                            new Int2(pixel.x + 1, pixel.y - 1), // right up
                        };

                        foreach (var neighbour in neighbours)
                        {
                            if (Clusters[j].Pixels.ContainsKey(neighbour))
                            {
                                Clusters[i].Pixels.AddRange(Clusters[j].Pixels);
                                Clusters[j].Pixels.Clear();

                                Combined = true;
                                break;
                            }
                        }

                        if (Combined)
                            break;
                    }
                }
            }

            Clusters = Clusters.Where(x => x.Pixels.Count > 0).ToList();
        }

        #endregion

        public Bitmap[] CreateBitmaps(int width, int height)
        {
            var bitmaps = new Bitmap[Clusters.Count];

            int index = 0;
            foreach (var cluster in Clusters)
            {
                var db = new DirectBitmap(width, height);

                foreach (var pair in cluster.Pixels)
                {
                    db.SetPixel(pair.Key.x, pair.Key.y, pair.Value.color);
                }

                bitmaps[index] = db.Bitmap;

                index++;
            }

            return bitmaps.ToArray();
        }
    }
}
