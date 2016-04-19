using LightImageViewer.Helpers;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace LightImageViewer.FileFormats
{
    /// <summary>
    /// Tif image
    /// </summary>
    public class Tif : MultiPagedImage
    {
        /// <summary>
        /// Bitmap saved after reading
        /// </summary>
        Bitmap _bmp;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="canvas">Parent canvas object</param>
        public Tif(MyCanvas canvas)
            :base(canvas)
        { }

        public override BitmapImage Precache(int width, int height)
        {
            using (var fs = new FileStream(FileList.CurrentPath, FileMode.Open, FileAccess.Read))
            {
                var bmp = new Bitmap(fs);
                _pagesCount = bmp.GetFrameCount(FrameDimension.Page);
                bmp.SelectActiveFrame(FrameDimension.Page, CurrentPage);
                _bmp = new Bitmap(bmp);
            }
            return _bmp.ToBitmapImage(width, height);
        }

        public override void GetImageParameters()
        {
            CurrentPage = GetLastPagePosition(CurrentPage);
            using (var fs = new FileStream(FileList.CurrentPath, FileMode.Open, FileAccess.Read))
            {
                var bmp = new Bitmap(fs);
                _pagesCount = bmp.GetFrameCount(FrameDimension.Page);
                bmp.SelectActiveFrame(FrameDimension.Page, CurrentPage);
                _bmp = new Bitmap(bmp);
            }
            ImageParameters.CalculateParameters(
                (int)Math.Min(_bmp.Width, _canvas.ActualWidth),
                (int)Math.Min(_bmp.Height, _canvas.ActualHeight),
                _canvas);
        }
    }
}
