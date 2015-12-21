using System;

namespace LightImageViewer.Helpers
{
    public static class ImageParameters
    {
        /// <summary>
        /// Image width
        /// </summary>
        public static int BmpWidth = 0;

        /// <summary>
        /// Image height
        /// </summary>
        public static int BmpHeight = 0;

        /// <summary>
        /// Indicates if image is wider than canvas
        /// </summary>
        public static bool WidthBigger = false;

        /// <summary>
        /// Aspect ratio of an image
        /// </summary>
        public static double Aspect = 1;

        /// <summary>
        /// Calculate all image parameters on its passed dimensions
        /// </summary>
        /// <param name="w">Width of an image</param>
        /// <param name="h">Height of an image</param>
        /// <param name="_canvas">Canvas, that will be used for drawing image</param>
        public static void CalculateParameters(int w, int h, MyCanvas _canvas)
        {
            BmpWidth = w;
            BmpHeight = h;
            
            Aspect = (double)BmpWidth / BmpHeight;

            WidthBigger = false;
            if (_canvas.ActualWidth / _canvas.ActualHeight < Aspect)
                WidthBigger = true;

            var usedSize = 0d;
            if (WidthBigger)
            {
                usedSize = Math.Min(_canvas.ActualWidth, BmpWidth);
                _canvas.Img.Width = usedSize;
                _canvas.Img.Height = usedSize / Aspect;
                _canvas.ImgTop = _canvas.ActualHeight / 2d - _canvas.Img.Height / 2d;
                _canvas.ImgLeft = _canvas.ActualWidth / 2d - usedSize / 2d;
            }
            else
            {
                usedSize = Math.Min(_canvas.ActualHeight, BmpHeight);
                _canvas.Img.Height = usedSize;
                _canvas.Img.Width = usedSize * Aspect;
                _canvas.ImgTop = _canvas.ActualHeight / 2d - usedSize / 2d;
                _canvas.ImgLeft = _canvas.ActualWidth / 2d - _canvas.Img.Width / 2d;
            }
        }
    }
}
