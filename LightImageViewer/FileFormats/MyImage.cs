using LightImageViewer.Helpers;
using System;
using System.Windows.Media.Imaging;

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
            bmp.DecodePixelWidth = Math.Min(width, ImageParameters.BmpWidth);
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
            ImageParameters.CalculateParameters(bmp.PixelWidth, bmp.PixelHeight, _canvas);
        }

        public event EventDelegates.MethodContainer ImageLoaded;

        public void OnImageLoaded()
        {
            if (ImageLoaded != null) ImageLoaded();
        }
    }
}
