﻿using System;
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
                    RealoadFilesList();
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

        /// <summary>
        /// Загружаем список файлов (с которыми работает программа) из текущей директории
        /// </summary>
        public static void RealoadFilesList()
        {
            var vf = new List<string>() { "svg" };
            var bf = new List<string>() { "png", "bmp", "tif", "tiff", "jpg", "jpeg", "psd", "odd", "ico" };
            var af = new List<string>() { "gif" };

            var searchPattern = new Regex(
                string.Format(@"({0}|{1}|{2})$", // webp | ai | pdf | tga
                string.Join("|", bf),
                string.Join("|", vf),
                string.Join("|", af)),
                RegexOptions.IgnoreCase);
            var files = Directory.EnumerateFiles(CurrentDirectory, "*.*", SearchOption.TopDirectoryOnly);
            var search = files.Where(f => searchPattern.IsMatch(f));
            _filenames = search.ToList();
            _currentFileIndex = _filenames.IndexOf(_currentPath);
        }

        public static bool GetPreviousImage()
        {
            RealoadFilesList();
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
            RealoadFilesList();
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
            RealoadFilesList();
            OnPathChanged();
        }
        
        public static event EventDelegates.MethodContainer PathChanged;

        public static void OnPathChanged()
        {
            if (PathChanged != null) PathChanged();
        }
    }
}
