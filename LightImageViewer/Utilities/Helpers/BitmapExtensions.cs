﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace LightImageViewer.Helpers
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Converts Bitmap to BitmapImage
        /// </summary>
        /// <param name="bitmap">Desired bitmap</param>
        /// <returns>Rendered BitmapImage</returns>
        public static BitmapImage ToBitmapImage(this Bitmap bitmap, int width, int height)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                if (ImageParameters.WidthBigger)
                    bitmapImage.DecodePixelWidth = Math.Min(width, ImageParameters.BmpWidth);
                else
                    bitmapImage.DecodePixelHeight = Math.Min(height, ImageParameters.BmpHeight);
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
