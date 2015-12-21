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
    /// <summary>
    /// Custom canvas class. It manages creating and positioning of loaded images
    /// </summary>
    public class MyCanvas : Canvas
    {
        /// <summary>
        /// Displayed image
        /// </summary>
        private Image _img;

        /// <summary>
        /// Opened image reader. Serves loading and rendering of images
        /// </summary>
        private ImageReader _bmp;

        /// <summary>
        /// Wait time before recaching
        /// </summary>
        private int _timeToWait;

        /// <summary>
        /// Default time we need to wait before recaching
        /// </summary>
        private int _timeToWaitPreset = 350;

        /// <summary>
        /// Time decreasing step. It's needed if while waiting for recaching we done new operation,
        /// and recaching time must be reset to it's preset
        /// </summary>
        private int _timeToWaitStep = 50;

        /// <summary>
        /// If image reader is waiting for recaching
        /// </summary>
        private bool _recaching = false;

        /// <summary>
        /// Get or sets horizontal position
        /// </summary>
        public double ImgLeft { get; set; }

        /// <summary>
        /// Get or sets vertical position
        /// </summary>
        public double ImgTop { get; set; }

        /// <summary>
        /// Gets or sets current image
        /// </summary>
        public Image Img
        {
            get { return _img; }
            set { _img = value; }
        }

        /// <summary>
        /// Gets or sets current image reader
        /// </summary>
        public ImageReader Bmp
        {
            get { return _bmp; }
            set { _bmp = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MyCanvas()
        {
            Img = new Image();
            Children.Add(Img);
            Img.Stretch = Stretch.Fill;
            FileList.PathChanged += DrawImage;
            LoadingFailed += FileList.LoadStubImage;
        }

        /// <summary>
        /// Clean current canvas state
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
        
        /// <summary>
        /// Renderding of canvas
        /// </summary>
        /// <param name="dc"></param>
        protected override void OnRender(DrawingContext dc)
        {
            if (Img == null) return;
            SetTop(Img, ImgTop);
            SetLeft(Img, ImgLeft);
        }

        /// <summary>
        /// Creating new image and rendering of it
        /// </summary>
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
                        _bmp = new ImageReader(this);
                        break;
                }
                _bmp.GetImageParameters();
                await Recache(true);
            }
            catch (Exception e)
            {
                OnLoadingFailed();
            }
            finally
            {
                OnImageLoaded();
            }
        }

        /// <summary>
        /// Command to recache image. Starts new thread or set new wait time if it is working
        /// </summary>
        /// <param name="immediate">Set wait time to 0 if is true</param>
        /// <returns></returns>
        public async Task Recache(bool immediate = false)
        {
            if (_recaching) return;
            // refresh wait time
            _timeToWait = _timeToWaitPreset;
            if (immediate)
                _timeToWait = 0;
            // if recache thread is not started then start it
            if (!_recaching)
                await Task.Factory.StartNew(WaitToRecache);
        }

        /// <summary>
        /// Rechaching thread
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
