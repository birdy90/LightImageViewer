using Ghostscript.NET.Rasterizer;
using LightImageViewer.Helpers;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace LightImageViewer.FileFormats
{
    public class Pdf :MyImage, IMultiPages
    {
        public int CurrentPage { get; set; }

        public int PagesCount { get { return _rastr.PageCount; } }

        public Pdf(MyCanvas canvas)
            :base(canvas)
        { }

        GhostscriptRasterizer _rastr;

        public override BitmapImage Precache(int width, int height)
        {
            return new Bitmap(_rastr.GetPage(120, 120, CurrentPage + 1)).ToBitmapImage();
        }

        public override void GetImageParameters()
        {
            _rastr = new GhostscriptRasterizer();
            _rastr.Open(FileList.CurrentPath);
            CurrentPage = 0;
            ImageParameters.CalculateParameters((int)_canvas.ActualWidth, (int)_canvas.ActualHeight, _canvas);
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
