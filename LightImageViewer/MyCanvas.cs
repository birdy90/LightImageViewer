using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace LightImageViewer
{
    public class MyCanvas : Canvas
    {
        private string _currentPath;
        private Image img;

        public string CurrentPath
        {
            get
            { 
                return _currentPath; 
            }
            set
            {
                _currentPath = value;
                GetUri();
                if ((_currentPath.Split('.').Last() as String).ToLower() == "gif")
                    img = new AnimatedImage();
                else
                    img = new Image();
                img.Stretch = Stretch.Fill;

                GetBmpParameters();

                CurrentImage = img;
                var usedSize = 0d;
                if (widthBigger)
                {
                    usedSize = Math.Min(ActualWidth, bmpWidth);
                    img.Width = usedSize;
                    img.Height = usedSize / aspect;
                    ImgTop = ActualHeight / 2d - img.Height / 2d;
                    ImgLeft = ActualWidth / 2d - usedSize / 2d;
                }
                else
                {
                    usedSize = Math.Min(ActualHeight, bmpHeight);
                    img.Height = usedSize;
                    img.Width = usedSize * aspect;
                    ImgTop = ActualHeight / 2d - usedSize / 2d;
                    ImgLeft = ActualWidth / 2d - img.Width / 2d;
                }
                CurrentImage = img;
                UpdateImageSource();
                Children.Clear();
                GC.Collect();
                Children.Add(img);
            }
        }

        private Uri _uri;

        public Image CurrentImage { get; set; }

        public double ImgLeft { get; set; }

        public double ImgTop { get; set; }

        public BitmapImage PrecacheBmp(int width, int height)
        {
            GetUri();
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.None;
            if (widthBigger)
                bmp.DecodePixelWidth = Math.Min(width, bmpWidth);
            else
                bmp.DecodePixelHeight = Math.Min(height, bmpHeight);
            bmp.UriSource = _uri;
            //var leftCrop = (int)Math.Max(-ImgLeft, 0);
            //var topCrop = (int)Math.Max(-ImgTop, 0);
            //var widthCrop = (int)Math.Min(-ImgLeft + ActualWidth, bmpWidth) - leftCrop;
            //var heightCrop = (int)Math.Min(-ImgTop + ActualHeight, bmpHeight) - topCrop;
            //bmp.SourceRect = new Int32Rect(leftCrop, topCrop, widthCrop, heightCrop);
            bmp.EndInit();
            return bmp;
        }

        int hCount = 0;
        int wCount = 0;
        int bmpHeight = 0;
        int bmpWidth = 0;
        bool widthBigger = false;
        double aspect = 1;
        private void GetBmpParameters()
        {
            GetUri();
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.None;
            bmp.UriSource = _uri;
            bmp.EndInit();
            bmpHeight = bmp.PixelHeight;
            bmpWidth = bmp.PixelWidth;
            if (bmpHeight > ActualHeight)
                hCount = (int)Math.Ceiling(bmpHeight / ActualHeight);
            if (bmpWidth > ActualWidth)
                wCount = (int)Math.Ceiling(bmpWidth / ActualWidth);
            aspect = (double)bmpWidth / (double)bmpHeight;

            widthBigger = false;
            if (ActualWidth / ActualHeight < aspect)
                widthBigger = true;
        }

        public bool CanMakeBigger(int width, int height)
        {
            return true;
            var bmp = new BitmapImage(_uri);
            return bmp.Height > height && bmp.Width > width;
        }

        public void GetUri()
        {
            if (File.Exists(_currentPath))
            {
                _uri = new Uri(CurrentPath);
            }
            else
            {
                _uri = new Uri("pack://application:,,,/Resources/error.png");
            }
        }

        public void UpdateImageSource()
        {
            GetUri();
            if ((_currentPath.Split('.').Last() as String).ToLower() == "gif")
            {
                //ImageBehavior.SetAnimatedSource(img, PrecacheBmp((int)img.Width, (int)img.Height));
                (img as AnimatedImage).Source = PrecacheBmp((int)img.Width, (int)img.Height);
                //(img as GifImage).GifSource = _uri;
            }
            else
                img.Source = PrecacheBmp((int)img.Width, (int)img.Height);
        }
        
        protected override void OnRender(DrawingContext dc)
        {
            if (CurrentImage == null) return;

            Canvas.SetTop(CurrentImage, ImgTop);
            Canvas.SetLeft(CurrentImage, ImgLeft);
        }
    }
}
