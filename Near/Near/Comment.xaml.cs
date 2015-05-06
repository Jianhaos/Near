using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;

namespace Near
{
    public partial class Comment : PhoneApplicationPage
    {
        private string uID = "", subjectID = "", senderID = "", lat = "", lon = "";

        private IMobileServiceTable<near_comment> postCommentTable = App.MobileService.GetTable<near_comment>();

        public Comment()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            lat = NavigationContext.QueryString["lat"];
            lon = NavigationContext.QueryString["lon"];
            uID = NavigationContext.QueryString["uid"];
            subjectID = NavigationContext.QueryString["id"];
            senderID = NavigationContext.QueryString["sid"];
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (CommentContent.Text.Length < 10)
                MessageBox.Show("Please say something more...");
            else
            {
                if (CommentContent.Text.Length > 200)
                    MessageBox.Show("Message is too long. Please make it under 200 characters.");
                else
                {
                    CommentContent.IsEnabled = Send.IsEnabled = Clear.IsEnabled = false;
                    near_comment item = new near_comment();
                    item.Sender = uID;
                    item.uName = "";
                    item.SubjectID = subjectID;
                    item.Content = CommentContent.Text;
                    InsertComment(item);
                }
            }
        }

        private async void InsertComment(near_comment postCommentItme)
        {
            try
            {
                await postCommentTable.InsertAsync(postCommentItme);
                MessageBox.Show("Comment send success");
                NavigationService.GoBack();
                //NavigationService.Navigate(new Uri("/ShowPost.xaml?uid=" + uID + "&sid=" + senderID + "&pid=" + pID + "&lat=" + lat + "&lon=" + lon + "&con=" + content, UriKind.Relative));
            }
            catch (MobileServiceInvalidOperationException e)
            {
                MessageBox.Show(e.Message,
                    string.Format("{0} (HTTP {1})",
                    e.Response.ReasonPhrase,
                    (int)e.Response.StatusCode),
                    MessageBoxButton.OK);
            }
            CommentContent.IsEnabled = Send.IsEnabled = Clear.IsEnabled = true;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            CommentContent.Text = "";
        }
    }
}