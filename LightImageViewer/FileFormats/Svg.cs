using LightImageViewer.Helpers;
using System.Windows.Media.Imaging;
using Svg;

namespace LightImageViewer.FileFormats
{
    public class Svg : MyImage
    {
        public Svg(MyCanvas canvas)
            :base(canvas)
        { }

        public override BitmapImage Precache(int width, int height)
        {
            var svg = SvgDocument.Open(FileList.Uri.LocalPath);
            var wScale = width / svg.Width;
            var hScale = height / svg.Height;
            var w = svg.Width * wScale;
            var h = svg.Height * hScale;
            if (svg.X.Type == SvgUnitType.Pixel)
            {
                svg.Width = w;
                svg.Height = h;
            }
            else
            {
                //svg.Transforms.Add(new Svg.Transforms.SvgScale(1 / wScale));
                //svg.Width = w;
                //svg.Height = h;
                //svg.ViewBox = new Svg.SvgViewBox(0, 0, w, h);
            }
            return svg.Draw().ToBitmapImage();
        }

        public override void GetImageParameters()
        {
            var svg = SvgDocument.Open(FileList.Uri.LocalPath);
            var bmp = svg.Draw().ToBitmapImage();
            CalculateParameters(bmp);
        }
    }
}
