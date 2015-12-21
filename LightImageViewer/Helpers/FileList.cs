using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LightImageViewer.Helpers
{
    /// <summary>
    /// Files manager for opening files
    /// </summary>
    public static class FileList
    {
        /// <summary>
        /// Current file path
        /// </summary>
        private static string _currentPath;

        /// <summary>
        /// Uri of an opened image
        /// </summary>
        private static Uri _uri;

        /// <summary>
        /// Index of a file in current directory
        /// </summary>
        private static int _currentFileIndex;

        /// <summary>
        /// List of all files that can be opened byt viewer in current directory
        /// </summary>
        private static List<string> _filenames = new List<string>();

        /// <summary>
        /// Get or sets current path
        /// </summary>
        public static string CurrentPath
        {
            set
            {
                if (File.Exists(value))
                {
                    _currentPath = value;
                    CurrentDirectory = Path.GetDirectoryName(_currentPath);
                    _uri = new Uri(CurrentPath);
                    OnPathChanged();
                }
                else
                {
                    CurrentDirectory = Path.GetDirectoryName(value);
                    LoadStubImage();
                }
            }
            get { return _currentPath; }
        }

        /// <summary>
        /// Gets or sets current directory
        /// </summary>
        public static string CurrentDirectory { get; private set; }

        /// <summary>
        /// Gets or sets current index of a file. Set calls for current path change
        /// </summary>
        public static int CurrentFileIndex
        {
            get { return _currentFileIndex; }
            set
            {
                _currentFileIndex = value;
                CurrentPath = _filenames[value];
            }
        }

        /// <summary>
        /// Gets current Uri
        /// </summary>
        public static Uri Uri { get { return _uri; } }

        /// <summary>
        /// Get the number of files in current directory
        /// </summary>
        public static int Count { get { return _filenames.Count(); } }

        /// <summary>
        /// Get the extension of current opened image
        /// </summary>
        public static string CurrentFileExtension { get { return _uri.Segments.Last().Split('.').Last().ToLower(); } }

        /// <summary>
        /// Default constructor
        /// </summary>
        static FileList()
        {
            PathChanged += ReloadFilesList;
        }

        /// <summary>
        /// Loads list of files program can work with from current directory
        /// </summary>
        public static void ReloadFilesList()
        {
            var bf = new List<string>() { "svg", /*"eps", "pdf",*/ "gif", "png", "bmp", "tif", "tiff", "jpg", "jpeg", "psd", "odd", "ico" };

            var searchPattern = new Regex(string.Format(@"({0})$", string.Join("|", bf)), RegexOptions.IgnoreCase);
            var files = Directory.EnumerateFiles(CurrentDirectory, "*.*", SearchOption.TopDirectoryOnly);
            var search = files.Where(f => searchPattern.IsMatch(f));
            _filenames = search.ToList();
            _currentFileIndex = _filenames.IndexOf(_currentPath);
        }

        /// <summary>
        /// Loads previous image
        /// </summary>
        /// <returns>Returns if an image was changed</returns>
        public static bool GetPreviousImage()
        {
            ReloadFilesList();
            if (Count == 0)
                return false;

            if (CurrentFileIndex > 0)
                CurrentFileIndex--;
            else
                if (CurrentFileIndex < 0)
                    CurrentFileIndex = 0;
            return true;
        }

        /// <summary>
        /// Opens next image
        /// </summary>
        /// <returns>Return if an image was changed</returns>
        public static bool GetNextImage()
        {
            ReloadFilesList();
            var count = Count;
            if (Count == 0)
                return false;

            if (CurrentFileIndex < count - 1)
                CurrentFileIndex++;
            else
                if (CurrentFileIndex > count - 1)
                    CurrentFileIndex = count - 1;
            return true;
        }

        /// <summary>
        /// Load a stub image for cases when normal image load process failed
        /// </summary>
        public static void LoadStubImage()
        {
            _uri = new Uri("pack://application:,,,/Resources/error.png");
            OnPathChanged();
        }

        #region События

        public static event EventDelegates.MethodContainer PathChanged;

        public static void OnPathChanged()
        {
            if (PathChanged != null) PathChanged();
        }

        #endregion
    }
}
