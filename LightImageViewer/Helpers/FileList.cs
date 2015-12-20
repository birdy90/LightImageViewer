using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LightImageViewer.Helpers
{
    public static class FileList
    {
        private static string _currentPath;

        private static string _currentDirectory;

        private static Uri _uri;

        private static int _currentFileIndex;

        /// <summary>
        /// Список файлов в текущей директории
        /// </summary>
        private static List<string> _filenames = new List<string>();

        public static string CurrentPath
        {
            set
            {
                if (File.Exists(value))
                {
                    _currentPath = value;
                    _currentDirectory = Path.GetDirectoryName(_currentPath);
                    _uri = new Uri(CurrentPath);
                    OnPathChanged();
                }
                else
                {
                    _currentDirectory = Path.GetDirectoryName(value);
                    LoadStubImage();
                }
            }
            get { return _currentPath; }
        }

        public static string CurrentDirectory
        {
            get { return _currentDirectory; }
            set { _currentDirectory = value; }
        }

        public static int CurrentFileIndex
        {
            get { return _currentFileIndex; }
            set
            {
                _currentFileIndex = value;
                CurrentPath = _filenames[value];
            }
        }

        public static Uri Uri { get { return _uri; } }
        public static int Count { get { return _filenames.Count(); } }
        public static string CurrentFileExtension { get { return _uri.Segments.Last().Split('.').Last().ToLower(); } }

        static FileList()
        {
            PathChanged += ReloadFilesList;
        }

        /// <summary>
        /// Загружаем список файлов (с которыми работает программа) из текущей директории
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
