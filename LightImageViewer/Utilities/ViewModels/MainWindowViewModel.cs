using LightImageViewer.Utilities.Helpers;
using System.ComponentModel;

namespace LightImageViewer.Utilities.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private Settings _settings = Settings.Instance;

        public float Opacity { get { return 1f - _settings.BackgroundTransparency / 100f; } }

        public MainWindowViewModel()
        {
            Settings.Instance.PropertyChanged += UpdateSettings;
        }

        void UpdateSettings(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Opacity");
        }
    }
}
