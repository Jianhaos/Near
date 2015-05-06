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
using Microsoft.Phone.Maps.Controls;
using System.Device.Location; // Provides the GeoCoordinate class.
using Windows.Devices.Geolocation; //Provides the Geocoordinate class.
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Maps.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Microsoft.Phone.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Near
{
    public partial class MainPage : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        //string[] AppBarIcon = { "post", "camera", "record", "findme" };
        string[] AppBarIcon = { "post", "findme", "addeals" };
        string[] AppBarItem = { "Login", "Logout", "ChatList", "Settings" };
        GeoCoordinate myGeoCoordinate = null;
        Map MyMap = new Map();
        private string uID = "";
        MapLayer myLocationLayer, postLocationLayer;
        string message = "";
        bool isLocReady = false, isPostReady = false, isAppBar = false, isChatReady = false, isIdc = false;
        DateTime now = DateTime.Now;
        double postLatLevel, postLonLevel;

        int iconHeight = 0;
        int iconWeight = 0;
        string iconPath = "";
        SolidColorBrush iconColor;

        private MobileServiceCollection<near_post, near_post> postItems;
        private MobileServiceCollection<near_chat, near_chat> chatItems;

        private IMobileServiceTable<near_post> postTable = App.MobileService.GetTable<near_post>();
        private IMobileServiceTable<near_chat> chatTable = App.MobileService.GetTable<near_chat>();

        // Data context for the local database
        private ChatDataContext chatDB;

        // Define an observable collection property that controls can bind to.
        private ObservableCollection<ChatItem> _chatItems;

        public ObservableCollection<ChatItem> ChatItems
        {
            get
            {
                return _chatItems;
            }
            set
            {
                if (_chatItems != value)
                {
                    _chatItems = value;
                    NotifyPropertyChanged("ChatItems");
                }
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            ChatRec.Fill = new SolidColorBrush(App.currentAccentColorHex);
            CountBlock.Foreground = new SolidColorBrush(App.currentAccentColorHex);
            SystemTray.SetProgressIndicator(this, App.progIdc);
            if (!settings.Contains("isTutorial"))// is user login or not?
            {
                settings.Add("isTutorial", false);
            }
            if (!bool.Parse(settings["isTutorial"].ToString()))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (MessageBox.Show("In this update, I added a new feature called \"News Sniffer\". You can hold on the map to try that.", "What's new?", MessageBoxButton.OK) == MessageBoxResult.OK)
                            settings["isTutorial"] = true;
                    });
            }
            settings.Save();

            // Create the database if it does not exist.
            //chatDB = new ChatDataContext(ChatDataContext.DBConnectionString + "Chat" + uID + ".sdf");

            //this.Loaded += MainPage_Loaded;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                //SystemTray.IsVisible = true;
                if (uID != "")
                    RefreshChatItems();
                RefreshPostItems();
                //RefreshImageItems();
                return;
            }
            try
            {
                string parameterValue = NavigationContext.QueryString["uid"];
                uID = parameterValue;
                if (uID != "")
                {
                    using (ChatDataContext db = new ChatDataContext(ChatDataContext.DBConnectionString + "Chat" + uID.Replace(":", "") + ".sdf"))
                    {
                        if (db.DatabaseExists() == false)
                        {
                            //Create the database
                            db.CreateDatabase();
                        }
                    }
                    chatDB = new ChatDataContext(ChatDataContext.DBConnectionString + "Chat" + uID.Replace(":", "") + ".sdf");
                }
                // Show my location
                ShowMyLocationOnTheMap();
                // Sample code to localize the ApplicationBar
                BuildLocalizedApplicationBar();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the app that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private void MainPage_Loaded(object sender, RoutedEventArgs e)// decided how to login
        {
            //RefreshPostItems();
        }

        private async void RefreshPostItems()
        {
            //// TODO #1: Mark this method as "async" and uncomment the following statment
            //// that defines a simple query for all postItems. 
            //postItems = await postTable.ToCollectionAsync();

            //// TODO #2: More advanced query that filters out completed postItems. 
            isPostReady = false;
            try
            {
                if (isIdc)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        App.progIdc.Text = "Loading posts... Please click \"findme\" if no response.";
                    });
                }

                IMobileServiceTableQuery<near_post> query = postTable
                    .Where(near_post => (near_post.Latitude - myMap.Center.Latitude) * (near_post.Latitude - myMap.Center.Latitude) < postLatLevel).Where(near_post => (near_post.Longitude - myMap.Center.Longitude) * (near_post.Longitude - myMap.Center.Longitude) < postLonLevel);
                //.Where(near_post => Math.Abs(myGeoCoordinate.Latitude - near_post.Latitude) < 0.01).Where(near_post => Math.Abs(myGeoCoordinate.Longitude - near_post.Longitude) < 0.01);

                postItems = await query.ToCollectionAsync();
                //postItems = await postTable.ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                //MessageBox.Show(e.Message, "Error loading postItems", //MessageBoxButton.OK);
            }
            ShowPostOnMap(postItems);
        }

        private async void RefreshChatItems()
        {
            if (!isChatReady)
            {
                isChatReady = true;
                try
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        App.progIdc.Text = "Receiving messages";
                    });
                    //ChatItems = new ObservableCollection<ChatItem>();
                    IMobileServiceTableQuery<near_chat> query = chatTable
                        .Where(near_chat => near_chat.Receiver == uID);

                    chatItems = await query.ToCollectionAsync();

                    foreach (near_chat chatItem in chatItems)
                    {
                        ChatItem newChat = new ChatItem { ItemContent = chatItem.Content, ItemThere = chatItem.Sender, ItemTime = DateTime.Now, ItemSender = false, IsRead = false };
                        // Add a to-do item to the observable collection.
                        //ChatItems.Add(newChat);

                        chatDB.ChatItems.InsertOnSubmit(newChat);
                        // Save changes to the database.
                        chatDB.SubmitChanges();
                    }
                    if (chatItems.Count > 0)
                    {
                        CountBlock.Text = chatItems.Count.ToString();
                        NewChatPanel.Visibility = Visibility.Visible;
                        NewChatPanel.Tap += (o, e) =>
                        {
                            NewChatPanel.Visibility = Visibility.Collapsed;
                            NavigationService.Navigate(new Uri("/ChatList.xaml?uid=" + uID, UriKind.Relative));
                        };
                        await chatTable.DeleteAsync(chatItems[0]);
                        //hasNewChat = true;
                    }
                    else
                        NewChatPanel.Visibility = Visibility.Collapsed;
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.Message, "Error loading chatItems", //MessageBoxButton.OK);
                }
            }
        }

        private void ShowPostOnMap(MobileServiceCollection<near_post, near_post> postItems)
        {
            try
            {
                if (myMap.Layers.Contains(postLocationLayer))
                    myMap.Layers.Remove(postLocationLayer);
                postLocationLayer = new MapLayer();
                foreach (near_post item in postItems)
                {
                    if (item.uID == "Admin:")
                    {
                        iconHeight = 34;
                        iconWeight = 47;
                        iconPath = "star.png";
                        iconColor = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        if (item.ImageUri == "" || item.ImageUri == null)
                        {
                            iconHeight = 38;
                            iconWeight = 40;
                            iconPath = "smiley.png";
                            iconColor = new SolidColorBrush(Colors.Magenta);
                        }
                        else
                        {
                            iconHeight = 40;
                            iconWeight = 40;
                            iconPath = "image.png";
                            iconColor = new SolidColorBrush(Colors.Green);
                        }
                    }
                    BitmapImage bitImage = new BitmapImage(new Uri("/Assets/PostIcon/" + iconPath, UriKind.Relative));
                    Rectangle myRec = new Rectangle { Height = iconHeight, Width = iconWeight, Fill = iconColor, OpacityMask = new ImageBrush { ImageSource = bitImage } };
                    myRec.Tap += (o, e) =>
                    {
                        //var stream = new MemoryStream();
                        App.postBackground = new WriteableBitmap((int)this.ActualWidth, (int)this.ActualHeight);
                        App.postBackground.Render(LayoutRoot, new MatrixTransform());
                        App.postBackground.Invalidate();
                        NavigationService.Navigate(new Uri("/ShowPost.xaml?uid=" + uID + "&sid=" + item.uID + "&pid=" + item.Id + "&lat=" + item.Latitude + "&lon=" + item.Longitude + "&con=" + item.Content + "&uri=" + item.ImageUri + "&time=" + item.__createdAt, UriKind.Relative));
                        //MessageBox.Show(item.uID.Substring(0, item.uID.IndexOf(":")) + " user:" + "\n" + item.Content);
                        //postBitImage = new BitmapImage(new Uri("/Assets/PostIcon/profanity.png", UriKind.Relative));
                        //myRec.OpacityMask = new ImageBrush { ImageSource = postBitImage };
                    };
                    AddSomthingToLayer(myRec, postLocationLayer, item.Latitude, item.Longitude);
                }
                // Add the MapLayer to the Map.
                myMap.Layers.Add(postLocationLayer);
                isPostReady = true;

                BuildLocalizedApplicationBar();

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    App.progIdc.IsVisible = false;
                    App.progIdc.IsIndeterminate = false;
                    SystemTray.IsVisible = false;
                });
                // user located!
                //BuildLocalizedApplicationBar();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "ShowPostOnMap", //MessageBoxButton.OK);
            }
        }

        private void AddSomthingToLayer(FrameworkElement element, MapLayer layer, double lat, double lon)
        {
            // Create a MapOverlay to contain the circle.
            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = element;
            myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = new GeoCoordinate(lat, lon);

            // Create a MapLayer to contain the MapOverlay.
            //MapLayer myLocationLayer = new MapLayer();
            layer.Add(myLocationOverlay);
        }

        private async void ShowMyLocationOnTheMap()
        {
            try
            {
                myMap.Layers.Clear();
                isLocReady = false;
                isIdc = true;
                SystemTray.IsVisible = true;
                if ((bool)settings["locationService"])
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        App.progIdc.IsVisible = true;
                        App.progIdc.IsIndeterminate = true;
                        App.progIdc.Text = "Locating... Please click \"findme\" if no response.";
                    });

                    //if (myMap.Layers.Contains(myLocationLayer))
                    //myMap.Layers.Remove(myLocationLayer);
                    myLocationLayer = new MapLayer();
                    // Get my current location.
                    Geolocator myGeolocator = new Geolocator();
                    Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
                    Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
                    myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);
                    // Make my current location the center of the Map.
                    this.myMap.Center = myGeoCoordinate;
                    this.myMap.ZoomLevel = 13;
                    transZoomLevel();
                    // Create a small circle to mark the current location.
                    BitmapImage postBitImage = new BitmapImage(new Uri("/Assets/PostIcon/people.png", UriKind.Relative));
                    Rectangle myRec = new Rectangle { Height = 44, Width = 36, Fill = new SolidColorBrush(App.currentAccentColorHex), OpacityMask = new ImageBrush { ImageSource = postBitImage } };
                    myRec.Tap += (o, e) =>
                    {
                        Maps_ReverseGeoCoding();
                        if (message != "")
                            MessageBox.Show(message);
                    };

                    AddSomthingToLayer(myRec, myLocationLayer, myGeoCoordinate.Latitude, myGeoCoordinate.Longitude);
                    //App.progIdc.IsVisible = false;
                    //App.progIdc.IsIndeterminate = false;
                }
                else
                {
                    transZoomLevel();
                    isAppBar = false;
                }

                isLocReady = true;
                // must put here!
                if (uID != "")
                {
                    // move the chat information to local
                    RefreshChatItems();
                }
                isChatReady = true;
                RefreshPostItems();

                if ((bool)settings["locationService"])
                {
                    // Add the MapLayer to the Map.
                    myMap.Layers.Add(myLocationLayer);
                    /*if(hasNewChat)
                    {
                        if (MessageBox.Show("You have new messages, check it now?", "Note:", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                            NavigationService.Navigate(new Uri("/ChatList.xaml?uid=" + uID, UriKind.Relative));   
                    }*/

                }
                isIdc = false;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "ShowMyLocationOnTheMap", //MessageBoxButton.OK);
                //MessageBox.Show(e.ToString(), "ShowMyLocationOnTheMap", //MessageBoxButton.OK);
            }
        }

        private void Maps_ReverseGeoCoding()
        {
            ReverseGeocodeQuery query = new ReverseGeocodeQuery()
            {
                GeoCoordinate = myGeoCoordinate
            };
            query.QueryCompleted += query_QueryCompleted;
            query.QueryAsync();
        }

        void query_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            message = "You are here:\n" + Address.ShowLocation(e).ToString();
        }

        private void BuildLocalizedApplicationBar()// show AppBar
        {
            if (isAppBar)
                return;
            ApplicationBar = new ApplicationBar { Opacity = 0.65 };
            //EventHandler[] AppBarButtonClick = { post_Click, camera_Click, record_Click, findme_Click };
            EventHandler[] AppBarButtonClick = { post_Click, findme_Click, addeals_Click };
            EventHandler[] AppBarItemClick = { Login_Click, Logout_Click, ChatList_Click, Settings_Click };
            if ((bool)settings["locationService"])
            {
                for (int i = 0; i < AppBarIcon.Length; i++)
                {
                    if (!isLocReady && AppBarIcon[i] != "findme")
                        continue;
                    else if (uID == "" && AppBarIcon[i] == "post")
                        continue;
                    // Create a new button and set the text value to the localized string from AppResources.
                    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/" + AppBarIcon[i] + ".png", UriKind.Relative));
                    appBarButton.Text = AppBarIcon[i];
                    appBarButton.Click += AppBarButtonClick[i];
                    ApplicationBar.Buttons.Add(appBarButton);
                }
                if (!isLocReady)
                    return;
                isAppBar = true;
            }
            for (int i = 0; i < AppBarItem.Length; i++)
            {
                if (AppBarItem[i] == "Login" && (bool)settings["isLogin"])
                    continue;
                if (AppBarItem[i] == "Logout" && !(bool)settings["isLogin"])
                    continue;
                if (AppBarItem[i] == "ChatList" && !(bool)settings["isLogin"])
                    continue;
                ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppBarItem[i]);
                appBarMenuItem.Click += AppBarItemClick[i];
                ApplicationBar.MenuItems.Add(appBarMenuItem);
            }
        }

        private void post_Click(object sender, EventArgs e)
        {
            App.postBackground = new WriteableBitmap((int)this.ActualWidth, (int)this.ActualHeight);
            App.postBackground.Render(LayoutRoot, new MatrixTransform());
            App.postBackground.Invalidate();

            if (myGeoCoordinate == null)
                NavigationService.Navigate(new Uri("/Post.xaml?lat=null&lon=null", UriKind.Relative));
            else
                NavigationService.Navigate(new Uri("/Post.xaml?uid=" + uID + "&lat=" + myGeoCoordinate.Latitude + "&lon=" + myGeoCoordinate.Longitude, UriKind.Relative));
        }

        private void findme_Click(object sender, EventArgs e)
        {
            DateTime temp = DateTime.Now;
            TimeSpan difference = temp - now;
            if (difference.Seconds > 1)
            {
                ShowMyLocationOnTheMap();
                now = DateTime.Now;
            }
        }

        private void addeals_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AdDealsSDKWP7;component/Views/MoreAdDeals.xaml", UriKind.Relative));
        }

        private void Login_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
            //NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
        }

        private async void Logout_Click(object sender, EventArgs e)
        {
            try
            {
                if (uID != "")
                {
                    App.MobileService.Logout();
                    WebBrowser dummyBrowser = new WebBrowser();
                    await WebBrowserExtensions.ClearCookiesAsync(dummyBrowser);
                    uID = "";
                }
                settings["isLogin"] = false;
                settings.Save();
                Login.isLoaded = false;
                NavigationService.GoBack();
                //NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
            }
            catch (Exception)
            {
                MessageBox.Show("Sorry, Logout failed. Please try again.");
            }
        }

        private void ChatList_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ChatList.xaml?uid=" + uID, UriKind.Relative));
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml?uid=" + uID, UriKind.Relative));
        }

        private void ClearBackEntries()
        {
            while (NavigationService.BackStack != null & NavigationService.BackStack.Count() > 0)
                NavigationService.RemoveBackEntry();
        }

        private void MapView_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // TO DO
            if (MessageBox.Show("Are you sure to quit Near?", "Exit?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                this.ClearBackEntries();
            else
                e.Cancel = true;
        }

        private void myMap_ZoomLevelChanged(object sender, MapZoomLevelChangedEventArgs e)
        {
            if (isLocReady && isPostReady && isChatReady)
            {
                transZoomLevel();
                RefreshPostItems();
            }
        }

        private void myMap_CenterChanged(object sender, MapCenterChangedEventArgs e)
        {
            if (isLocReady && isPostReady && isChatReady)
            {
                transZoomLevel();
                RefreshPostItems();
            }
        }

        private void transZoomLevel()
        {
            //postLatLevel = (180 / (myMap.ZoomLevel * myMap.ZoomLevel / 2) * 180 / (myMap.ZoomLevel * myMap.ZoomLevel / 2));
            //postLonLevel = (360 / (myMap.ZoomLevel * myMap.ZoomLevel / 2) * 360 / (myMap.ZoomLevel * myMap.ZoomLevel / 2));
            postLatLevel = (180 / (Math.Pow(2, myMap.ZoomLevel - 1)) * 180 / (Math.Pow(2, myMap.ZoomLevel - 1)));
            postLonLevel = (360 / (Math.Pow(2, myMap.ZoomLevel - 1)) * 360 / (Math.Pow(2, myMap.ZoomLevel - 1)));
        }

        private void myMap_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "bcb0ee0e-2b60-47ff-8ed9-3ae79b0a3d1e";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "bjZ3YwNWYy70oYU40ydhWA";
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        private void myMap_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.postBackground = new WriteableBitmap((int)this.ActualWidth, (int)this.ActualHeight);
            App.postBackground.Render(LayoutRoot, new MatrixTransform());
            App.postBackground.Invalidate();
            GeoCoordinate position = myMap.ConvertViewportPointToGeoCoordinate(e.GetPosition(myMap));
            NavigationService.Navigate(new Uri("/NewsList.xaml?lat=" + position.Latitude + "&lon=" + position.Longitude, UriKind.Relative));
        }
    }
}