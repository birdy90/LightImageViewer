using LightImageViewer.Helpers;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace LightImageViewer.FileFormats
{
    public class Tif : MyImage, IMultiPages
    {
        public int CurrentPage { get; set; }

        public int PagesCount { get { return _bmp.GetFrameCount(FrameDimension.Page); } }

        public Tif(MyCanvas canvas)
            :base(canvas)
        { }

        Bitmap _bmp;

        public override BitmapImage Precache(int width, int height)
        {
            _bmp.SelectActiveFrame(FrameDimension.Page, CurrentPage);            
            return _bmp.ToBitmapImage();
        }

        public override void GetImageParameters()
        {
            _bmp = (Bitmap)Image.FromFile(FileList.CurrentPath);
            CurrentPage = 0;
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
