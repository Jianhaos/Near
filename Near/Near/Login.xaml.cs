using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Near.Resources;
using Microsoft.WindowsAzure.MobileServices;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Notification;

namespace Near
{
    public partial class Login : PhoneApplicationPage
    {
        static private MobileServiceUser user;
        static IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        public static bool isLoaded = false;

        // Constructor
        public Login()
        {
            InitializeComponent();
            this.Loaded += Login_Loaded;
        }

        async void Login_Loaded(object sender, RoutedEventArgs e)// decided how to login
        {
            try
            {
                if (!isLoaded)
                {
                    if (!settings.Contains("isLogin"))// is user login or not?
                    {
                        settings.Add("isLogin", false);
                    }

                    if (!settings.Contains("locationService"))// location service on?
                    {
                        if (MessageBox.Show("Near will use your location service, turn it on now? You can turn off it in settings.", "Notification:", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                            settings.Add("locationService", true);
                        else
                            settings.Add("locationService", false);
                    }

                    if (!settings.Contains("pushService"))// location service on?
                    {
                        if (MessageBox.Show("Near will push notifications to you, turn it on now? You can turn off it in settings.", "Notification:", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                            settings.Add("pushService", true);
                        else
                            settings.Add("pushService", false);
                    }

                    if (!settings.Contains("firstLogin"))// is user first login or not?
                    {
                        // TO DO
                        if (MessageBox.Show("For your convenience I recommend you to enable lock screen notification of Near, it can help you connect with others.", "Go to settings?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            var op = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
                        }
                        settings.Add("firstLogin", true);
                        MessageBox.Show("Welcome to Near!\n\nPlease login first.\nEnter without login means you can't post anything!");
                        Frame.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if ((bool)settings["isLogin"])
                        {
                            Frame.Visibility = Visibility.Collapsed;
                            await Authenticate(settings["loginCorp"].ToString());
                        }
                        else
                            Frame.Visibility = Visibility.Visible;
                    }
                    settings.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async System.Threading.Tasks.Task Authenticate(string Corporation)
        {
            string message;

            try
            {
                switch (Corporation)
                {
                    case "Microsoft": user = await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount); break;
                    case "Facebook": user = await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook); break;
                    case "Twitter": user = await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Twitter); break;
                    case "Google": user = await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Google); break;
                }

                settings["isLogin"] = true;
                Frame.Visibility = Visibility.Collapsed;
                //message = "You are now logged with " + Corporation + " account.";
            }
            catch (InvalidOperationException)
            {
                if ((bool)settings["isLogin"])
                {
                    settings["isLogin"] = false;
                    settings.Save();
                    message = "Login failed!\nYou can login later.";
                    MessageBox.Show(message);
                    NavigationService.Navigate(new Uri("/MainPage.xaml?uid=" + "", UriKind.Relative));
                }
                settings["isLogin"] = false;
                message = "Login failed!\nPlease try again!";
                MessageBox.Show(message);
            }

            if (user != null)
            {
                if (!settings.Contains("loginCorp"))
                {
                    settings.Add("loginCorp", Corporation);// which method used to login?
                }
                else
                {
                    settings["loginCorp"] = Corporation;
                }
                isLoaded = true;
                settings["firstLogin"] = false;
                settings.Save();
                AcquirePushChannel();
                NavigationService.Navigate(new Uri("/MainPage.xaml?uid=" + user.UserId, UriKind.Relative));
            }
        }

        private async void Microsoft_Click(object sender, RoutedEventArgs e)
        {
            await Authenticate("Microsoft");
        }

        private async void Facebook_Click(object sender, RoutedEventArgs e)
        {
            await Authenticate("Facebook");
        }

        private async void Twitter_Click(object sender, RoutedEventArgs e)
        {
            await Authenticate("Twitter");
        }

        private async void Google_Click(object sender, RoutedEventArgs e)
        {
            await Authenticate("Google");
        }

        private void NoLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml?uid=" + "", UriKind.Relative));
        }

        public static void AcquirePushChannel()
        {
            try
            {
                IMobileServiceTable<Channel> channelTable = App.MobileService.GetTable<Channel>();
                var channel = new Channel { Uri = App.CurrentChannel.ChannelUri.ToString(), User = user.UserId, IsPush = (bool)settings["pushService"] };
                channelTable.InsertAsync(channel);
                // this is the most important part
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                MessageBox.Show("Opps! Seems the MS server has some problems now, so you may not receive the push notifications. Sorry about that.");
            }
        }
    }
}