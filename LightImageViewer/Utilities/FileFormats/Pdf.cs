using Ghostscript.NET.Rasterizer;
using LightImageViewer.Helpers;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace LightImageViewer.FileFormats
{
    /// <summary>
    /// Pdf document
    /// </summary>
    public class Pdf : MultiPagedImage
    {
        GhostscriptRasterizer _rasterizer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="canvas">Parent canvas object</param>
        public Pdf(MyCanvas canvas)
            :base(canvas)
        { }

        public override BitmapImage Precache(int width, int height)
        {
            return new Bitmap(_rasterizer.GetPage(120, 120, CurrentPage + 1)).ToBitmapImage(width, height);
        }

        public override void GetImageParameters()
        {
            CurrentPage = GetLastPagePosition(CurrentPage);
            _rasterizer = new GhostscriptRasterizer();
            _rasterizer.Open(FileList.CurrentPath);
            CurrentPage = 0;
            ImageParameters.CalculateParameters((int)_canvas.ActualWidth, (int)_canvas.ActualHeight, _canvas);
        }
    }
}
