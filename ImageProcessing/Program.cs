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

            var img1_r = ResizeImage(img1, img1.Width / 20, img1.Height / 20);

            var img2 = new Bitmap(@"C:\Users\archa\Documents\Projects\ImageProcessing\ImageProcessing\sample_a_resized.tif");

                       
            var wd = img1_r.Width;
            var ht = img1_r.Height;

            //Console.WriteLine($"w:{wd} h:{ht}");

            var mtx = new Color[wd][];
            for (var i=0; i<wd; i++) {

                mtx[i] = new Color[ht];

                for (var j=0; j<ht; j++) {

                    var pixel = img1_r.GetPixel(i,j);
                    //Console.WriteLine(pixel);
                    //Console.WriteLine(pixel.GetHashCode());

                    mtx[i][j] = pixel;
                }
            }


            var same = true;

            for (var i = 0; i < wd; i++)
            {

                for (var j = 0; j < ht; j++)
                {

                    var pixel = img2.GetPixel(i, j);
                    //Console.WriteLine(pixel);
                    //Console.WriteLine(pixel.GetHashCode());

                    //Console.WriteLine($"{mtx[i][j].GetHashCode()} {pixel.GetHashCode()}");
                    if (mtx[i][j].GetHashCode() != pixel.GetHashCode()) {
                        same = false;
                        break;
                    }
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








            //Console.WriteLine($"mtx len:{mtx.Length}");
            /*
            for (var i=0; i<mtx.Length; i++) {

                var m = mtx[i];

                for (var j=0; j<m.Length; j++) {

                    //Console.WriteLine(m[j]);

                }
                
            }
            */



            /*
              
            if (img1 == img2)
            {
                Console.WriteLine("The images are same");
            }
            else {
                Console.WriteLine("Not same");
            }
             
             var img2 = new Bitmap(@"C:\Users\archa\Documents\Projects\ImageProcessing\ImageProcessing\sample_a_same.tif");

            
             */







            end();
            Console.ReadKey();
        }


        public bool AreImagesSame(Bitmap image1, Bitmap image2) {

            var img1W = image1.Width;
            var img1H = image1.Height;

            var img2W = image2.Width;
            var img2H = image2.Height;

            if (img1H > img2H && img1W > img2W) {

                image1 = ResizeImage(image1, img2W, img2H);

            }else if (img1H < img2H && img1W < img2W) {

                image2 = ResizeImage(image2, img1W, img1H);
            }

            var same = true;

            for (var i = 0; i < img1W; i++)
            {

                for (var j = 0; j < img1H; j++)
                {
                    var img1Pixel = image1.GetPixel(i, j);
                    var img2Pixel = image2.GetPixel(i, j);

                    if (img1Pixel.GetHashCode() != img2Pixel.GetHashCode())
                    {
                        same = false;
                        break;
                    }
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
                       
            return same;
        }



        public void CreateResizedImage() {
            var img = new Bitmap(@"C:\Users\archa\Documents\Projects\ImageProcessing\ImageProcessing\sample_a.tif");

            var img2 = ResizeImage(img, img.Width / 20, img.Height / 20);

            img2.Save(@"C:\Users\archa\Documents\Projects\ImageProcessing\ImageProcessing\sample_a_resized.tif");
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

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
            Console.ReadLine();
        }
    }
}
