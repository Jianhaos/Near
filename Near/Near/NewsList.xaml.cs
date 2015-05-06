using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.IO;
using System.Xml;
using Microsoft.Phone.Tasks;
using System.Xml.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Xml.XPath;
using System.Text;
using Microsoft.Phone.Maps.Services;
using System.Device.Location;
using System.Windows.Navigation;
using System.Windows.Media;
using Microsoft.Phone.Shell;

namespace Near
{
    public partial class NewsList : PhoneApplicationPage
    {
        GeoCoordinate myGeoCoordinate = new GeoCoordinate();

        int counter = 0;

        public NewsList()
        {
            InitializeComponent();
            LayoutRoot.Background = new ImageBrush { ImageSource = App.postBackground, Opacity = 0.2 };
            SystemTray.SetProgressIndicator(this, App.progIdc);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                base.OnNavigatedTo(e);
                if (e.NavigationMode == NavigationMode.Back)
                    return;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    App.progIdc.IsVisible = true;
                    App.progIdc.IsIndeterminate = true;
                    App.progIdc.Text = "Loading content... Please wait...";
                });
                string lat = NavigationContext.QueryString["lat"];
                string lon = NavigationContext.QueryString["lon"];
                myGeoCoordinate.Latitude = double.Parse(lat);
                myGeoCoordinate.Longitude = double.Parse(lon);
                Maps_ReverseGeoCoding();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            try
            {
                HtmlWeb web = new HtmlWeb();
                web.LoadCompleted += new EventHandler<HtmlDocumentLoadCompleted>(htmlDocComplete);
                web.LoadAsync("http://www.bing.com/news?q=" + Address.ShowLocation(e, "city").ToString().Replace(' ', '+') + "&&p1=%5bNewsVertical+SortByDate%3d\"1\"%5d&FORM=YGNR", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadnewsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                web.LoadCompleted += new EventHandler<HtmlDocumentLoadCompleted>(htmlDocComplete);
                web.LoadAsync("http://www.bing.com/news?q=local&FORM=NSBABR", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void htmlDocComplete(object sender, HtmlDocumentLoadCompleted e)
        {
            try
            {
                if (e.Error != null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(e.Error.Message);
                    });
                }
                else
                {
                    HtmlDocument htmlDoc = e.Document;
                    if (htmlDoc != null)
                    {
                        UpdatenewsList(e.Document);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdatenewsList(HtmlDocument newsDoc)
        {
            try
            {
                var list = new List<Newsnews>();

                foreach (HtmlNode divNode in newsDoc.DocumentNode.SelectNodes("//div[@class]"))
                {
                    HtmlAttribute classAtt = divNode.Attributes["class"];
                    if (classAtt.Value == "sn_r")
                    {
                        Newsnews news = new Newsnews();
                        foreach (HtmlNode newsNode in divNode.SelectNodes("div[@class]"))
                        {
                            HtmlAttribute newsAtt = newsNode.Attributes["class"];
                            if (newsAtt.Value == "newstitle")
                            {
                                news.Title = newsNode.FirstChild.InnerText;
                                news.NavURL = (newsNode.FirstChild.Attributes["href"]).Value;
                            }
                            if (newsAtt.Value == "sn_txt")
                            {
                                news.Summary = newsNode.FirstChild.FirstChild.InnerText;
                                foreach (HtmlNode childNode in newsNode.FirstChild.SelectNodes("span[@class]"))
                                {
                                    HtmlAttribute childAtt = childNode.Attributes["class"];
                                    if (childAtt.Value == "sn_ST")
                                    {
                                        foreach (HtmlNode dateNode in childNode.SelectNodes("span[@class]"))
                                        {
                                            HtmlAttribute dateAtt = dateNode.Attributes["class"];
                                            if (dateAtt.Value == "sn_tm tm_fre" || dateAtt.Value == "sn_tm")
                                            {
                                                news.PublishDate = dateNode.InnerText;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        list.Add(news);
                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    App.progIdc.IsVisible = false;
                    App.progIdc.IsIndeterminate = false;
                    SystemTray.IsVisible = false;
                    newsListBox.ItemsSource = list;
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        // The SelectionChanged handler for the news items 
        private void newsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListBox listBox = sender as ListBox;

                if (listBox != null && listBox.SelectedItem != null)
                {
                    // Get the SyndicationItem that was tapped.
                    Newsnews newsItem = (Newsnews)listBox.SelectedItem;

                    // Set up the page navigation only if a link actually exists in the news item.

                    // Get the associated URI of the news item.
                    Uri uri = new Uri(newsItem.NavURL);

                    // Create a new WebBrowserTask Launcher to navigate to the news item. 
                    // An alternative solution would be to use a WebBrowser control, but WebBrowserTask is simpler to use. 
                    WebBrowserTask webBrowserTask = new WebBrowserTask();
                    webBrowserTask.Uri = uri;
                    webBrowserTask.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    public class Newsnews
    {
        public string Title { get; set; }
        public string NavURL { get; set; }
        public string Summary { get; set; }
        public string ImageURL { get; set; }
        public string PublishDate { get; set; }
    }
}