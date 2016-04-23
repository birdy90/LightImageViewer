using LightImageViewer.Utilities.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LightImageViewer.Utilities.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private Settings _settings = Settings.Instance;

        private MainWindow _window;

        /// <summary>
        /// Window background opacity
        /// </summary>
        public float Opacity { get { return 1f - _settings.BackgroundTransparency / 100f; } }

        /// <summary>
        /// Command for opening settings window
        /// </summary>
        public ICommand OpenSettingsWindow { get; private set; }

        /// <summary>
        /// Color of window background
        /// </summary>
        public Color BackgroundColor { get { return _settings.BackgroundColor; } }

        public MainWindowViewModel(MainWindow window)
        {
            _window = window;
            OpenSettingsWindow = new ClickCommand(_window.OpenSettingsWindowHandler);

            Settings.Instance.PropertyChanged += UpdateSettings;
        }

        /// <summary>
        /// Updating properties when needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void UpdateSettings(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Opacity");
            OnPropertyChanged("BackgroundColor");
        }
    }
}
