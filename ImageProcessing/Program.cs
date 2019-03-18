using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ImageProcessing
{
    class Program
    {
        private static Stopwatch _timer = new Stopwatch();
        private static long _initMemory;
        private const int scaleFactor = 10;

        private static string _baseDir = "";
        static void Main(string[] args)
        {
            init();

            //DoBulkResize("srcimages_large", "destimages");


            var img1 = new Bitmap(@"C:\Users\archa\Documents\Projects\image-identifier\ImageProcessing\srcimages_large\anita-austvika-1426774-unsplash.jpg");

            var img2 = new Bitmap(@"C:\Users\archa\Documents\Projects\image-identifier\ImageProcessing\destimages\anita-austvika-1426774-unsplash-resized.jpg");

            AreSameImages1(img1, img2);



            end();
            Console.ReadKey();
        }


        public static bool FixBaseDir(string dirName) {

            string workingDirectory = Environment.CurrentDirectory;
            // or: Directory.GetCurrentDirectory() gives the same result

            // This will get the current PROJECT directory
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;

            var dir = Directory.GetDirectories(projectDirectory).SingleOrDefault(s => s.Equals($"{projectDirectory}\\{dirName}"));

            if (!string.IsNullOrEmpty(dir)) {
                _baseDir = dir.Replace(dirName, "");
                return true;
            }
            return false;
        }

        public static void DoBulkResize(string srcDir, string dstDir)
        {

            if (!Directory.Exists(srcDir))
                FixBaseDir(srcDir);

            if (!Directory.Exists($"{_baseDir}\\{dstDir}"))
                Directory.CreateDirectory($"{_baseDir}\\{dstDir}");

            var files = Directory.EnumerateFiles($"{_baseDir}\\{srcDir}");

            foreach (var file in files)
            {

                SaveResizedImage1(new Bitmap(file), scaleFactor, 
                    $"{_baseDir}\\{dstDir}\\{Path.GetFileNameWithoutExtension(file)}-resized{Path.GetExtension(file)}");

            }
        }
                     
        //the method of resize matters. can change that but have to check timing
        public static bool AreSameImages1(Bitmap image1, Bitmap image2) {
            var rezTimer = new Stopwatch();
            rezTimer.Start();

            if (image1.Width > image2.Width || image1.Height > image2.Height) {
                Console.WriteLine("Resizing to image2 's dimensions");
                image1 = ResizeImage1(image1, image2.Width, image2.Height);
            }
            else if(image1.Width < image2.Width || image1.Height < image2.Height)
            {
                Console.WriteLine("Resizing to image1 's dimensions");
                image2 = ResizeImage1(image2, image1.Width, image1.Height);
            }
            rezTimer.Stop();
            Console.WriteLine($"Resize took {rezTimer.ElapsedMilliseconds}ms");


            return AreSamePixels(image1, image2);
        }

        public static bool AreSameImages2(Bitmap image1, Bitmap image2)
        {
            var rezTimer = new Stopwatch();
            rezTimer.Start();

            if (image1.Width > image2.Width || image1.Height > image2.Height)
            {
                Console.WriteLine("Resizing to image2 's dimensions");
                image1 = new Bitmap(image1, new Size(image2.Width, image2.Height));
            }
            else if (image1.Width < image2.Width || image1.Height < image2.Height)
            {
                Console.WriteLine("Resizing to image1 's dimensions");
                image2 = new Bitmap(image2, new Size(image1.Width, image1.Height));
            }
            rezTimer.Stop();
            Console.WriteLine($"Resize took {rezTimer.ElapsedMilliseconds}ms");

            return AreSamePixels(image1, image2);
        }

        public static bool AreSameImages3(Bitmap image1, Bitmap image2)
        {
            var rezTimer = new Stopwatch();
            rezTimer.Start();

            if (image1.Width > image2.Width || image1.Height > image2.Height)
            {
                Console.WriteLine("Resizing to image2 's dimensions");
                image1 = new Bitmap(image1, new Size(image2.Width, image2.Height));
            }
            else if (image1.Width < image2.Width || image1.Height < image2.Height)
            {
                Console.WriteLine("Resizing to image1 's dimensions");
                image2 = new Bitmap(image2, new Size(image1.Width, image1.Height));
            }
            rezTimer.Stop();
            Console.WriteLine($"Resize took {rezTimer.ElapsedMilliseconds}ms");

            return AreSamePixels(image1, image2);
        }

        //lossless
        public static Bitmap ResizeImage1(Image image, int width, int height)
        {
            //init();
            Console.WriteLine("Resizing");

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            //destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

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

        //lossy
        public static Bitmap ResizeImage2(Image image, int width, int height) {

            return new Bitmap(image, width, height);
        }

        public static Image ResizeImage3(Image image, int width, int height)
        {
            Image newImage = new Bitmap(width, height);

            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, width, height);
            }
            
            return newImage;
        }

        public static bool AreSamePixels(Bitmap image1, Bitmap image2)
        {

            var same = true;
            var pixelsChecked = 0;

            var imgCompTimer = new Stopwatch();
            imgCompTimer.Start();

            for (var i = 0; i < image1.Width; i = i + image1.Width / 3)
            //for (var i = 0; i < image1.Width; i++)
            {
                for (var j = 0; j < image1.Height; j = j + image1.Height / 3)
                //for (var j = 0; j < image1.Height; j++)
                {
                    pixelsChecked++;

                    if (image1.GetPixel(i, j).GetHashCode() != image2.GetPixel(i, j).GetHashCode())
                    {
                        same = false;
                        break;
                    }

                }
            }
            imgCompTimer.Stop();
            Console.WriteLine($"Image compare took {imgCompTimer.ElapsedMilliseconds}ms");


            if (same)
            {
                Console.WriteLine("The images are same....OK");
            }
            else
            {
                Console.WriteLine("Images are not same X");
            }

            Console.WriteLine($"Pixels checked {pixelsChecked}");

            return same;
        }

        public static void SaveResizedImage1(Bitmap image, int scaleFactor, string newFilename)
        {

            var resizedImage = ResizeImage1(image, image.Width / scaleFactor, image.Height / scaleFactor);

            if (File.Exists(newFilename))
            {
                File.Delete(newFilename);
            }

            resizedImage.Save(newFilename);

        }

        public static void SaveResizedImage3(Bitmap image, int scaleFactor, string newFilename)
        {

            var resizedImage = ResizeImage3(image, image.Width / scaleFactor, image.Height / scaleFactor);

            if (File.Exists(newFilename)) {
                File.Delete(newFilename);
            }
                
            resizedImage.Save(newFilename);

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
