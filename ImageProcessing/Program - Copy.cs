using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ImageProcessing
{
    class Program
    {
        private static Stopwatch _timer = new Stopwatch();
        private static long _initMemory;
        private const int _scaleFactor = 4;
        private const int _skip = 30;

        private static string _baseDir = "";
        static void Main(string[] args)
        {
            init();

            //DoBulkResize("srcpasimages", "destpasimages");

            //var img1 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\srcpasimages_1\doc-a.tif");
            //var img2 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\destpasimages_1\doc-a-resized.TIF");

            //var img1 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\srcpasimages_5\2019004860.tif");
            //var img2 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\destpasimages_5\0566-06-000003050.FIL.0004.TIF");

            var img1 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\srcpasimages_5\2019004861.tif");
            var img2 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\destpasimages_5\0566-06-000003050.FIL.0005.TIF");


            var img3 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\srcpasimages_1\doc-b.tif");
            var img4 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\destpasimages_1\doc-b-resized.tif");


            //VerifyPixels(img1);
            //VerifyPixels(img2);

            //VerifyPixels(img3);
            //VerifyPixels(img4);

            //img1 = new Bitmap(img1, 100, 100);
            //img2 = new Bitmap(img2, 100, 100);

            //img3 = new Bitmap(img3, 100, 100);
            //img4 = new Bitmap(img4, 100, 100);


            //img1 = ConvertToGrayscale(img1);
            //img2 = ConvertToGrayscale(img2);

            AreSameImages1(img1, img2);

            //var matchresults = MatchIimages("srcpasimages_1", "destpasimages_1");

            //Console.WriteLine($"{matchresults.Count} matches made");

            //foreach (var result in matchresults)
            //{
            //    Console.WriteLine($"{Path.GetFileName(result.Key)} \t\t {Path.GetFileName(result.Value)}");
            //}

            end();
            Console.ReadKey();
        }


        public static Dictionary<string,string> MatchIimages(string srcDir, string dstDir) {
            var matchresults = new Dictionary<string, string>();

            if (!Directory.Exists(srcDir))
                FixBaseDir(srcDir);

            if (!Directory.Exists($"{_baseDir}\\{srcDir}") || !Directory.Exists($"{_baseDir}\\{dstDir}")) {
                Console.WriteLine("Invalid src or dest dirs");
                return matchresults;
            }

            BuildLookup($"{_baseDir}\\{dstDir}");

            var imagesToCheck = Directory.GetFiles($"{_baseDir}\\{srcDir}");

            var imgCompTimer = new Stopwatch();
            imgCompTimer.Start();

            foreach (var imagefile in imagesToCheck) {

                var image = new Bitmap(imagefile);

                image = ResizeImage1(image, image.Width / _scaleFactor, image.Height / _scaleFactor);

                var hash = new StringBuilder();

                for (var i = 0; i < image.Width; i = i + image.Width / _skip)
                {
                    for (var j = 0; j < image.Height; j = j + image.Height / _skip)
                    {
                        hash.Append(image.GetPixel(i, j).GetHashCode());
                    }
                }

                if (_lookup.ContainsKey(hash.ToString())) {
                    matchresults.Add(imagefile, _lookup[hash.ToString()]);
                }
            }

            imgCompTimer.Stop();
            Console.WriteLine($"Matching took {imgCompTimer.ElapsedMilliseconds}ms");

            return matchresults;
        }

        private static Dictionary<string,string> _lookup = new Dictionary<string, string>();
        public static void BuildLookup(string imageLocation)
        {
            var imagesToCheckAgainst = Directory.GetFiles(imageLocation);

            var imgCompTimer = new Stopwatch();
            imgCompTimer.Start();

            foreach (var imagefile in imagesToCheckAgainst){

                var hash = new StringBuilder();

                var image = new Bitmap(imagefile);

                for (var i = 0; i < image.Width; i = i + image.Width / _skip)
                {
                    for (var j = 0; j < image.Height; j = j + image.Height / _skip)
                    {
                        hash.Append(image.GetPixel(i, j).GetHashCode());
                    }
                }

                if (!_lookup.ContainsKey(hash.ToString()))
                {
                    _lookup.Add(hash.ToString(), imagefile);
                }
                else
                {
                    Console.WriteLine($"Possible duplicate image. {imagefile} seems similar to {_lookup[hash.ToString()]}");
                }
            }

            imgCompTimer.Stop();
            Console.WriteLine($"Lookup build took {imgCompTimer.ElapsedMilliseconds}ms");
        }
        
        //the method of resize matters. can change that but have to check timing
        public static bool AreSameImages1(Bitmap image1, Bitmap image2) {

            if (image1.Width > image2.Width || image1.Height > image2.Height) {
                Console.WriteLine("Resizing to image2 's dimensions");
                image1 = ResizeImage1(image1, image2.Width, image2.Height);
            }
            else if(image1.Width < image2.Width || image1.Height < image2.Height)
            {
                Console.WriteLine("Resizing to image1 's dimensions");
                image2 = ResizeImage1(image2, image1.Width, image1.Height);
            }

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

        public static Bitmap ConvertToGrayscale(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            int i = 0;
            Color p;

            //Grayscale
            
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        p = bmp.GetPixel(x, y);
                        int a = p.A;
                        int r = p.R;
                        int g = p.G;
                        int b = p.B;
                        int avg = (r + g + b) / 3;
                        avg = avg < 128 ? 0 : 255; // Converting gray pixels to either pure black or pure white
                        //bmp.SetPixel(x, y, Color.FromArgb(a, avg, avg, avg));
                        Console.WriteLine(Color.FromArgb(a, avg, avg, avg));
                    }
                }
            

            return bmp;
        }

        public static void VerifyPixels(Bitmap image)
        {

            var format = image.PixelFormat;
            Console.WriteLine(format);

            //lock all image bits
            var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);

            //this  will return the pixel index in the color pallete
            // since is 1bpp it will return 0 or 1

            //for (var i = 0; i < image.Width; i++)
            //{
            //    for (var j = 0; j < image.Height; j++)
            //    {
            //        Console.Write($"{GetIndexedPixel(i, j, bitmapData)}");
            //    }

            //    Console.WriteLine();
            //}


            //read the color from pallete
            //Color pixelColor = image.Pallete.Entries[pixelColorIndex];


            //for (var i = 0; i < image.Width; i++)
            //{
            //    for (var j = 0; j < image.Height; j++)
            //    {
            //        Console.WriteLine($"{image.GetPixel(i, j)}");
            //    }
            //}
        }

        // x, y relative to the locked area
        private static int GetIndexedPixel(int x, int y, BitmapData bitmapData)
        {
            var index = y * bitmapData.Stride + (x >> 3);
            var chunk = Marshal.ReadByte(bitmapData.Scan0, index);

            var mask = (byte)(0x80 >> (x & 0x7));
            return (chunk & mask) == mask ? 1 : 0;
        }

        public static bool AreSamePixels(Bitmap image1, Bitmap image2)
        {

            var same = true;
            var pixelsChecked = 0;
            var threshold = 0;

            var imgCompTimer = new Stopwatch();
            imgCompTimer.Start();

            //for (var i = 0; i < image1.Width && same; i = i + image1.Width / 30)
            for (var i = 0; i < image1.Width; i++)
            {
                for (var j = 0; j < image1.Height; j++)
                //for (var j = 0; j < image1.Height; j = j+image1.Height / 30)
                {
                    pixelsChecked++;

                    var img1Pixel = image1.GetPixel(i, j);
                    int a1 = img1Pixel.A;
                    int r1 = img1Pixel.R;
                    int g1 = img1Pixel.G;
                    int b1 = img1Pixel.B;
                    int avg1 = (r1 + g1 + b1) / 3;

                    avg1 = avg1 < 128 ? 0 : 255; // Converting gray pixels to either pure black or pure white
                    //Console.WriteLine(Color.FromArgb(a, avg, avg, avg));
                    img1Pixel = Color.FromArgb(a1, avg1, avg1, avg1);

                    var img2Pixel = image2.GetPixel(i, j);
                    int a2 = img2Pixel.A;
                    int r2 = img2Pixel.R;
                    int g2 = img2Pixel.G;
                    int b2 = img2Pixel.B;
                    int avg2 = (r2 + g2 + b2) / 3;

                    avg2 = avg2 < 128 ? 0 : 255; 
                    img2Pixel = Color.FromArgb(a2, avg2, avg2, avg2);
                    
                    //Console.WriteLine($"{img1Pixel} \t {img1Pixel.GetHashCode()} \t\t {img2Pixel} \t {img2Pixel.GetHashCode()}");
                    
                    if (img1Pixel.GetHashCode() != img2Pixel.GetHashCode())
                    {
                        threshold++;
                        Console.WriteLine($"Pixels are different at {i},{j} \t {img1Pixel} \t\t {img2Pixel} =============================");

                        if (threshold > 200)
                        {
                            same = false;
                            break;
                        }
                        
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

            Console.WriteLine($"Pixels checked {pixelsChecked} with threshold {threshold}");

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

        public static bool FixBaseDir(string dirName)
        {

            string workingDirectory = Environment.CurrentDirectory;
            // or: Directory.GetCurrentDirectory() gives the same result

            // This will get the current PROJECT directory
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;

            var dir = Directory.GetDirectories(projectDirectory).SingleOrDefault(s => s.Equals($"{projectDirectory}\\{dirName}"));

            if (!string.IsNullOrEmpty(dir))
            {
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

                SaveResizedImage1(new Bitmap(file), _scaleFactor,
                    $"{_baseDir}\\{dstDir}\\{Path.GetFileNameWithoutExtension(file)}-resized{Path.GetExtension(file)}");

            }
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
