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
        private const int MAX_WPA2_PASSWORD_LENGTH = 63;
        private const int MIN_WPA2_PASSWORD_LENGTH = 8;

        private ConnectionProfile conProfile;
        private static NetworkOperatorTetheringManager tetheringManager;

        private DispatcherTimer _timer;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Setup_Tethering();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (_1, _2) =>
            {
                UpdateTetheringStatus();
            };
            _timer.Start();

            if (ckAutoConn.IsChecked.Value)
            {
                if (tetheringManager.TetheringOperationalState == TetheringOperationalState.Off)
                {
                    var result = await tetheringManager.StartTetheringAsync();
                    if (result.Status == TetheringOperationStatus.Success)
                    {
                        if (ckCloseApp.IsChecked.Value)
                        {
                            Application.Current.Exit();
                        }
                    }
                }
            }
        }

        private void ConfGui_Enable(bool val)
        {
            txtSSID.IsEnabled = val;
            txtPass.IsEnabled = val;
        }

        private void UpdateTetheringStatus()
        {
            switch (tetheringManager.TetheringOperationalState)
            {
                case TetheringOperationalState.InTransition:
//                    txtStatus.Text = "Operation in progress...";
                    break;
                case TetheringOperationalState.Off:
                    ConfGui_Enable(true);
//                    txtStatus.Text = "Stopped!";
                    tgSwitch.IsEnabled = true;
                    //                    btnStartStop.Content = "Start";
                    tgSwitch.IsOn = false;
                    break;
                case TetheringOperationalState.On:
                    ConfGui_Enable(false);
//                    txtStatus.Text = "Started!";
                    tgSwitch.IsEnabled = true;
                    //                    btnStartStop.Content = "Stop";
                    tgSwitch.IsOn = true;
                    break;
                case TetheringOperationalState.Unknown:
//                    txtStatus.Text = "Unknown operation state...";
                    break;
            }
        }

        private async void TgSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            tgSwitch.IsEnabled = false;

            if (tgSwitch.IsOn &&
                (tetheringManager.TetheringOperationalState == TetheringOperationalState.Off))
            {
                bool fNewConfig = false;

                ConfGui_Enable(false);

                NetworkOperatorTetheringAccessPointConfiguration apConfig =
                tetheringManager.GetCurrentAccessPointConfiguration();

                if (txtSSID.Text != apConfig.Ssid)
                {
                    apConfig.Ssid = txtSSID.Text;
                    fNewConfig = true;
                }

                if (txtPass.Password != apConfig.Passphrase)
                {
                    apConfig.Passphrase = txtPass.Password;
                    fNewConfig = true;
                }

                if (fNewConfig)
                {
                    await tetheringManager.ConfigureAccessPointAsync(apConfig);
                }

                var result = await tetheringManager.StartTetheringAsync();
                if (result.Status != TetheringOperationStatus.Success)
                {
//                    txtStatus.Text = "Can't start!";
                }
            } else if (!tgSwitch.IsOn &&
                (tetheringManager.TetheringOperationalState == TetheringOperationalState.On))
            {
                var result = await tetheringManager.StopTetheringAsync();
                if (result.Status == TetheringOperationStatus.Success)
                {
                    Setup_Tethering();
                }
                else
                {
//                    txtStatus.Text = "Can't stop!";
                }
            }
        }

        private void TxtPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtPass.Password.Length < MIN_WPA2_PASSWORD_LENGTH)
            {
                txtPassErr.Text = "Password too short!";
            }
            else if (txtPass.Password.Length > MAX_WPA2_PASSWORD_LENGTH)
            {
                txtPassErr.Text = "Password too long!";
            }
            else
            {
                txtPassErr.Text = "";
            }
        }

        private bool Setup_Tethering()
        {
            conProfile = NetworkInformation.GetInternetConnectionProfile();
            txtWAN.Text = conProfile.ProfileName;
            tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(conProfile);
            NetworkOperatorTetheringAccessPointConfiguration apConfig = 
                tetheringManager.GetCurrentAccessPointConfiguration();
            txtSSID.Text = apConfig.Ssid;
            txtPass.Password = apConfig.Passphrase;
            return true;
        }
    }
}
