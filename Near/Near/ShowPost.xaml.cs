using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Maps.Services;
using System.Device.Location;
using System.Windows.Media;
using Microsoft.WindowsAzure.MobileServices;
using System.Windows.Media.Imaging;

namespace Near
{
    public partial class ShowPost : PhoneApplicationPage
    {
        string[] AppBarIcon = { "chat", "comment", "delete" };
        private string uID = "", pID = "", senderID = "", content = "", lat = "", lon = "", uri = "", time = "";
        private bool isCommentReady = true;
        GeoCoordinate myGeoCoordinate = new GeoCoordinate();

        private MobileServiceCollection<near_comment, near_comment> commentItems;
        private IMobileServiceTable<near_comment> commentTable = App.MobileService.GetTable<near_comment>();
        private IMobileServiceTable<near_post> postTable = App.MobileService.GetTable<near_post>();

        ContextMenu contextMenu = new ContextMenu();
        MenuItem menuItem1 = new MenuItem() { Header = "Delete", Tag = "Delete" };

        public static BitmapImage tempImage = null;

        public ShowPost()
        {
            InitializeComponent();
            LayoutRoot.Background = new ImageBrush { ImageSource = App.postBackground, Opacity = 0.2 };
            contextMenu.Items.Add(menuItem1);
            SystemTray.SetProgressIndicator(this, App.progIdc);
            //PostSender.Foreground = new SolidColorBrush(App.currentAccentColorHex);
            //Location.Foreground = new SolidColorBrush(App.currentAccentColorHex);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            lat = NavigationContext.QueryString["lat"];
            lon = NavigationContext.QueryString["lon"];
            uID = NavigationContext.QueryString["uid"];
            pID = NavigationContext.QueryString["pid"];
            senderID = NavigationContext.QueryString["sid"];
            content = NavigationContext.QueryString["con"];
            uri = NavigationContext.QueryString["uri"];
            time = NavigationContext.QueryString["time"];
            myGeoCoordinate.Latitude = double.Parse(lat);
            myGeoCoordinate.Longitude = double.Parse(lon);
            Maps_ReverseGeoCoding();
            RefreshCommentItems();
            BuildLocalizedApplicationBar();
        }

        private async void RefreshCommentItems()
        {
            if (!isCommentReady)
                return;
            try
            {
                isCommentReady = false;
                IMobileServiceTableQuery<near_comment> query = commentTable
                    .Where(near_comment => near_comment.SubjectID == pID);

                commentItems = await query.ToCollectionAsync();
                isCommentReady = true;
            }
            catch (MobileServiceInvalidOperationException e)
            {
                MessageBox.Show(e.Message, "Error loading postItems", MessageBoxButton.OK);
            }
            ShowComment(commentItems);
        }

        private void ShowComment(MobileServiceCollection<near_comment, near_comment> commentItems)
        {
            try
            {
                CommentStack.Children.Clear();
                foreach (var comment in commentItems)
                {
                    TextBlock commentSender = new TextBlock { Text = comment.Sender, Margin = new Thickness(10), FontSize = 20 };
                    TextBlock tempBlock = new TextBlock { Text = comment.Content, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(10, 0, 10, 10), FontSize = 24 };
                    StackPanel tempStack = new StackPanel();
                    tempStack.Hold += (o, e) =>
                    {
                        if (uID == senderID)
                        {
                            ContextMenuService.SetContextMenu(tempStack, contextMenu);
                            menuItem1.Click += async (ob, ev) =>
                            {
                                await commentTable.DeleteAsync(comment);
                                RefreshCommentItems();
                            };
                        }
                    };
                    tempStack.Children.Add(commentSender);
                    tempStack.Children.Add(tempBlock);
                    Border tempBorder = new Border { BorderThickness = new Thickness(4), BorderBrush = new SolidColorBrush(Colors.White), Margin = new Thickness(10) };
                    tempBorder.Child = tempStack;
                    CommentStack.Children.Add(tempBorder);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(e.ToString());
            }
        }

        private void Maps_ReverseGeoCoding()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                App.progIdc.IsVisible = true;
                App.progIdc.IsIndeterminate = true;
                App.progIdc.Text = "Loading content... Please wait...";
            });

            ReverseGeocodeQuery query = new ReverseGeocodeQuery()
            {
                GeoCoordinate = myGeoCoordinate
            };
            query.QueryCompleted += query_QueryCompleted;
            query.QueryAsync();
        }

        void query_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            //PostSender.Text = senderID.Substring(0, senderID.IndexOf(":")) + " user:\n";
            if (content != "")
            {
                PostTime.Visibility = Visibility.Visible;
                PostContent.Visibility = Visibility.Visible;

                PostTime.Text = time;
                PostContent.Text = senderID.Substring(0, senderID.IndexOf(":")) + " user:\n" + content;
            }
            Location.Text = "\nSending at:\n" + Address.ShowLocation(e).ToString();
            if (uri != "")
            {
                tempImage = new BitmapImage { UriSource = new Uri(uri, UriKind.Absolute) };
                PostImage.Source = tempImage;
                PostImage.ImageOpened += (o, ev) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
               {
                   App.progIdc.IsVisible = false;
                   App.progIdc.IsIndeterminate = false;
                   SystemTray.IsVisible = false;
               });
                };
            }
            else
            {
                App.progIdc.IsVisible = false;
                App.progIdc.IsIndeterminate = false;
                SystemTray.IsVisible = false;
            }
        }

        private void BuildLocalizedApplicationBar()// show AppBar
        {
            //if (uID == "" || uID == senderID)
            if (uID != "")
            {
                ApplicationBar = new ApplicationBar { Opacity = 0.6 };
                EventHandler[] AppBarButtonClick = { chat_Click, comment_Click, delete_Click };
                for (int i = 0; i < AppBarIcon.Length; i++)
                {
                    if (uID == senderID && AppBarIcon[i] == "chat")
                        continue;
                    if (uID != "Twitter:1225287356" && AppBarIcon[i] == "delete" && senderID != uID)
                        continue;
                    // Create a new button and set the text value to the localized string from AppResources.
                    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/" + AppBarIcon[i] + ".png", UriKind.Relative));
                    appBarButton.Text = AppBarIcon[i];
                    appBarButton.Click += AppBarButtonClick[i];
                    ApplicationBar.Buttons.Add(appBarButton);
                }
            }
        }

        private void chat_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ChatRoom.xaml?uid=" + uID + "&rid=" + senderID, UriKind.Relative));
        }

        private void comment_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Comment.xaml?uid=" + uID + "&sid=" + senderID + "&id=" + pID + "&lat=" + lat + "&lon=" + lon, UriKind.Relative));
        }

        private void PostImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ViewImage.xaml", UriKind.Relative));
        }

        private async void delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to delete this post?", "Delete?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                try
                {
                    near_post postItem = new near_post { Id = pID };
                    await postTable.DeleteAsync(postItem);
                    MessageBox.Show("Delete success!");
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}