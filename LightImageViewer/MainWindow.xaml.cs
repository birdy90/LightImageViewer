using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;

namespace LightImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> filenames = new List<string>();
        int currentFileIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Screen currentScreen;
            currentScreen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
            Left = currentScreen.WorkingArea.X;
            Top = currentScreen.WorkingArea.Y;
            Height = currentScreen.WorkingArea.Height;
            Width = currentScreen.WorkingArea.Width;
            canvas.Background = Brushes.AliceBlue;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            var dict = Environment.GetCommandLineArgs();

            canvas.CurrentPath = dict[1];
            canvas.InvalidateVisual();
            GetFilesList();

            if (dict.Length == 1) return;
        }
        
        public void WaitToRecache()
        {
            recaching = true;
            counter++;
            while (timeToWait > 0)
            {
                timeToWait -= timeToWaitStep;
                System.Threading.Thread.Sleep(timeToWaitStep);
            }
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (canvas.CurrentImage != null)
                {
                    //canvas.CurrentImage.Source = canvas.PrecacheBmp((int)canvas.CurrentImage.Width, (int)canvas.CurrentImage.Height);
                    canvas.UpdateImageSource();
                }
            }));
            counter--;
            recaching = false;
        }

        int counter = 0;
        int timeToWait;
        int timeToWaitPreset = 350;
        int timeToWaitStep = 10;
        bool recaching = false;

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Scale(e.Delta, e.GetPosition(canvas));
        }

        public void Scale(int delta, Point scaleCenter)
        {
            if (canvas.CurrentImage == null) return;

            bool resized = false;

            var oldWidth = (int)canvas.CurrentImage.Width;
            var oldHeight = (int)canvas.CurrentImage.Height;
            var width = 0;
            var height = 0;
            var checkWidth = oldWidth > oldHeight;
            if (delta < 0)
            {
                if (checkWidth ? oldHeight > minSize : oldWidth > minSize)
                {
                    width = (int)Math.Max(DecreaseSize(oldWidth), minSize);
                    height = (int)Math.Max(DecreaseSize(oldHeight), minSize);
                    resized = true;
                }
            }
            else
            {
                if (checkWidth ?
                    oldWidth < maxSizeMultiplier * canvas.CurrentImage.Width :
                    oldHeight < maxSizeMultiplier * canvas.CurrentImage.Height)
                {
                    width = (int)Math.Min(IncreaseSize(oldWidth), maxSizeMultiplier * canvas.CurrentImage.Width);
                    height = (int)Math.Min(IncreaseSize(oldHeight), maxSizeMultiplier * canvas.CurrentImage.Height);
                    resized = true;
                }
            }

            if (!resized || !canvas.CanMakeBigger(width, height)) return;

            timeToWait = timeToWaitPreset;
            canvas.CurrentImage.Width = width;
            canvas.CurrentImage.Height = height;
            if (!recaching) Task.Factory.StartNew(WaitToRecache);

            var mp = scaleCenter;
            var a = (oldHeight - height) * (canvas.ImgTop - mp.Y) / oldHeight;
            var b = (oldWidth - width) * (canvas.ImgLeft - mp.X) / oldWidth;
            canvas.ImgLeft -= b;
            canvas.ImgTop -= a;
            //RectangleGeometry clipGeometry = new RectangleGeometry(new Rect(new Point(Top, Left), new Point(width, height)));
            //canvas.CurrentImage.Clip = clipGeometry;
            canvas.InvalidateVisual();
        }

        int minSize = 50;
        int maxSizeMultiplier = 4;

        public double DecreaseSize(double scale)
        {
            return scale /= 1.1;
        }

        public double IncreaseSize(double scale)
        {
            return scale *= 1.1;
        }

        /// <summary>
        /// Width is bigger then height
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public bool CompareSides(double w, double h)
        {
            var ar = Width / Height;
            return w > h * ar;
        }

        bool panning = false;
        Point lastPoint;
        
        private void imageField_MouseUp(object sender, MouseButtonEventArgs e)
        {
            panning = false;
        }

        private void imageField_MouseMove(object sender, MouseEventArgs e)
        {
            if (panning)
            {
                timeToWait = timeToWaitPreset;
                var newPoint = e.GetPosition(this);
                var diff = lastPoint - newPoint;
                lastPoint = newPoint;

                canvas.ImgLeft -= diff.X;
                canvas.ImgTop -= diff.Y;
                // рекэширование нужно в случае если изображение кропается
                //canvas.PrecacheBmp((int)canvas.CurrentImage.Width, (int)canvas.CurrentImage.Height);
                canvas.InvalidateVisual();

            }
        }

        private void imageField_MouseDown(object sender, MouseButtonEventArgs e)
        {
            panning = true;
            lastPoint = e.GetPosition(this);
        }

        private void imageField_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void MyWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (canvas.CurrentImage == null) return;

            var centerPoint = new Point(canvas.ImgLeft + canvas.CurrentImage.Width / 2d, canvas.ImgTop + canvas.CurrentImage.Height / 2d);
            switch (e.Key)
            {
                case Key.Down:
                    Scale(-1, centerPoint);
                    break;
                case Key.Up:
                    Scale(1, centerPoint);
                    break;
                case Key.Left:
                    GetFilesList();
                    if (currentFileIndex == 0) break;
                    currentFileIndex--;
                    canvas.CurrentPath = filenames[currentFileIndex];
                    text.Text = string.Format("{0}: {1}", currentFileIndex, canvas.CurrentPath);
                    break;
                case Key.Right:
                    GetFilesList();
                    if (currentFileIndex == filenames.Count - 1) break;
                    currentFileIndex++;
                    canvas.CurrentPath = filenames[currentFileIndex];
                    text.Text = string.Format("{0}: {1}", currentFileIndex, canvas.CurrentPath);
                    break;
            }
            canvas.InvalidateVisual();
        }

        void GetFilesList()
        {
            var path = System.IO.Path.GetDirectoryName(canvas.CurrentPath);
            filenames = GetFiles(path, "*.png|*.jpg|*.jpeg|*.gif|*.bmp|*.tif", SearchOption.TopDirectoryOnly);
            for (var i = 0; i < filenames.Count; i++)
            {
                if (string.Equals(filenames[i], canvas.CurrentPath))
                {
                    currentFileIndex = i;
                    break;
                }
            }
        }

        private static List<string> GetFiles(string sourceFolder, string filters, SearchOption searchOption)
        {
            return filters.Split('|').SelectMany(filter => Directory.GetFiles(sourceFolder, filter, searchOption)).ToList();
        }
    }
}
