using LightImageViewer.Helpers;
using System;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace LightImageViewer.FileFormats
{
    public class MyImage
    {
        protected MyCanvas _canvas;

        public MyImage(MyCanvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// Перекэширование изображения. Необходимо для подзагрузки новой копии изображения,
        /// плотность пикселей которого будет совпадать с плотностью пикселей экрана. Таким образом,
        /// будет загружено минимально возможное изображение, которое не будет иметь размытость.
        /// </summary>
        /// <param name="width">Ширина изображения на экране</param>
        /// <param name="height">Высота изображения на экране</param>
        /// <returns></returns>
        public virtual BitmapImage Precache(int width, int height)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            // уловие, в зависимости от того, горизонтальное изображение или вертикальное, указывает битмапу, 
            // каков требуемый размер изображения в пикселях. Задаётся только одно измерение, второе будет 
            // сформировано автоматически в соответствии с соотношением сторон изображения. "Загружаемый размер" 
            // не должен превышать настоящий размер изображения
            if (ImageParameters.WidthBigger)
                bmp.DecodePixelWidth = Math.Min(width, ImageParameters.BmpWidth);
            else
                bmp.DecodePixelHeight = Math.Min(height, ImageParameters.BmpHeight);
            bmp.UriSource = FileList.Uri;
            bmp.EndInit();
            return bmp;
        }

        public virtual void GetImageParameters()
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.None;
            bmp.UriSource = FileList.Uri;
            bmp.EndInit();
            CalculateParameters(bmp);
        }

        protected void CalculateParameters(BitmapImage bmp)
        {
            ImageParameters.BmpHeight = bmp.PixelHeight;
            ImageParameters.BmpWidth = bmp.PixelWidth;
            if (ImageParameters.BmpHeight > _canvas.ActualHeight)
                ImageParameters.Hcount = (int)Math.Ceiling(ImageParameters.BmpHeight / _canvas.ActualHeight);
            if (ImageParameters.BmpWidth > _canvas.ActualWidth)
                ImageParameters.Wcount = (int)Math.Ceiling(ImageParameters.BmpWidth / _canvas.ActualWidth);
            ImageParameters.Aspect = (double)ImageParameters.BmpWidth / ImageParameters.BmpHeight;

            ImageParameters.WidthBigger = false;
            if (_canvas.ActualWidth / _canvas.ActualHeight < ImageParameters.Aspect)
                ImageParameters.WidthBigger = true;

            var usedSize = 0d;
            if (ImageParameters.WidthBigger)
            {
                usedSize = Math.Min(_canvas.ActualWidth, ImageParameters.BmpWidth);
                _canvas.Img.Width = usedSize;
                _canvas.Img.Height = usedSize / ImageParameters.Aspect;
                _canvas.ImgTop = _canvas.ActualHeight / 2d - _canvas.Img.Height / 2d;
                _canvas.ImgLeft = _canvas.ActualWidth / 2d - usedSize / 2d;
            }
            else
            {
                usedSize = Math.Min(_canvas.ActualHeight, ImageParameters.BmpHeight);
                _canvas.Img.Height = usedSize;
                _canvas.Img.Width = usedSize * ImageParameters.Aspect;
                _canvas.ImgTop = _canvas.ActualHeight / 2d - usedSize / 2d;
                _canvas.ImgLeft = _canvas.ActualWidth / 2d - _canvas.Img.Width / 2d;
            }
        }
    }
}
