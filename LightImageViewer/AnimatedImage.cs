using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace LightImageViewer
{/// <summary>
    /// Control the "Images", which supports animated GIF.
    /// </summary>
    public class AnimatedImage : Image
    {
        #region Public properties

        /// <summary>
        /// Gets / sets the number of the current frame.
        /// </summary>
        public int FrameIndex
        {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }

        /// <summary>
        /// Gets / sets the image that will be drawn.
        /// </summary>
        public new ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        #endregion

        #region Protected interface

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Source property.
        /// </summary>
        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs aEventArgs)
        {
            ClearAnimation();

            BitmapImage lBitmapImage = aEventArgs.NewValue as BitmapImage;

            if (lBitmapImage == null)
            {
                ImageSource lImageSource = aEventArgs.NewValue as ImageSource;
                base.Source = lImageSource;
                return;
            }

            if (!IsAnimatedGifImage(lBitmapImage))
            {
                base.Source = lBitmapImage;
                return;
            }

            PrepareAnimation(lBitmapImage);
        }

        #endregion

        #region Private properties

        private Int32Animation Animation { get; set; }
        private GifBitmapDecoder Decoder { get; set; }
        private bool IsAnimationWorking { get; set; }

        private static BitmapFrame _cachedFrame;

        #endregion

        #region Private methods

        private void ClearAnimation()
        {
            if (Animation != null)
            {
                BeginAnimation(FrameIndexProperty, null);
            }

            IsAnimationWorking = false;
            _cachedFrame = null;
            Animation = null;
            Decoder = null;
        }

        private BitmapCacheOption _cacheOption = BitmapCacheOption.None;
        private BitmapCreateOptions _creationOption = BitmapCreateOptions.PreservePixelFormat;// .PreservePixelFormat;

        private void PrepareAnimation(BitmapImage aBitmapImage)
        {
            Debug.Assert(aBitmapImage != null);

            if (aBitmapImage.UriSource != null)
            {
                Decoder = new GifBitmapDecoder(aBitmapImage.UriSource, _creationOption, _cacheOption);
            }
            else
            {
                aBitmapImage.StreamSource.Position = 0;
                Decoder = new GifBitmapDecoder(aBitmapImage.StreamSource, _creationOption, _cacheOption);
            }

            var fps = 24d;
            Animation =
                new Int32Animation(
                    0,
                    Decoder.Frames.Count - 1,
                    new Duration(
                        new TimeSpan(
                            0,
                            0,
                            0,
                            Decoder.Frames.Count / (int)fps,
                            (int)((Decoder.Frames.Count / fps - Decoder.Frames.Count / (int)fps) * 1000))))
                {
                    RepeatBehavior = RepeatBehavior.Forever
                };
            
            base.Source = Decoder.Frames[0];
            BeginAnimation(FrameIndexProperty, Animation);
            IsAnimationWorking = true;
        }

        private bool IsAnimatedGifImage(BitmapImage aBitmapImage)
        {
            Debug.Assert(aBitmapImage != null);

            bool lResult = false;
            if (aBitmapImage.UriSource != null)
            {
                BitmapDecoder lBitmapDecoder = BitmapDecoder.Create(aBitmapImage.UriSource, _creationOption, _cacheOption);
                lResult = lBitmapDecoder is GifBitmapDecoder;
            }
            else if (aBitmapImage.StreamSource != null)
            {
                try
                {
                    long lStreamPosition = aBitmapImage.StreamSource.Position;
                    aBitmapImage.StreamSource.Position = 0;
                    GifBitmapDecoder lBitmapDecoder =
                        new GifBitmapDecoder(aBitmapImage.StreamSource, _creationOption, _cacheOption);
                    lResult = lBitmapDecoder.Frames.Count > 1;

                    aBitmapImage.StreamSource.Position = lStreamPosition;
                }
                catch
                {
                    lResult = false;
                }
            }

            return lResult;
        }

        private static void ChangingFrameIndex
            (DependencyObject aObject, DependencyPropertyChangedEventArgs aEventArgs)
        {
            AnimatedImage lAnimatedImage = aObject as AnimatedImage;

            if (lAnimatedImage == null || !lAnimatedImage.IsAnimationWorking)
            {
                return;
            }

            int lFrameIndex = (int)aEventArgs.NewValue;
            //_cachedFrame = JoinFrames(_cachedFrame, lAnimatedImage.Decoder.Frames[lFrameIndex]);
            ((Image)lAnimatedImage).Source = lAnimatedImage.Decoder.Frames[lFrameIndex];
            lAnimatedImage.Decoder.Frames[lFrameIndex].Freeze();
            var fi = lAnimatedImage.Decoder.Frames[lFrameIndex];
            lAnimatedImage.InvalidateVisual();
        }

        private static BitmapFrame JoinFrames(BitmapFrame bf, BitmapFrame preBF)
        {
            int imageWidth = bf.PixelWidth;
            int imageHeight = bf.PixelHeight;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(bf, new Rect(0, 0, imageWidth, imageHeight));
                drawingContext.DrawImage(preBF, new Rect(0, 0, imageWidth, imageHeight));
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Default);
            bmp.Render(drawingVisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            return BitmapFrame.Create(bmp);

            //using (Stream stream = File.Create(pathTileImage))
            //    encoder.Save(stream);
        }

        /// <summary>
        /// Handles changes to the Source property.
        /// </summary>
        private static void OnSourceChanged
            (DependencyObject aObject, DependencyPropertyChangedEventArgs aEventArgs)
        {
            ((AnimatedImage)aObject).OnSourceChanged(aEventArgs);
        }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// FrameIndex Dependency Property
        /// </summary>
        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register(
                "FrameIndex",
                typeof(int),
                typeof(AnimatedImage),
                new UIPropertyMetadata(0, ChangingFrameIndex));

        /// <summary>
        /// Source Dependency Property
        /// </summary>
        public new static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                "Source",
                typeof(ImageSource),
                typeof(AnimatedImage),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnSourceChanged));

        #endregion
    }
}
