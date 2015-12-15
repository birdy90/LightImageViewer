﻿using LightImageViewer.Helpers;
using System;
using System.Windows.Media.Imaging;

namespace LightImageViewer.FileFormats
{
    public class Ico : MyImage
    {
        public Ico(MyCanvas canvas)
            :base(canvas)
        { }

        public override BitmapImage Precache(int width, int height)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            if (ImageParameters.WidthBigger)
                bmp.DecodePixelWidth = Math.Min(width, ImageParameters.BmpWidth);
            else
                bmp.DecodePixelHeight = Math.Min(height, ImageParameters.BmpHeight);
            bmp.UriSource = FileList.Uri;
            bmp.EndInit();
            return bmp;
        }

        public override void GetImageParameters()
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = FileList.Uri;
            bmp.EndInit();
            CalculateParameters(bmp);
        }
    }
}
