using System.Drawing;
using System.Windows.Media.Imaging;

namespace LightImageViewer.FileFormats
{
    /// <summary>
    /// Eps image
    /// </summary>
    public class Eps : ImageReader
    {
        /// <summary>
        /// Bitmap saved after reading
        /// </summary>
        Bitmap _bmp;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="canvas">Parent canvas object</param>
        public Eps(MyCanvas canvas)
            :base(canvas)
        { }

        public override BitmapImage Precache(int width, int height)
        {
            /*var mi = _bmp.Clone();
            if (ImageParameters.BmpWidth > width)
                mi.Zoom(width, height);
            return mi.ToBitmap().ToBitmapImage(width, height);*/
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
