using LightImageViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LightImageViewer.FileFormats
{
    /// <summary>
    /// Base class for images containing multiple pages
    /// </summary>
    public class MultiPagedImage : ImageReader, IMultiPages
    {
        /// <summary>
        /// Number of pages
        /// </summary>
        protected int _pagesCount;

        /// <summary>
        /// List of strings, declaring file page positions
        /// </summary>
        private static List<string> _positions;

        /// <summary>
        /// Path to file, where positions are stored
        /// </summary>
        private static string _positionsFileDirectory;

        /// <summary>
        /// Full file name, where positions are stored
        /// </summary>
        private static string _positionsFile;

        /// <summary>
        /// Gets or sets current page of image
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets number of pages
        /// </summary>
        public int PagesCount { get { return _pagesCount; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="canvas">Parent canvas object</param>
        public MultiPagedImage(MyCanvas canvas)
            :base(canvas)
        { }

        static MultiPagedImage()
        {
            _positionsFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LightImageViewer\\";
            _positionsFile = _positionsFileDirectory + "pages.txt";
            if (!File.Exists(_positionsFile))
            {
                Directory.CreateDirectory(_positionsFileDirectory);
                File.Create(_positionsFile).Dispose();
            }
            _positions = File.ReadLines(_positionsFile).ToList();
        }

        /// <summary>
        /// Get the last page position image was loaded
        /// </summary>
        /// <returns></returns>
        public static int GetLastPagePosition(int index)
        {
            var position = _positions.FirstOrDefault(s => s.Split(';')[1] == FileList.CurrentPath);
            if (position == null)
            {
                _positions.Add(string.Format("{0};{1};{2}", index, FileList.CurrentPath, DateTime.Now.ToString("d")));
                File.WriteAllLines(_positionsFile, _positions);
                return 0;
            }
            return Convert.ToInt32(position.Split(';')[0]);
        }

        /// <summary>
        /// Remember position of current opened image
        /// </summary>
        public static void RememberPosition(int index)
        {
            var sw = new Stopwatch();
            sw.Start();
            DeleteOldPositions();
            var position = _positions.FirstOrDefault(s => s.Split(';')[1] == FileList.CurrentPath);
            var line = string.Format("{0};{1};{2}", index, FileList.CurrentPath, DateTime.Now.ToString("d"));
            if (position != null)
                _positions.Remove(position);
            _positions.Add(line);
            File.WriteAllLines(_positionsFile, _positions);
            sw.Stop();
            var t = sw.ElapsedMilliseconds;
        }

        /// <summary>
        /// Delete that records about positions that is old enough
        /// </summary>
        public static void DeleteOldPositions()
        {
            var newPositions = new List<string>();
            foreach (var line in _positions)
            {
                var date = DateTime.ParseExact(line.Split(';')[2], "d", CultureInfo.InvariantCulture);
                var dt = DateTime.Now - date;
                if (dt <= TimeSpan.FromDays(120))
                    newPositions.Add(line);
            }
            _positions = newPositions;
        }

        /// <summary>
        /// Opening the next page of an image
        /// </summary>
        public void NextPage()
        {
            if (CurrentPage < PagesCount - 1)
                CurrentPage++;
            RememberPosition(CurrentPage);
            Precache(ImageParameters.BmpWidth, ImageParameters.BmpHeight);
        }

        /// <summary>
        /// Opening the previous pae of an image
        /// </summary>
        public void PreviousPage()
        {
            if (CurrentPage > 0)
                CurrentPage--;
            RememberPosition(CurrentPage);
            Precache(ImageParameters.BmpWidth, ImageParameters.BmpHeight);
        }
    }
}
