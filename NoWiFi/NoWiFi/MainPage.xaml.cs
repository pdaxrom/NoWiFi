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

        public MainPage()
        {
            this.InitializeComponent();

            Setup_Tethering();

            if (tetheringManager.TetheringOperationalState == TetheringOperationalState.On) 
//                || (conProfile.ProfileName == "WFD_GROUP_OWNER_PROFILE"))
            {
                txtStatus.Text = "Started!";
                btnStartStop.Content = "Stop";
            }
            else
            {
                txtStatus.Text = "Stopped!";
                btnStartStop.Content = "Start";
            }
        }

        private async void BtnStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (tetheringManager.TetheringOperationalState == TetheringOperationalState.Off)
            {
                bool fNewConfig = false;
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
                if (result.Status == TetheringOperationStatus.Success)
                {
                    txtStatus.Text = "Started!";
                    btnStartStop.Content = "Stop";
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
                    btnStartStop.Content = "Start";
                    Setup_Tethering();
                }
                else
                {
                    txtStatus.Text = "Can't stop!";
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
