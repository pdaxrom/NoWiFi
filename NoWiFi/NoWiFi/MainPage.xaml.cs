using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Networking.NetworkOperators;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace NoWiFi
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ConnectionProfile conProfile;
        private bool fStarted;
        private static NetworkOperatorTetheringManager tetheringManager;

        public MainPage()
        {
            this.InitializeComponent();

            conProfile = NetworkInformation.GetInternetConnectionProfile();

            txtProfile.Text = conProfile.ProfileName;

            fStarted = false;
        }

        private async void BtStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (!fStarted)
            {
                tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(conProfile);
                var result = await tetheringManager.StartTetheringAsync();
                if (result.Status == TetheringOperationStatus.Success)
                {
                    fStarted = true;
                    txtStatus.Text = "Started!";
                    btStartStop.Content = "Stop";
                }
                else
                {
                    txtStatus.Text = "Can't start!";
                }
            } else
            {
                var result = await tetheringManager.StopTetheringAsync();
                if (result.Status == TetheringOperationStatus.Success)
                {
                    txtStatus.Text = "Stopped!";
                    btStartStop.Content = "Start";
                    fStarted = false;
                }
                else
                {
                    txtStatus.Text = "Can't stop!";
                }
            }
        }
    }
}
