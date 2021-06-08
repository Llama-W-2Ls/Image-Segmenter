using System;
using System.Drawing;
using ImageAnalyzer.Segmenting;

namespace ImageAnalyzer
{
    class Program
    {
        // Choose your own image directory
        const string ImageDirectory = "(...).png";
        const string SaveFolderDirectory = "(...)";

        static void Main()
        {
            var image = Image.FromFile(ImageDirectory);

            // Segments the image, clustering together similar pixels with a tolerance of 40%
            var segmenter = new Segmenter(image, 40);

            // Time taken to process image
            Console.WriteLine("Processed in " + segmenter.ProcessingTimeTaken + " milliseconds");

            // Saves all clusters as individual bitmaps
            Console.WriteLine("Saving " + segmenter.Clusters.Count + " bitmaps");
            var clusters = segmenter.CreateBitmaps(image.Width, image.Height);

            for (int i = 0; i < clusters.Length; i++)
            {
                string directory = SaveFolderDirectory + "/cluster(" + i + ").png";
                clusters[i].Save(directory);
            }

            Console.WriteLine("Saved");
        }
    }
}
