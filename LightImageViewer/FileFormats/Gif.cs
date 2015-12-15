using LightImageViewer.Helpers;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace LightImageViewer.FileFormats
{
    public class Gif : MyImage
    {
        public Gif(MyCanvas canvas)
            :base(canvas)
        { }

        BitmapImage _bmp;

        public override BitmapImage Precache(int width, int height)
        {
            return _bmp;
        }

        public override void GetImageParameters()
        {
            _bmp = new BitmapImage();
            _bmp.BeginInit();
            _bmp.CacheOption = BitmapCacheOption.OnLoad;
            _bmp.UriSource = FileList.Uri;
            _bmp.EndInit();
            CalculateParameters(_bmp);
            ImageBehavior.SetAnimatedSource(_canvas.Img, _bmp);
        }
    }
}
