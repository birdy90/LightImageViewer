using System;

namespace LightImageViewer.Helpers
{
    public static class ImageParameters
    {
        public static int Hcount = 0;
        public static int Wcount = 0;
        public static int BmpHeight = 0;
        public static int BmpWidth = 0;
        public static bool WidthBigger = false;
        public static double Aspect = 1;

        public static void CalculateParameters(int w, int h, MyCanvas _canvas)
        {
            BmpWidth = w;
            BmpHeight = h;

            if (BmpHeight > _canvas.ActualHeight)
                Hcount = (int)Math.Ceiling(BmpHeight / _canvas.ActualHeight);
            if (BmpWidth > _canvas.ActualWidth)
                Wcount = (int)Math.Ceiling(BmpWidth / _canvas.ActualWidth);
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
