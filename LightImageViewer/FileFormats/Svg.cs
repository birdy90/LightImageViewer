using LightImageViewer.Helpers;
using System.Windows.Media.Imaging;
using Svg;
using System;

namespace LightImageViewer.FileFormats
{
    public class Svg : MyImage
    {
        public Svg(MyCanvas canvas)
            :base(canvas)
        { }

        private SvgDocument _svg;

        public override BitmapImage Precache(int width, int height)
        {
            if (_svg.Width.Type != SvgUnitType.User)
            {
                _svg.Width = new SvgUnit(SvgUnitType.Pixel, width);
                _svg.Height = new SvgUnit(SvgUnitType.Pixel, height);
            }
            return _svg.Draw().ToBitmapImage();
        }

        public override void GetImageParameters()
        {
            _svg = SvgDocument.Open(FileList.Uri.LocalPath);

            ImageParameters.BmpHeight = (int)(float)_svg.Height;
            ImageParameters.BmpWidth = (int)(float)_svg.Width;
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
