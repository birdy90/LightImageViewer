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

namespace LightImageViewer
{
    public class MyCanvas : Canvas
    {
        /// <summary>
        /// Путь до текущего изображения
        /// </summary>
        private string _currentPath;
        private Image _img;

        private Uri _uri;

        private int _hCount = 0;
        private int _wCount = 0;
        private int _bmpHeight = 0;
        private int _bmpWidth = 0;
        private bool _widthBigger = false;
        private double _aspect = 1;

        /// <summary>
        /// Путь до текущего изображения. Задание пути инициирует создание нового изображения,
        /// загружаемого по указанному пути (задаются новые размеры и положение изображения).
        /// При повторном задании того же пути, который используется в данный момент, 
        /// ничего не будет происходить.
        /// </summary>
        public string CurrentPath
        {
            get
            {
                return _currentPath;
            }
            set
            {
                // если путь тот же, то игнорируем
                if (string.Equals(_currentPath, value)) return;

                _currentPath = value;
                GetUri();
                if ((_currentPath.Split('.').Last() as String).ToLower() == "gif")
                    _img = new AnimatedImage();
                else
                    _img = new Image();
                _img.Stretch = Stretch.Fill;

                GetBmpParameters();

                CurrentImage = _img;
                var usedSize = 0d;
                if (_widthBigger)
                {
                    usedSize = Math.Min(ActualWidth, _bmpWidth);
                    _img.Width = usedSize;
                    _img.Height = usedSize / _aspect;
                    ImgTop = ActualHeight / 2d - _img.Height / 2d;
                    ImgLeft = ActualWidth / 2d - usedSize / 2d;
                }
                else
                {
                    usedSize = Math.Min(ActualHeight, _bmpHeight);
                    _img.Height = usedSize;
                    _img.Width = usedSize * _aspect;
                    ImgTop = ActualHeight / 2d - usedSize / 2d;
                    ImgLeft = ActualWidth / 2d - _img.Width / 2d;
                }
                CurrentImage = _img;
                UpdateImageSource();
                Children.Clear();
                GC.Collect();
                Children.Add(_img);
            }
        }

        /// <summary>
        /// Текущий отрисовываемый объект Image
        /// </summary>
        public Image CurrentImage { get; set; }

        /// <summary>
        /// Горизонтальное положение изображения
        /// </summary>
        public double ImgLeft { get; set; }

        /// <summary>
        /// Вертикальное положение изображения
        /// </summary>
        public double ImgTop { get; set; }

        /// <summary>
        /// Перекэширование изображения. Необходимо для подзагрузки новой копии изображения,
        /// плотность пикселей которого будет совпадать с плотностью пикселей экрана. Таким образом,
        /// будет загружено минимально возможное изображение, которое не будет иметь размытость.
        /// </summary>
        /// <param name="width">Ширина изображения на экране</param>
        /// <param name="height">Высота изображения на экране</param>
        /// <returns></returns>
        public BitmapImage PrecacheBmp(int width, int height)
        {
            GetUri();
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.None;
            // уловие, в зависимости от того, горизонтальное изображение или вертикальное,
            // указывает битмапу, каков требуемый размер изображения в пикселях. Задаётся только
            // одно измерение, второе будет сформировано автоматически в соответствии
            // с соотношением сторон изображения. "Загружаемый размер" не должен превышать
            // настоящий размер изображения
            if (_widthBigger)
                bmp.DecodePixelWidth = Math.Min(width, _bmpWidth);
            else
                bmp.DecodePixelHeight = Math.Min(height, _bmpHeight);
            bmp.UriSource = _uri;
            bmp.EndInit();
            InvalidateVisual();
            return bmp;
        }

        /// <summary>
        /// Получение характеристик текущего изображения. Нужно для определения его реального размера в пикселях,
        /// соотношения сторон и определения, вертикальное изображение или горизонтальное
        /// </summary>
        private void GetBmpParameters()
        {
            GetUri();
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.None;
            bmp.UriSource = _uri;
            bmp.EndInit();
            _bmpHeight = bmp.PixelHeight;
            _bmpWidth = bmp.PixelWidth;
            if (_bmpHeight > ActualHeight)
                _hCount = (int)Math.Ceiling(_bmpHeight / ActualHeight);
            if (_bmpWidth > ActualWidth)
                _wCount = (int)Math.Ceiling(_bmpWidth / ActualWidth);
            _aspect = (double)_bmpWidth / (double)_bmpHeight;

            _widthBigger = false;
            if (ActualWidth / ActualHeight < _aspect)
                _widthBigger = true;
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
                (_img as AnimatedImage).Source = PrecacheBmp((int)_img.Width, (int)_img.Height);
            else
                _img.Source = PrecacheBmp((int)_img.Width, (int)_img.Height);
        }
        
        protected override void OnRender(DrawingContext dc)
        {
            if (CurrentImage == null) return;

            Canvas.SetTop(CurrentImage, ImgTop);
            Canvas.SetLeft(CurrentImage, ImgLeft);
        }
    }
}
