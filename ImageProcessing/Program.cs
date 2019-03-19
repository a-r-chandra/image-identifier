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
        private static Dictionary<string, string> _lookup = new Dictionary<string, string>();
        private static string _baseDir = "";

        static void Main(string[] args)
        {
            init();

            //DoBulkResize("srcpasimages", "destpasimages");
            
            //var img1 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\srcpasimages_5\2019004861.tif");
            //var img2 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\destpasimages_5\2019004861.tif");

            var img1 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\srcpasimages_5\sample.tif");
            var img2 = new Bitmap(@"C:\Users\rchandra\Desktop\image-identifier-master\ImageProcessing\destpasimages_5\sample-resized.tif");


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
        public static bool AreSameImages1(Bitmap image1, Bitmap image2)
        {
            var img1StartXY = GetStartOfText(image1);
            var img2StartXY = GetStartOfText(image2);

            //resize to text placement
            image1 = ResizeImage1(image1, image1.Width - img1StartXY.Item1, image1.Height - img1StartXY.Item2,
                img1StartXY.Item1, img1StartXY.Item2);

            image2 = ResizeImage1(image2, image2.Width - img2StartXY.Item1, image2.Height - img2StartXY.Item2,
                img2StartXY.Item1, img2StartXY.Item2);

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
        
        //lossless
        public static Bitmap ResizeImage1(Image image, int width, int height, int x=0, int y=0)
        {
            //init();
            Console.WriteLine("Resizing");

            var destRect = new Rectangle(x, y, width, height);
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
                    graphics.DrawImage(image, destRect, x, y, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            //end();

            return destImage;
        }

        public static Tuple<int, int> GetStartOfText(Bitmap image)
        {
            for (var i = 0; i < image.Width; i++)
            {
                for (var j = 0; j < image.Height; j++)
                {

                    var img1Pixel = image.GetPixel(i, j);
                    int a1 = img1Pixel.A;
                    int r1 = img1Pixel.R;
                    int g1 = img1Pixel.G;
                    int b1 = img1Pixel.B;
                    int avg1 = (r1 + g1 + b1) / 3;
                    avg1 = avg1 < 128 ? 0 : 255; // Converting gray pixels to either pure black or pure white
                    img1Pixel = Color.FromArgb(a1, avg1, avg1, avg1);


                    if (r1 == 0 || g1 == 0 || b1 == 0)
                    {
                        Console.WriteLine($"Text found at {i},{j} {img1Pixel} {img1Pixel.GetHashCode()}");
                        return new Tuple<int, int>(i, j);
                    }

                }
            }

            return new Tuple<int, int>(0, 0);
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
                    img1Pixel = Color.FromArgb(a1, avg1, avg1, avg1);

                    var img2Pixel = image2.GetPixel(i, j);
                    int a2 = img2Pixel.A;
                    int r2 = img2Pixel.R;
                    int g2 = img2Pixel.G;
                    int b2 = img2Pixel.B;
                    int avg2 = (r2 + g2 + b2) / 3;
                    avg2 = avg2 < 128 ? 0 : 255; 
                    img2Pixel = Color.FromArgb(a2, avg2, avg2, avg2);

                    //Console.WriteLine($"{i},{j} {img1Pixel} \t {img1Pixel.GetHashCode()} \t\t {img2Pixel} \t {img2Pixel.GetHashCode()}");

                    if (r1 == 0 || r2 == 0 || g1 == 0 || g2 == 0 || b1 == 0 || b2 == 0)
                    {
                        //Console.WriteLine($"Text found at {i},{j} {img1Pixel} {img1Pixel.GetHashCode()} \t\t {img2Pixel} {img2Pixel.GetHashCode()}");
                    }


                    if (img1Pixel.GetHashCode() != img2Pixel.GetHashCode())
                    {
                        threshold++;
                        Console.WriteLine($"Pixels are different at {i},{j} : {img1Pixel} {img2Pixel}");

                        if (threshold > 0)
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

            Console.WriteLine($"Pixels checked {pixelsChecked} threshold {threshold}");

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
