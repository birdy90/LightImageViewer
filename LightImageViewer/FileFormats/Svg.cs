using LightImageViewer.Helpers;
using System.Windows.Media.Imaging;
using Svg;

namespace LightImageViewer.FileFormats
{
    /// <summary>
    /// Svg image
    /// </summary>
    public class Svg : ImageReader
    {
        /// <summary>
        /// Opened SVG document
        /// </summary>
        private SvgDocument _svg;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="canvas">Parent canvas object</param>
        public Svg(MyCanvas canvas)
            :base(canvas)
        { }

        public override BitmapImage Precache(int width, int height)
        {
            if (_svg.Width.Type != SvgUnitType.User && _svg.Width.Type != SvgUnitType.Millimeter)
            {
                _svg.Width = new SvgUnit(SvgUnitType.Pixel, width);
                _svg.Height = new SvgUnit(SvgUnitType.Pixel, height);
            }
            var bmp = _svg.Draw().ToBitmapImage(width, height);
            //bmp.Freeze();
            return bmp;
        }

        public override void GetImageParameters()
        {
            _svg = SvgDocument.Open(FileList.Uri.LocalPath);
            ImageParameters.CalculateParameters((int)(float)_svg.Width, (int)(float)_svg.Height, _canvas);
        }
    }
}
