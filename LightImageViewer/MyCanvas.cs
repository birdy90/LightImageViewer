using LightImageViewer.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

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

        public MyCanvas()
        {
            Img = new Image();
            Children.Add(Img);
            Img.Stretch = Stretch.Fill;
            CurrentImage = Img;
        }

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
                ImageBehavior.SetAnimatedSource(Img, null);

                GetBmpParameters();
                
                var usedSize = 0d;
                if (_widthBigger)
                {
                    usedSize = Math.Min(ActualWidth, _bmpWidth);
                    Img.Width = usedSize;
                    Img.Height = usedSize / _aspect;
                    ImgTop = ActualHeight / 2d - Img.Height / 2d;
                    ImgLeft = ActualWidth / 2d - usedSize / 2d;
                }
                else
                {
                    usedSize = Math.Min(ActualHeight, _bmpHeight);
                    Img.Height = usedSize;
                    Img.Width = usedSize * _aspect;
                    ImgTop = ActualHeight / 2d - usedSize / 2d;
                    ImgLeft = ActualWidth / 2d - Img.Width / 2d;
                }
                UpdateImageSource();
                InvalidateVisual();
                GC.Collect();
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

        public Image Img
        {
            get
            {
                return _img;
            }

            set
            {
                _img = value;
            }
        }

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
            switch (_uri.Segments.Last().Split('.').Last())
            {
                case "svg":
                    var svg = Svg.SvgDocument.Open(_uri.LocalPath);
                    InvalidateVisual();
                    var wScale = width / svg.Width;
                    var hScale = height / svg.Height;
                    //svg.Transforms.Add(new Svg.Transforms.SvgScale(1/wScale, 1/hScale));
                    var w = svg.Width * wScale;
                    var h = svg.Height * hScale;
                    svg.Width = w;
                    svg.Height = h;
                    return svg.Draw().ToBitmapImage();
                case "gif":
                    return null;
                default:
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
                    return bmp;
            }
        }

        /// <summary>
        /// Получение характеристик текущего изображения. Нужно для определения его реального размера в пикселях,
        /// соотношения сторон и определения, вертикальное изображение или горизонтальное
        /// </summary>
        private void GetBmpParameters()
        {
            GetUri();

            BitmapImage bmp = null;
            switch (_uri.Segments.Last().Split('.').Last())
            {
                case "svg":
                    var svg = Svg.SvgDocument.Open(_uri.LocalPath);
                    bmp = svg.Draw().ToBitmapImage();
                    break;
                case "gif":
                    bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = _uri;
                    bmp.EndInit();
                    ImageBehavior.SetAnimatedSource(Img, bmp);
                    break;
                default:
                    bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.None;
                    bmp.UriSource = _uri;
                    bmp.EndInit();
                    break;
            }
            
            _bmpHeight = bmp.PixelHeight;
            _bmpWidth = bmp.PixelWidth;
            if (_bmpHeight > ActualHeight)
                _hCount = (int)Math.Ceiling(_bmpHeight / ActualHeight);
            if (_bmpWidth > ActualWidth)
                _wCount = (int)Math.Ceiling(_bmpWidth / ActualWidth);
            _aspect = (double)_bmpWidth / _bmpHeight;

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
            Img.Source = PrecacheBmp((int)Img.Width, (int)Img.Height);
        }
        
        protected override void OnRender(DrawingContext dc)
        {
            if (CurrentImage == null) return;

            SetTop(CurrentImage, ImgTop);
            SetLeft(CurrentImage, ImgLeft);
        }
    }
}
