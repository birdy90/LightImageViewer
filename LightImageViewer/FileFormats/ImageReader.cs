using LightImageViewer.Helpers;
using System;
using System.Windows.Media.Imaging;

namespace LightImageViewer.FileFormats
{
    /// <summary>
    /// Base class for loading images into viewer
    /// </summary>
    public class ImageReader
    {
        /// <summary>
        /// Parent canvas object
        /// </summary>
        protected MyCanvas _canvas;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="canvas">Parent canvas object</param>
        public ImageReader(MyCanvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// Recache (reread or just peek only needed pixel) image to fit it to desired
        /// size on display. Used after loading and after scaling of image
        /// </summary>
        /// <param name="width">Width of image on a screen</param>
        /// <param name="height">Height of image on a screen</param>
        /// <returns>Retuns <see cref="BitmapImage"/> to set it as a source for Image object on canvas</returns>
        public virtual BitmapImage Precache(int width, int height)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            if (ImageParameters.WidthBigger)
                bmp.DecodePixelWidth = Math.Min(width, ImageParameters.BmpWidth);
            else
                bmp.DecodePixelHeight = Math.Min(height, ImageParameters.BmpHeight);
            bmp.UriSource = FileList.Uri;
            bmp.EndInit();
            return bmp;
        }

        /// <summary>
        /// Get image size and calculate it's dimensions for display and position
        /// </summary>
        public virtual void GetImageParameters()
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.None;
            bmp.UriSource = FileList.Uri;
            bmp.EndInit();
            ImageParameters.CalculateParameters(bmp.PixelWidth, bmp.PixelHeight, _canvas);
        }
    }
}
