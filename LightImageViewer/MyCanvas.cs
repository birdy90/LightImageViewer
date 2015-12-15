using LightImageViewer.Helpers;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using WpfAnimatedGif;
using LightImageViewer.FileFormats;
using System.Windows.Media.Imaging;

namespace LightImageViewer
{
    public class MyCanvas : Canvas
    {
        private Image _img;
        private MyImage _bmp;
        
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
                RedrawImage();
            }
            catch (Exception e)
            {
                OnLoadingFailed();
            }
        }

        public void RedrawImage()
        {
            Img.Source = _bmp.Precache((int)Img.Width, (int)Img.Height);
            InvalidateVisual();
            GC.Collect();
        }

        public event EventDelegates.MethodContainer LoadingFailed;

        public void OnLoadingFailed()
        {
            if (LoadingFailed != null) LoadingFailed();
        }
    }
}
