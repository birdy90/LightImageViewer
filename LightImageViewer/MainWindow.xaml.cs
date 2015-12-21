using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Threading;
using LightImageViewer.Helpers;
using Microsoft.Win32;
using System.Diagnostics;
using System.Linq;
using LightImageViewer.FileFormats;
using System.Windows.Media.Imaging;

namespace LightImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Mimum size of image
        /// </summary>
        private int _minSize = 50;

        /// <summary>
        /// Max size relative to it's initial size that image can reach
        /// </summary>
        private int _maxSizeMultiplier = 10;

        /// <summary>
        /// Factor of scaling
        /// </summary>
        private double _scaleFactor = 1.1;

        /// <summary>
        /// If image is panning at the time
        /// </summary>
        private bool _panning = false;

        /// <summary>
        /// Previous mouse position
        /// </summary>
        private Point _lastPoint;

        /// <summary>
        /// If shift button is pressed
        /// </summary>
        private bool ShiftPressed { get { return (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)); } }

        /// <summary>
        /// If control button is pressed
        /// </summary>
        private bool CtrlPressed { get { return (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)); } }

        /// <summary>
        /// If alt button is pressed
        /// </summary>
        private bool AltPressed { get { return (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)); } }

        #region Конструкторы

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            FileList.PathChanged += UpdateLabels;
            canvas.ImageStartLoading += ToggleLoadingRect;
            canvas.ImageLoaded += ToggleLoadingRect;
        }

        #endregion


        #region Методы

        /// <summary>
        /// Close the application
        /// </summary>
        public void CloseApplication()
        {
            Close();
        }

        /// <summary>
        /// Click on window close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CloseApplication();
        }

        /// <summary>
        /// Main window loaded handler
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
        /// Form rendering finished handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnContentRendered(EventArgs e)
        {
            // получаем открываемую картинку и передаёт её в контрол отображения
            // передать изображение на отрисовку можно только после того, как все контролы будут отрисованы на экране
            var dict = Environment.GetCommandLineArgs();
            if (dict.Length == 1) return;
            FileList.CurrentPath = dict[1];
        }


        #region Масштабирование

        /// <summary>
        /// Scaling of an image
        /// </summary>
        /// <param name="delta">Scale change direction</param>
        /// <param name="scaleCenter">Anchor point for scaling</param>
        public bool Scale(int delta, Point scaleCenter)
        {
            bool resized = false;
            var oldWidth = (int)canvas.Img.Width;
            var oldHeight = (int)canvas.Img.Height;
            var width = 0;
            var height = 0;
            
            if (delta < 0)
            {
                // decrease
                if (ImageParameters.WidthBigger ? oldHeight > _minSize : oldWidth > _minSize)
                {
                    width = (int)Math.Max(DecreaseSize(oldWidth), _minSize);
                    height = (int)Math.Max(DecreaseSize(oldHeight), _minSize);
                    resized = true;
                }
            }
            else
            {
                // increase
                if (ImageParameters.WidthBigger ?
                    oldWidth < _maxSizeMultiplier * canvas.ActualWidth:
                    oldHeight < _maxSizeMultiplier * canvas.ActualHeight)
                {
                    width = (int)Math.Min(IncreaseSize(oldWidth), _maxSizeMultiplier * canvas.Img.Width);
                    height = (int)Math.Min(IncreaseSize(oldHeight), _maxSizeMultiplier * canvas.Img.Height);
                    resized = true;
                }
            }

            // if size haven't changed then we don't need to rescale image
            if (!resized) return resized;
            
            // set new image dimensions
            canvas.Img.Width = width;
            canvas.Img.Height = height;

            canvas.Recache();

            // set new image position based on anchor point
            var mp = scaleCenter;
            var a = (oldHeight - height) * (canvas.ImgTop - mp.Y) / oldHeight;
            var b = (oldWidth - width) * (canvas.ImgLeft - mp.X) / oldWidth;
            canvas.ImgLeft -= b;
            canvas.ImgTop -= a;

            canvas.InvalidateVisual();
            OnImageChanged();
            return resized;
        }

        /// <summary>
        /// Decrease value
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Decreased value</returns>
        public double DecreaseSize(double value)
        {
            return value /= _scaleFactor;
        }

        /// <summary>
        /// Increase value
        /// </summary>
        /// <param name="value">Old value</param>
        /// <returns>Increased value</returns>
        public double IncreaseSize(double value)
        {
            return value *= _scaleFactor;
        }

        #endregion
        

        #region Взаимодействие с пользователем
        
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!ShiftPressed && !CtrlPressed && !AltPressed)
            {
                Scale(e.Delta, e.GetPosition(canvas));
            }
            if (CtrlPressed)
            {
                var scrollSize = 40;
                var center = new Point(ActualWidth / 2d, 0d);
                var scaled = true;
                while (canvas.Img.Width < ActualWidth * 2d / 5d && scaled)
                {
                    canvas.ImgLeft = center.X - canvas.Img.Width / 2d;
                    canvas.ImgTop = 0d;
                    scaled = Scale(1, center);
                }
                if (canvas.ImgTop == 0)
                    canvas.ImgTop = scrollSize * 1.5;
                canvas.ImgTop += scrollSize * Math.Sign(e.Delta);
                canvas.InvalidateVisual();
            }
        }
        
        private void imageField_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _panning = false;
        }

        private void imageField_MouseMove(object sender, MouseEventArgs e)
        {
            if (_panning)
            {
                var newPoint = e.GetPosition(this);
                var diff = _lastPoint - newPoint;
                _lastPoint = newPoint;

                canvas.ImgLeft -= diff.X;
                canvas.ImgTop -= diff.Y;
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
            if (canvas.Img == null) return;

            var centerPoint = new Point(canvas.ImgLeft + canvas.Img.Width / 2d, canvas.ImgTop + canvas.Img.Height / 2d);

            if (!ShiftPressed && !CtrlPressed && !AltPressed)
                switch (e.Key)
                {
                    case Key.Escape:
                        CloseApplication();
                        break;
                    case Key.Down:
                        Scale(-1, centerPoint);
                        canvas.InvalidateVisual();
                        break;
                    case Key.Up:
                        Scale(1, centerPoint);
                        canvas.InvalidateVisual();
                        break;
                    case Key.Left:
                        if (FileList.GetPreviousImage()) break;
                        CloseApplication();
                        return;
                    case Key.Right:
                        if (FileList.GetNextImage()) break;
                        CloseApplication();
                        return;
                    case Key.P:
                        var psPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\photoshop.exe\", "", null);
                        if (psPath != null)
                            Process.Start(psPath, FileList.CurrentPath);
                        break;
                    case Key.Delete:
                        var result1 = MessageBox.Show("Delete file?", "Important Question", MessageBoxButton.YesNo);
                        if (result1 == MessageBoxResult.No) return;
                        if (File.Exists(FileList.CurrentPath))
                        {
                            while (!IsFileReady(FileList.CurrentPath))
                                Thread.Sleep(100);
                            File.Delete(FileList.CurrentPath);
                        }
                        var _oldIndex = FileList.CurrentFileIndex;
                        FileList.ReloadFilesList();
                        if (_oldIndex == FileList.Count)
                            _oldIndex = FileList.Count - 1;
                        if (FileList.Count == 0)
                        {
                            CloseApplication();
                            return;
                        }
                        FileList.CurrentFileIndex = _oldIndex;
                        break;
                    case Key.D0:
                    case Key.NumPad0:
                        ImageParameters.CalculateParameters(ImageParameters.BmpWidth, ImageParameters.BmpHeight, canvas);
                        canvas.Recache();
                        canvas.InvalidateVisual();
                        break;
                }
            if (CtrlPressed)
                switch (e.Key)
                {
                    case Key.C:
                        Clipboard.SetImage((BitmapSource)canvas.Img.Source);
                        break;
                    case Key.D:
                        var psPath = "explorer.exe";
                        if (psPath != null)
                            Process.Start(psPath, FileList.CurrentDirectory);
                        break;
                    case Key.Left:
                        if (!(canvas.Bmp is IMultiPages)) return;
                        (canvas.Bmp as IMultiPages).PreviousPage();
                        UpdateLabels();
                        canvas.Recache();
                        break;
                    case Key.Right:
                        if (!(canvas.Bmp is IMultiPages)) return;
                        (canvas.Bmp as IMultiPages).NextPage();
                        canvas.Recache();
                        UpdateLabels();
                        break;
                }
        }

        #endregion


        /// <summary>
        /// Updating labels on a form
        /// </summary>
        public void UpdateLabels()
        {
            var pages = "";
            if (canvas.Bmp is IMultiPages)
            {
                var mp = canvas.Bmp as IMultiPages;
                if (mp.PagesCount > 1)
                    pages = string.Format(" page {0} from {1}", mp.CurrentPage + 1, mp.PagesCount);
            }
            if (FileList.CurrentPath != null)
            {
                labelName.Content = FileList.CurrentPath.Split('\\').Last() + pages;
                labelPath.Content = FileList.CurrentPath;
            }
            else
            {
                labelName.Content = "not found";
                labelPath.Content = FileList.CurrentDirectory + "*not found*";
            }
            labelCount.Content = string.Format("{0} / {1}", FileList.CurrentFileIndex + 1, FileList.Count);
        }
        
        /// <summary>
        /// Set visibility of "loading" label
        /// </summary>
        public void ToggleLoadingRect()
        {
            if (loadingRect.Visibility == Visibility.Hidden)
                loadingRect.Visibility = Visibility.Visible;
            else
                loadingRect.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Check if file is unlocked and can be deleted (from SO)
        /// </summary>
        /// <param name="filename">Checked file</param>
        /// <returns></returns>
        public static bool IsFileReady(string filename)
        {
            // if the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    if (inputStream.Length > 0)
                        return true;
                    else
                        return false;
            }
            catch (Exception) { return false; }
        }

        #endregion


        public event EventDelegates.MethodContainer ImageChanged;

        public void OnImageChanged()
        {
            if (ImageChanged != null) ImageChanged();
        }
    }
}
