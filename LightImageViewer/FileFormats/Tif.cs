using LightImageViewer.Helpers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace LightImageViewer.FileFormats
{
    public class Tif : MyImage, IMultiPages
    {
        public int CurrentPage { get; set; }

        public int PagesCount { get { return _pagesCount; } }

        public Tif(MyCanvas canvas)
            :base(canvas)
        { }

        Bitmap _bmp;
        int _pagesCount;

        public override BitmapImage Precache(int width, int height)
        {
            using (var fs = new FileStream(FileList.CurrentPath, FileMode.Open, FileAccess.Read))
            {
                var bmp = new Bitmap(fs);
                _pagesCount = bmp.GetFrameCount(FrameDimension.Page);
                bmp.SelectActiveFrame(FrameDimension.Page, CurrentPage);
                _bmp = new Bitmap(bmp);
            }
            return _bmp.ToBitmapImage();
        }

        public override void GetImageParameters()
        {
            CurrentPage = 0;
            using (var fs = new FileStream(FileList.CurrentPath, FileMode.Open, FileAccess.Read))
            {
                var bmp = new Bitmap(fs);
                _pagesCount = bmp.GetFrameCount(FrameDimension.Page);
                bmp.SelectActiveFrame(FrameDimension.Page, CurrentPage);
                _bmp = new Bitmap(bmp);
            }
            ImageParameters.CalculateParameters(_bmp.Width, _bmp.Height, _canvas);
        }

        public void NextPage()
        {
            if (CurrentPage < PagesCount - 1)
                CurrentPage++;
            Precache(1000, 1000);
        }

        public void PreviousPage()
        {
            if (CurrentPage > 0)
                CurrentPage--;
            Precache(1000, 1000);
        }
    }
}
