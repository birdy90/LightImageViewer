using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace LightImageViewer.Utilities
{
    sealed class Settings : INotifyPropertyChanged
    {
        /// <summary>
        /// Singleton implementation
        /// </summary>
        private static Settings _instance = new Settings();
        public static Settings Instance { get { return _instance; } }

        /// <summary>
        /// Gets or sets transparency of the application background
        /// </summary>
        public float BackgroundTransparency
        {
            get { return GetProperty<float>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// Gets or sets color of the application background
        /// </summary>
        public Color BackgroundColor
        {
            get { return GetProperty<Color>(); }
            set { SetProperty(value); }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        private Settings() { }

        /// <summary>
        /// Retrieve property value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public T GetProperty<T>([CallerMemberName]string propertyName = null)
        {
            try
            {
                return (T)Properties.Settings.Default[propertyName];
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Setting property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public bool SetProperty(object propertyValue, [CallerMemberName]string propertyName = null)
        {
            Properties.Settings.Default[propertyName] = propertyValue;
            Properties.Settings.Default.Save();
            OnPropertyChanged();
            return true;
        }

        /// <summary>
        /// INotifyPropertyChanged interface implementation
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
