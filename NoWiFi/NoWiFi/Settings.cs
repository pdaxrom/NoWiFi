using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace NoWiFi
{
    class Settings : INotifyPropertyChanged
    {
        public static Settings Instance = new Settings();

        public bool AutoConn
        {
            get => Read_Settings(nameof(AutoConn), false);
            set => Save_Settings(nameof(AutoConn), value);
        }

        private T Read_Settings<T>(string key, T defaultValue)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("AutoConn"))
            {
                return (T)ApplicationData.Current.LocalSettings.Values["AutoConn"];
            }
            if (defaultValue != null)
            {
                return defaultValue;
            }

            return default(T);
        }

        private void Save_Settings(string key, object value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
