using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;

namespace Near
{
    public partial class Settings : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        string uID = "";

        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            uID = NavigationContext.QueryString["uid"];

            if ((bool)settings["locationService"])
                Loc_Ser.IsChecked = true;
            else
                Loc_Ser.IsChecked = false;
            if (uID != "")
            {
                Push_Ser.Visibility = Visibility.Visible;
                if ((bool)settings["pushService"])
                    Push_Ser.IsChecked = true;
                else
                    Push_Ser.IsChecked = false;
            }
        }

        private void Loc_Ser_Checked(object sender, RoutedEventArgs e)
        {
            settings["locationService"] = true;
            settings.Save();
        }

        private void Loc_Ser_Unchecked(object sender, RoutedEventArgs e)
        {
            settings["locationService"] = false;
            settings.Save();
        }

        private void Push_Ser_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                settings["pushService"] = true;
                settings.Save();
                Login.AcquirePushChannel();
            }
            catch (Exception)
            {
                MessageBox.Show("Turn on push failed! Please try again later.");
                Push_Ser.IsChecked = false;
            }
        }

        private void Push_Ser_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                settings["pushService"] = false;
                settings.Save();
                Login.AcquirePushChannel();
            }
            catch (Exception)
            {
                MessageBox.Show("Turn off push failed! Please try again later.");
                Push_Ser.IsChecked = true;
            }
        }

        private void email_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            new EmailComposeTask()
            {
                Subject = "Privacy Question",
                To = "freewhitebone@gmail.com",
            }.Show();
        }

        private async void btnGoToLockSettings_Click(object sender, RoutedEventArgs e)
        {
            // Launch URI for the lock screen settings screen.
            var op = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
        }
    }
}