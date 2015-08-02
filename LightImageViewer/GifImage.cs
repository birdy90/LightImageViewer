using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace LightImageViewer
{
    public class GifImage : Image
    {
        GifBitmapDecoder _gf;
        Int32Animation _anim;
        bool _animationIsWorking = false;

        /// <summary>
        /// Индес отображаемого кадра GIF-а.
        /// </summary>
        public int FrameIndex
        {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }

        /// <summary>
        /// Свойство зависимоти - индес отображаемого кадра GIF-а.
        /// </summary>
        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new UIPropertyMetadata(0, ChangingFrameIndex));

        private Uri _gifSource;
        private double _framesPerSecond = 10.0;

        static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            var ob = (GifImage)obj;
            ob.Source = ob._gf.Frames[(int)ev.NewValue];

            ob.InvalidateVisual();
        }

        /// <summary>
        /// Адрес отображаемого GIF-а.
        /// </summary>
        public Uri GifSource
        {
            get { return _gifSource; }
            set
            {
                _gifSource = value;
                RefreshGif();
            }
        }

        /// <summary>
        /// Скорость анимации GIF-а (кадров в секунду) 
        /// </summary>
        public double FramesPerSecond
        {
            get { return _framesPerSecond; }
            set
            {
                _framesPerSecond = value;
                RefreshGif();
            }
        }

        private void RefreshGif()
        {
            _gf = new GifBitmapDecoder(_gifSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            if (_gf.Frames != null)
            {
                _anim = new Int32Animation(0,
                    _gf.Frames.Count - 1,
                    new Duration(TimeSpan.FromSeconds(_gf.Frames.Count / FramesPerSecond)));
                _anim.RepeatBehavior = RepeatBehavior.Forever;
                Source = _gf.Frames[0];
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (!_animationIsWorking && _anim != null)
            {
                BeginAnimation(FrameIndexProperty, _anim);
                _animationIsWorking = true;
            }
        }
    }
}
