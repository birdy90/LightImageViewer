using LightImageViewer.Helpers;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using WpfAnimatedGif;
using LightImageViewer.FileFormats;
using System.Windows.Media.Imaging;
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
            GC.Collect();
        }
        
        protected override void OnRender(DrawingContext dc)
        {
            if (Img == null) return;

            SetTop(Img, ImgTop);
            SetLeft(Img, ImgLeft);
        }

        public void DrawImage()
        {
            try
            {
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
                Recache(true);
            }
            catch (Exception e)
            {
                OnLoadingFailed();
            }
        }

        public void Recache(bool immediate = false)
        {
            // обновляем счетчик ожидания рекэширования
            _timeToWait = _timeToWaitPreset;
            if (immediate)
                _timeToWait = 0;
            // если кэширование не начато, то запустить поток с ожиданием
            if (!_recaching) Task.Factory.StartNew(WaitToRecache);
        }

        /// <summary>
        /// Методы запускающийся в отдельном потоке и используемый для того, чтобы перекэшировать отображаемое изображение,
        /// но сделать это не сразу, а с небольшой паузой. Таким образом, например при вращении колеса мыши 
        /// перекэширование будет происходить после того, как пользователь "докрутит до нужного масштаба", а не после
        /// каждого деления колёсика
        /// </summary>
        public void WaitToRecache()
        {
            if (_recaching) return;
            _recaching = true;
            while (_timeToWait > 0)
            {
                _timeToWait -= _timeToWaitStep;
                Thread.Sleep(_timeToWaitStep);
            }
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (Img != null)
                {
                    Img.Source = _bmp.Precache((int)Img.Width, (int)Img.Height);
                    InvalidateVisual();
                    GC.Collect();
                }
                _recaching = false;
            }));
        }

        public event EventDelegates.MethodContainer LoadingFailed;

        public void OnLoadingFailed()
        {
            if (LoadingFailed != null) LoadingFailed();
        }
    }
}
