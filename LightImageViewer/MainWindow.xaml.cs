﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;

namespace LightImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Список файлов в текущей директории
        /// </summary>
        private List<string> _filenames = new List<string>();

        /// <summary>
        /// Индекс картинки, отображаемой в данный момент
        /// </summary>
        private int _currentFileIndex = 0;

        private int _timeToWait;
        private int _timeToWaitPreset = 350;
        private int _timeToWaitStep = 10;
        private bool _recaching = false;

        private int _minSize = 50;
        private int _maxSizeMultiplier = 4;
        private double _scaleFactor = 1.1;

        private bool _panning = false;
        private Point _lastPoint;

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion


        #region Методы

        public void CloseApplication()
        {
            Close();
        }

        /// <summary>
        /// Обработка нажатия на кнопку закрытия окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CloseApplication();
        }

        /// <summary>
        /// Обработка загрузки главного окна приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Matrix m = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;
            // окно можно растянуть (так чтобы у него не было рамки и чтобы оно не закрывало панель задач)
            // только вручную, что и сделано тут
            var currentScreen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
            Left = currentScreen.WorkingArea.X;
            Top = currentScreen.WorkingArea.Y;
            Height = currentScreen.WorkingArea.Height / m.M11;
            Width = currentScreen.WorkingArea.Width / m.M22;
        }

        /// <summary>
        /// Обработка окончания рендеринга формы
        /// </summary>
        /// <param name="e"></param>
        protected override void OnContentRendered(EventArgs e)
        {
            // получаем открываемую картинку и передаёт её в контрол отображения
            // передать изображение на отрисовку можно только после того, как все контролы будут отрисованы на экране
            var dict = Environment.GetCommandLineArgs();
            if (dict.Length == 1) return;
            LoadImage(dict[1]);
            GetFilesList();
        }

        /// <summary>
        /// Методы запускающийся в отдельном потоке и используемый для того, чтобы перекэшировать отображаемое изображение,
        /// но сделать это не сразу, а с небольшой паузой. Таким образом, например при вращении колеса мыши 
        /// перекэширование будет происходить после того, как пользователь "докрутит до нужного масштаба", а не после
        /// каждого деления колёсика
        /// </summary>
        public void WaitToRecache()
        {
            _recaching = true;
            while (_timeToWait > 0)
            {
                _timeToWait -= _timeToWaitStep;
                System.Threading.Thread.Sleep(_timeToWaitStep);
            }
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (canvas.CurrentImage != null)
                {
                    //canvas.CurrentImage.Source = canvas.PrecacheBmp((int)canvas.CurrentImage.Width, (int)canvas.CurrentImage.Height);
                    canvas.UpdateImageSource();
                }
            }));
            _recaching = false;
        }


        #region Масштабирование

        /// <summary>
        /// Масштабирование, используется в событиях вращения колеса мыши и нажатия стрелок 
        /// вверх и вниз
        /// </summary>
        /// <param name="delta">Направление изменения масштаба</param>
        /// <param name="scaleCenter">Точка, относительно которой масштабируем (используется для сдвига изобажения)</param>
        public void Scale(int delta, Point scaleCenter)
        {
            // контролируем, было ли изменено изображение. если нет, то рекэширование не трежуется
            bool resized = false;

            var oldWidth = (int)canvas.CurrentImage.Width;
            var oldHeight = (int)canvas.CurrentImage.Height;
            var width = 0;
            var height = 0;
            var checkWidth = oldWidth > oldHeight;
            if (delta < 0)
            {
                if (checkWidth ? oldHeight > _minSize : oldWidth > _minSize)
                {
                    width = (int)Math.Max(DecreaseSize(oldWidth), _minSize);
                    height = (int)Math.Max(DecreaseSize(oldHeight), _minSize);
                    resized = true;
                }
            }
            else
            {
                if (checkWidth ?
                    oldWidth < _maxSizeMultiplier * canvas.CurrentImage.Width :
                    oldHeight < _maxSizeMultiplier * canvas.CurrentImage.Height)
                {
                    width = (int)Math.Min(IncreaseSize(oldWidth), _maxSizeMultiplier * canvas.CurrentImage.Width);
                    height = (int)Math.Min(IncreaseSize(oldHeight), _maxSizeMultiplier * canvas.CurrentImage.Height);
                    resized = true;
                }
            }

            // если изображение не изменилось, то ничего не делаем
            if (!resized) return;

            // обновляем счетчик ожидания рекэширования
            _timeToWait = _timeToWaitPreset;

            // задаём новые размеры изображения
            canvas.CurrentImage.Width = width;
            canvas.CurrentImage.Height = height;

            // если кэширование не начато, то запустить поток с ожиданием
            if (!_recaching) Task.Factory.StartNew(WaitToRecache);

            // сдвигаем изображение так, чтобы мышь осталась в той же точке на картинке (масштабирование 
            // относительно указателя мыши)
            var mp = scaleCenter;
            var a = (oldHeight - height) * (canvas.ImgTop - mp.Y) / oldHeight;
            var b = (oldWidth - width) * (canvas.ImgLeft - mp.X) / oldWidth;
            canvas.ImgLeft -= b;
            canvas.ImgTop -= a;

            canvas.InvalidateVisual();
        }

        /// <summary>
        /// Уменьшение масштаба
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public double DecreaseSize(double scale)
        {
            return scale /= _scaleFactor;
        }

        /// <summary>
        /// Увеличение масштаба
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public double IncreaseSize(double scale)
        {
            return scale *= _scaleFactor;
        }

        #endregion

        /// <summary>
        /// Функция проверяет, вытянуто изображение вертикально, или горизонтально (при равенстве считаем изображение вертикальным)
        /// </summary>
        /// <param name="w">Ширина изображения</param>
        /// <param name="h">Высота изображения</param>
        /// <returns>Если истина, то изображение горизонтальное, если ложь - вертикальное</returns>
        public bool CompareSides(double w, double h)
        {
            var ar = Width / Height;
            return w > h * ar;
        }

        /// <summary>
        /// Загружаем список файлов (с которыми работает программа) из текущей директории
        /// </summary>
        void GetFilesList()
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
            var path = System.IO.Path.GetDirectoryName(canvas.CurrentPath);
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            var search = files.Where(f => searchPattern.IsMatch(f));
            _filenames = search.ToList();
            _currentFileIndex = _filenames.IndexOf(canvas.CurrentPath);

            labelCount.Content = string.Format("{0} / {1}", _currentFileIndex + 1, _filenames.Count());
        }


        #region Взаимодействие с пользователем

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Scale(e.Delta, e.GetPosition(canvas));
        }

        private void imageField_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _panning = false;
        }

        private void imageField_MouseMove(object sender, MouseEventArgs e)
        {
            if (_panning)
            {
                _timeToWait = _timeToWaitPreset;
                var newPoint = e.GetPosition(this);
                var diff = _lastPoint - newPoint;
                _lastPoint = newPoint;

                canvas.ImgLeft -= diff.X;
                canvas.ImgTop -= diff.Y;
                // рекэширование нужно в случае если изображение кропается
                //canvas.PrecacheBmp((int)canvas.CurrentImage.Width, (int)canvas.CurrentImage.Height);
                canvas.InvalidateVisual();

            }
        }

        private void imageField_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _panning = true;
            _lastPoint = e.GetPosition(this);
        }

        private void MyWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (canvas.CurrentImage == null) return;

            var centerPoint = new Point(canvas.ImgLeft + canvas.CurrentImage.Width / 2d, canvas.ImgTop + canvas.CurrentImage.Height / 2d);
            switch (e.Key)
            {
                case Key.Escape:
                    CloseApplication();
                    break;
                case Key.Down:
                    // уменьшение масштаба
                    Scale(-1, centerPoint);
                    break;
                case Key.Up:
                    // увеличение масштаба
                    Scale(1, centerPoint);
                    break;
                case Key.Left:
                    // предыдущая картинка
                    // список файлов получается ещё раз на случай изменения содержимого рабочего папки
                    GetFilesList();
                    if (_currentFileIndex == 0) _currentFileIndex = 1;
                    LoadImage(_filenames[--_currentFileIndex]);
                    break;
                case Key.Right:
                    // следующая картинка
                    // список файлов получается ещё раз на случай изменения содержимого рабочего папки
                    GetFilesList();
                    if (_currentFileIndex >= _filenames.Count - 1) _currentFileIndex = _filenames.Count - 2;
                    LoadImage(_filenames[++_currentFileIndex]);
                    break;
                case Key.P:
                    var psPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\photoshop.exe\", "", null);
                    if (psPath != null)
                        Process.Start(psPath, _filenames[_currentFileIndex]);
                    break;
                case Key.Delete:
                    var result1 = MessageBox.Show("Delete file?", "Important Question", MessageBoxButton.YesNo);
                    if (result1 == MessageBoxResult.Yes)
                    {
                        var fileToDelete = _filenames[_currentFileIndex];
                        canvas.CurrentImage = null;
                        _filenames.RemoveAt(_currentFileIndex);
                        if (_currentFileIndex == _filenames.Count)
                            _currentFileIndex--;
                        if (_currentFileIndex < 0)
                        {
                            CloseApplication();
                            return;
                        }
                        else
                        {
                            canvas.CurrentPath = _filenames[_currentFileIndex];
                            LoadImage(canvas.CurrentPath);
                        }
                        while (!IsFileReady(fileToDelete))
                            Thread.Sleep(50);
                        File.Delete(fileToDelete);
                    }
                    break;
            }
        }

        #endregion

        public void LoadImage(string filename)
        {
            labelName.Content = filename;
            labelCount.Content = string.Format("{0} / {1}", _currentFileIndex + 1, _filenames.Count());
            canvas.CurrentPath = filename;
        }

        public static bool IsFileReady(string sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                    if (inputStream.Length > 0)
                        return true;
                    else
                        return false;
            }
            catch (Exception) { return false; }
        }

        #endregion
    }
}
