using LightImageViewer.Helpers;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using WpfAnimatedGif;
using LightImageViewer.FileFormats;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;

namespace LightImageViewer
{
    public class MyCanvas : Canvas
    {
        private Image _img;
        private MyImage _bmp;

        private int _timeToWait;
        private int _timeToWaitPreset = 350;
        private int _timeToWaitStep = 10;
        private bool _recaching = false;

        /// <summary>
        /// Горизонтальное положение изображения
        /// </summary>
        public double ImgLeft { get; set; }

        /// <summary>
        /// Вертикальное положение изображения
        /// </summary>
        public double ImgTop { get; set; }

        /// <summary>
        /// Текущий отрисовываемый объект Image
        /// </summary>
        public Image Img
        {
            get { return _img; }
            set { _img = value; }
        }

        public MyImage Bmp
        {
            get { return _bmp; }
            set { _bmp = value; }
        }

        public MyCanvas()
        {
            Img = new Image();
            Children.Add(Img);
            Img.Stretch = Stretch.Fill;
            FileList.PathChanged += DrawImage;
            LoadingFailed += FileList.LoadStubImage;
        }

        /// <summary>
        /// Очистка текущего состояния холста
        /// </summary>
        public void Clear()
        {
            if (Img != null)
                ImageBehavior.SetAnimatedSource(Img, null);
            Children.Clear();
            Img = new Image();
            Children.Add(Img);
            InvalidateVisual();
            GC.Collect();
        }
        
        protected override void OnRender(DrawingContext dc)
        {
            if (Img == null) return;

            SetTop(Img, ImgTop);
            SetLeft(Img, ImgLeft);
        }

        public async void DrawImage()
        {
            try
            {
                OnImagestartLoading();
                Clear();
                switch (FileList.CurrentFileExtension)
                {
                    //case "pdf":
                    //    _bmp = new Pdf(this);
                    //    break;
                    //case "eps":
                    //    _bmp = new Eps(this);
                    //    break;
                    case "tif":
                    case "tiff":
                        _bmp = new Tif(this);
                        break;
                    case "svg":
                        _bmp = new FileFormats.Svg(this);
                        break;
                    case "gif":
                        _bmp = new Gif(this);
                        break;
                    case "ico":
                        _bmp = new Ico(this);
                        break;
                    default:
                        _bmp = new MyImage(this);
                        break;
                }
                _bmp.GetImageParameters();
                await Recache(true);
                OnImageLoaded();
            }
            catch (Exception e)
            {
                OnLoadingFailed();
            }
        }

        public async Task Recache(bool immediate = false)
        {
            if (_recaching) return;
            // обновляем счетчик ожидания рекэширования
            _timeToWait = _timeToWaitPreset;
            if (immediate)
                _timeToWait = 0;
            // если кэширование не начато, то запустить поток с ожиданием
            if (!_recaching)
                await Task.Factory.StartNew(WaitToRecache);
        }

        /// <summary>
        /// Методы запускающийся в отдельном потоке и используемый для того, чтобы перекэшировать отображаемое изображение,
        /// но сделать это не сразу, а с небольшой паузой. Таким образом, например при вращении колеса мыши 
        /// перекэширование будет происходить после того, как пользователь "докрутит до нужного масштаба", а не после
        /// каждого деления колёсика
        /// </summary>
        public void WaitToRecache()
        {
            _recaching = true;
            while (_timeToWait > 0)
            {
                _timeToWait -= _timeToWaitStep;
                Thread.Sleep(_timeToWaitStep);
            }
            int w = 0;
            int h = 0;
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            { 
                w = (int)Img.Width;
                h = (int)Img.Height;
            }));
            //var img = _bmp.Precache(w, h);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (Img != null)
                {
                    var img = _bmp.Precache(w, h);
                    Img.Source = img;
                    InvalidateVisual();
                    GC.Collect();
                }
                _recaching = false;
            }));
        }

        public event EventDelegates.MethodContainer LoadingFailed;
        public event EventDelegates.MethodContainer ImageLoaded;
        public event EventDelegates.MethodContainer ImageStartLoading;


        public void OnLoadingFailed()
        {
            if (LoadingFailed != null) LoadingFailed();
        }

        public void OnImageLoaded()
        {
            if (ImageLoaded != null) ImageLoaded();
        }

        public void OnImagestartLoading()
        {
            if (ImageLoaded != null) ImageLoaded();
        }
    }
}
