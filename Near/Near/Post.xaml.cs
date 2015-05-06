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
using System.Text;
using Microsoft.WindowsAzure.MobileServices;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Windows.Media;

namespace Near
{
    public partial class Post : PhoneApplicationPage
    {
        string[] AppBarIcon = { "send", "camera", "record" };

        GeoCoordinate myGeoCoordinate = new GeoCoordinate();
        private string uID = "";
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        private IMobileServiceTable<near_post> postTable = App.MobileService.GetTable<near_post>();
        private IMobileServiceTable<near_image> imageTable = App.MobileService.GetTable<near_image>();

        CameraCaptureTask cameraCaptureTask;

        // Using a stream reference to upload the image to blob storage.
        Stream imageStream = null;

        BitmapImage bmi = new BitmapImage();

        WriteableBitmap wb;

        public Post()
        {
            InitializeComponent();
            if (!settings.Contains("firstPost"))// is user login or not?
            {
                settings.Add("firstPost", true);
                settings.Save();
            }
            if (!(bool)settings["firstPost"])
            {
                PostContent.Text = "";
            }
            LayoutRoot.Background = new ImageBrush { ImageSource = App.postBackground, Opacity = 0.2 };
            BuildLocalizedApplicationBar();
            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += cameraCaptureTask_Completed;
            SystemTray.SetProgressIndicator(this, App.progIdc);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string lat = NavigationContext.QueryString["lat"];
            string lon = NavigationContext.QueryString["lon"];
            uID = NavigationContext.QueryString["uid"];
            if (lat == "null" && lon == "null")
                return;
            myGeoCoordinate.Latitude = double.Parse(lat);
            myGeoCoordinate.Longitude = double.Parse(lon);
            Maps_ReverseGeoCoding();
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
            Location.Text = "You are here:\n" + Address.ShowLocation(e).ToString();
        }

        private void BuildLocalizedApplicationBar()// show AppBar
        {
            ApplicationBar = new ApplicationBar { Opacity = 0.6 };
            EventHandler[] AppBarButtonClick = { Send_Click, camera_Click, record_Click };
            for (int i = 0; i < AppBarIcon.Length; i++)
            {
                // Create a new button and set the text value to the localized string from AppResources.
                ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/" + AppBarIcon[i] + ".png", UriKind.Relative));
                appBarButton.Text = AppBarIcon[i];
                appBarButton.Click += AppBarButtonClick[i];
                ApplicationBar.Buttons.Add(appBarButton);
            }
        }

        private void Send_Click(object sender, EventArgs e)
        {
            try
            {
                if (bmi.PixelHeight == 0 || bmi.PixelWidth == 0)
                {
                    if (PostContent.Text.Length < 10)
                        MessageBox.Show("Please say something more...");
                    else
                    {
                        if (PostContent.Text.Length > 300)
                            MessageBox.Show("Message is too long. Please make it under 300 characters.");
                        else
                        {
                            ApplicationBar.IsVisible = PostContent.IsEnabled = false;
                            Remove.Visibility = Visibility.Collapsed;
                            SystemTray.IsVisible = true;
                            App.progIdc.IsVisible = true;
                            App.progIdc.IsIndeterminate = true;
                            App.progIdc.Text = "Uploading...";

                            near_post item = new near_post { uID = uID, uName = "", Latitude = myGeoCoordinate.Latitude, Longitude = myGeoCoordinate.Longitude, Content = PostContent.Text, ImageUri = "" };
                            //item.uID = "Admin:";
                            //randomLocation();
                            //item.PostDate = DateTime.Now;
                            InsertPost(item);
                        }
                    }
                }
                else
                {
                    if (PostContent.Text.Length < 10 && PostContent.Text.Length > 0)
                        MessageBox.Show("Please say something more...");
                    else
                    {
                        if (PostContent.Text.Length > 300)
                            MessageBox.Show("Message is too long. Please make it under 300 characters.");
                        else
                        {
                            ApplicationBar.IsVisible = PostContent.IsEnabled = false;
                            Remove.Visibility = Visibility.Collapsed;
                            SystemTray.IsVisible = true;
                            App.progIdc.IsVisible = true;
                            App.progIdc.IsIndeterminate = true;
                            App.progIdc.Text = "Uploading...";

                            wb = new WriteableBitmap(bmi);
                            ResizeImage(800);

                            imageStream = new MemoryStream();
                            wb.SaveJpeg(imageStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            imageStream.Seek(0, SeekOrigin.Begin);
                            //imageStream = e.ChosenPhoto;
                            var imageItem = new near_image { uID = uID };
                            InsertImageItem(imageItem);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void ResizeImage(int limitation)
        {
            double rate = 0, tempPixel = 0;

            if (wb.PixelHeight > wb.PixelWidth)
            {
                if (wb.PixelHeight > limitation)
                {
                    rate = limitation * 1.0 / wb.PixelHeight;
                    tempPixel = wb.PixelWidth * rate;
                    wb = wb.Resize(Convert.ToInt32(tempPixel), limitation, WriteableBitmapExtensions.Interpolation.Bilinear);
                }

            }
            else
            {
                if (wb.PixelWidth > limitation)
                {
                    rate = limitation * 1.0 / wb.PixelWidth;
                    tempPixel = wb.PixelHeight * rate;
                    wb = wb.Resize(limitation, Convert.ToInt32(tempPixel), WriteableBitmapExtensions.Interpolation.Bilinear);
                }
            }
        }

        void randomLocation()//随机生成洛杉矶经纬度
        {
            Random Seed = new Random();
            myGeoCoordinate.Longitude = RanFloat(-1800000, 1800000, Seed);
            myGeoCoordinate.Latitude = RanFloat(-900000, 900000, Seed);
        }

        public double RanFloat(int Min, int Max, Random Seed)
        {
            double Result = 0;
            int Temp = 0;
            Temp = Seed.Next(Min, Max);
            Result = Temp * 0.0001;
            return Result;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            PostContent.Text = "";
        }

        private async void InsertPost(near_post postItme)
        {
            try
            {
                await postTable.InsertAsync(postItme);
                SystemTray.IsVisible = false;
                App.progIdc.IsVisible = false;
                App.progIdc.IsIndeterminate = false;
                MessageBox.Show("Post send success!");
                if ((bool)settings["firstPost"])
                {
                    settings["firstPost"] = false;
                    settings.Save();
                }
                NavigationService.GoBack();
                //NavigationService.Navigate(new Uri("/MainPage.xaml?uid=" + uID, UriKind.Relative));
            }
            catch (MobileServiceInvalidOperationException e)
            {
                MessageBox.Show(e.Message,
                    string.Format("{0} (HTTP {1})",
                    e.Response.ReasonPhrase,
                    (int)e.Response.StatusCode),
                    MessageBoxButton.OK);
            }
            ApplicationBar.IsVisible = PostContent.IsEnabled = true;
        }

        private void camera_Click(object sender, EventArgs e)
        {
            //Do work for your application here.
            //NavigationService.Navigate(new Uri("/Camera.xaml", UriKind.Relative));
            cameraCaptureTask.Show();
        }

        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            bmi.SetSource(e.ChosenPhoto);
            wb = new WriteableBitmap(bmi);
            ResizeImage(400);
            PostImage.Source = wb;
            ImageGrid.Width = PostImage.Width = wb.PixelWidth;
            ImageGrid.Height = PostImage.Height = wb.PixelHeight;
            Remove.Visibility = Visibility.Visible;
        }

        private async void InsertImageItem(near_image imageItem)
        {
            string errorString = string.Empty;

            if (imageStream != null)
            {
                try
                {
                    // Set blob properties of imageItem.
                    imageItem.ContainerName = "imageitems";
                    imageItem.ResourceName = Guid.NewGuid().ToString() + ".jpg";


                    // Send the item to be inserted. When blob properties are set this
                    // generates an SAS in the response.
                    await imageTable.InsertAsync(imageItem);


                    // If we have a returned SAS, then upload the blob.
                    if (!string.IsNullOrEmpty(imageItem.SasQueryString))
                    {
                        // Get the URI generated that contains the SAS 
                        // and extract the storage credentials.
                        StorageCredentials cred = new StorageCredentials(imageItem.SasQueryString);
                        var imageUri = new Uri(imageItem.ImageUri);

                        // Instantiate a Blob store container based on the info in the returned item.
                        CloudBlobContainer container = new CloudBlobContainer(
                            new Uri(string.Format("https://{0}/{1}",
                                imageUri.Host, imageItem.ContainerName)), cred);

                        // Upload the new image as a BLOB from the stream.
                        CloudBlockBlob blobFromSASCredential =
                            container.GetBlockBlobReference(imageItem.ResourceName);
                        await blobFromSASCredential.UploadFromStreamAsync(imageStream);

                        // When you request an SAS at the container-level instead of the blob-level,
                        // you are able to upload multiple streams using the same container credentials.

                        imageStream = null;

                        near_post item = new near_post { uID = uID, uName = "", Latitude = myGeoCoordinate.Latitude, Longitude = myGeoCoordinate.Longitude, Content = PostContent.Text, ImageUri = imageItem.ImageUri };
                        //item.uID = "Admin:";
                        //randomLocation();
                        //item.PostDate = DateTime.Now;
                        InsertPost(item);
                    }
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.Message, "InsertImageItem", //MessageBoxButton.OK);
                }
            }
        }

        private void record_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coming soon!");
            //Do work for your application here.
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            bmi.UriSource = null;
            PostImage.Source = null;
            Remove.Visibility = Visibility.Collapsed;
        }
    }
}