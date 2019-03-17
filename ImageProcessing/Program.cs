using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace ImageProcessing
{
    class Program
    {
        private static Stopwatch _timer = new Stopwatch();
        private static long _initMemory;
        static void Main(string[] args)
        {
            init();
            
            //compare two images and find if they are resized versions

            var img1 = new Bitmap(@"C:\Users\archa\Documents\Projects\ImageProcessing\ImageProcessing\sample_a.tif");

            var img2 = new Bitmap(@"C:\Users\archa\Documents\Projects\ImageProcessing\ImageProcessing\sample_a_resized.tif");

            AreSameImages(img1, img2);


            end();
            Console.ReadKey();
        }


        public static bool AreSameImages(Bitmap image1, Bitmap image2) {

            if (image1.Width > image2.Width && image1.Height > image2.Height) {
                image1 = ResizeImage(image1, image2.Width, image2.Height);
            }else if (image1.Width < image2.Width && image1.Height < image2.Height)
            {
                image2 = ResizeImage(image2, image1.Width, image1.Height);
            }

            var same = true;
            var counter = 0;

            for (var i = 0; i < image1.Width / 4; i++)
            {
                for (var j = 0; j < image1.Height / 4; j++)
                {
                    if (image1.GetPixel(i, j).GetHashCode() != image2.GetPixel(i, j).GetHashCode())
                    {
                        same = false;
                        break;
                    }

                    counter++;
                }
            }


            if (same)
            {
                Console.WriteLine("The images are same");
            }
            else
            {
                Console.WriteLine("Not same");
            }

            Console.WriteLine($"pixels checked {counter}");
                       
            return same;
        }

        public static void CreateResizedImage() {
            var img = new Bitmap(@"C:\Users\archa\Documents\Projects\ImageProcessing\ImageProcessing\sample_a.tif");

            var img2 = ResizeImage(img, img.Width / 20, img.Height / 20);

            img2.Save(@"C:\Users\archa\Documents\Projects\ImageProcessing\ImageProcessing\sample_a_resized.tif");
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            //init();

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                //graphics.CompositingMode = CompositingMode.SourceCopy;
                //graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            //end();

            return destImage;
        }


























        private static void init()
        {
            _timer = new Stopwatch();
            _initMemory = Process.GetCurrentProcess().VirtualMemorySize64;
            _timer.Start();
        }

        private static void end()
        {
            _timer.Stop();
            Console.WriteLine($"{_timer.ElapsedMilliseconds}ms {(Process.GetCurrentProcess().VirtualMemorySize64 - _initMemory) / 1024}kb");
        }
    }
}
