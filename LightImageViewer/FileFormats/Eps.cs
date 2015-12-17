using System.Drawing;
using System.Windows.Media.Imaging;

namespace LightImageViewer.FileFormats
{
    public class Eps : MyImage
    {
        public Eps(MyCanvas canvas)
            :base(canvas)
        { }

        Bitmap _bmp;

        public override BitmapImage Precache(int width, int height)
        {
            /*var mi = _bmp.Clone();
            if (ImageParameters.BmpWidth > width)
                mi.Zoom(width, height);
            return mi.ToBitmap().ToBitmapImage();*/

            return null;
        }

        public override void GetImageParameters()
        {
            /*_bmp = new MagickImage(FileList.CurrentPath);
            var info = new MagickImageInfo();
            info.Read(FileList.CurrentPath);
            ImageParameters.CalculateParameters(info.Width, info.Height, _canvas);*/
        }
    }
}
