using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Threading;
using LightImageViewer.Helpers;
using Microsoft.Win32;
using System.Diagnostics;

namespace LightImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private int _timeToWait;
        private int _timeToWaitPreset = 350;
        private int _timeToWaitStep = 10;
        private bool _recaching = false;

        private int _minSize = 50;
        private int _maxSizeMultiplier = 4;
        private double _scaleFactor = 1.1;

        private bool _panning = false;
        private Point _lastPoint;

        private bool ShiftPressed { get { return (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)); } }
        private bool CtrlPressed { get { return (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)); } }
        private bool AltPressed { get { return (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)); } }

        #region Конструкторы

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            FileList.PathChanged += ImageUpdated;
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
            FileList.CurrentPath = dict[1];
        }

        /// <summary>
        /// Методы запускающийся в отдельном потоке и используемый для того, чтобы перекэшировать отображаемое изображение,
        /// но сделать это не сразу, а с небольшой паузой. Таким образом, например при вращении колеса мыши 
        /// перекэширование будет происходить после того, как пользователь "докрутит до нужного масштаба", а не после
        /// каждого деления колёсика
        /// </summary>
        public void WaitToRecache()
        {
            if (_recaching) return;
            _recaching = true;
            while (_timeToWait > 0)
            {
                _timeToWait -= _timeToWaitStep;
                Thread.Sleep(_timeToWaitStep);
            }
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (canvas.Img != null)
                    canvas.RedrawImage();
                _recaching = false;
            }));
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

            var oldWidth = (int)canvas.Img.Width;
            var oldHeight = (int)canvas.Img.Height;
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
                    oldWidth < _maxSizeMultiplier * canvas.Img.Width :
                    oldHeight < _maxSizeMultiplier * canvas.Img.Height)
                {
                    width = (int)Math.Min(IncreaseSize(oldWidth), _maxSizeMultiplier * canvas.Img.Width);
                    height = (int)Math.Min(IncreaseSize(oldHeight), _maxSizeMultiplier * canvas.Img.Height);
                    resized = true;
                }
            }

            // если изображение не изменилось, то ничего не делаем
            if (!resized) return;

            // обновляем счетчик ожидания рекэширования
            _timeToWait = _timeToWaitPreset;

            // задаём новые размеры изображения
            canvas.Img.Width = width;
            canvas.Img.Height = height;

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
        

        #region Взаимодействие с пользователем

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Scale(e.Delta, e.GetPosition(canvas));
            OnImageChanged();
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
            if (canvas.Img == null) return;

            var centerPoint = new Point(canvas.ImgLeft + canvas.Img.Width / 2d, canvas.ImgTop + canvas.Img.Height / 2d);

            if (!ShiftPressed && !CtrlPressed && !AltPressed)
                switch (e.Key)
                {
                    case Key.Escape:
                        CloseApplication();
                        break;
                    case Key.Down:
                        // уменьшение масштаба
                        Scale(-1, centerPoint);
                        OnImageChanged();
                        break;
                    case Key.Up:
                        // увеличение масштаба
                        Scale(1, centerPoint);
                        OnImageChanged();
                        break;
                    case Key.Left:
                        FileList.RealoadFilesList();
                        if (FileList.Count == 0)
                        {
                            CloseApplication();
                            return;
                        }
                        if (FileList.CurrentFileIndex > 0)
                            FileList.CurrentFileIndex--;
                        else
                            FileList.CurrentFileIndex = 0;
                        break;
                    case Key.Right:
                        FileList.RealoadFilesList();
                        var count = FileList.Count;
                        if (count == 0)
                        {
                            CloseApplication();
                            return;
                        }
                        if (FileList.CurrentFileIndex < count - 1)
                            FileList.CurrentFileIndex++;
                        else
                            FileList.CurrentFileIndex = count - 1;
                        break;
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
                                Thread.Sleep(50);
                            File.Delete(FileList.CurrentPath);
                        }
                        var _oldIndex = FileList.CurrentFileIndex;
                        FileList.RealoadFilesList();
                        canvas.Clear();
                        if (_oldIndex == FileList.Count)
                            _oldIndex = FileList.Count - 1;
                        if (_oldIndex < 0)
                        {
                            CloseApplication();
                            return;
                        }
                        else
                        {
                            FileList.CurrentFileIndex = _oldIndex;
                        }
                        break;
                }
        }

        #endregion


        public void ImageUpdated()
        {
            FileList.RealoadFilesList();
            labelName.Content = FileList.CurrentPath;
            labelCount.Content = string.Format("{0} / {1}", FileList.CurrentFileIndex + 1, FileList.Count);
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

        public event EventDelegates.MethodContainer ImageChanged;

        public void OnImageChanged()
        {
            if (ImageChanged != null) ImageChanged();
        }
    }
}
