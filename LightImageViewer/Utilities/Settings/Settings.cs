using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LightImageViewer.Utilities
{
    sealed class Settings : INotifyPropertyChanged
    {
        /// <summary>
        /// Singleton implementation
        /// </summary>
        private static Settings _instance = new Settings();
        public static Settings Instance { get { return _instance; } }

        public float BackgroundTransparency
        {
            get { return GetProperty<float>(); }
            set { SetProperty(value); }
        }

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

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
