using LightImageViewer.Helpers;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace LightImageViewer.FileFormats
{
    public class MyImage
    {
        protected MyCanvas _canvas;

        protected Stream _inputStream;

        protected byte[] _bytes;

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
            //_inputStream.Seek(0, SeekOrigin.Begin);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            if (ImageParameters.WidthBigger)
                bmp.DecodePixelWidth = Math.Min(width, ImageParameters.BmpWidth);
            else
                bmp.DecodePixelHeight = Math.Min(height, ImageParameters.BmpHeight);
            bmp.UriSource = FileList.Uri;
            //bmp.StreamSource = _inputStream;
            bmp.EndInit();
            //bmp.Freeze();
            return bmp;
        }

        public virtual void GetImageParameters()
        {
            //_inputStream = new FileStream(FileList.CurrentPath, FileMode.Open);
            //_bytes = new byte[_inputStream.Length];
            //_inputStream.Read(_bytes, 0, (int)_inputStream.Length);
            //_inputStream.Close();
            //_inputStream = new MemoryStream(_bytes);

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.None;
            bmp.UriSource = FileList.Uri;
            //bmp.StreamSource = _inputStream;
            bmp.EndInit();
            //bmp.Freeze();
            ImageParameters.CalculateParameters(bmp.PixelWidth, bmp.PixelHeight, _canvas);
        }
    }
}
