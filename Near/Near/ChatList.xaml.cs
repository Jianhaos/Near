using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace Near
{
    public partial class ChatList : PhoneApplicationPage
    {
        private string uID = "";

        // Data context for the local database
        private ChatDataContext chatDB;

        // Define an observable collection property that controls can bind to.
        private ObservableCollection<ChatItem> _chatItems;

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

        public ChatList()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                uID = NavigationContext.QueryString["uid"];
                chatDB = new ChatDataContext(ChatDataContext.DBConnectionString + "Chat" + uID.Replace(":", "") + ".sdf");
                ChatStack.Children.Clear();
                // Define the query to gather all of the to-do items.
                var chatItemsInDB = from chat in chatDB.ChatItems
                                    group chat by chat.ItemThere into grp
                                    let maxTime = grp.Max(chat => chat.ItemTime)
                                    from row in grp
                                    where row.ItemTime == maxTime
                                    orderby row.ItemTime descending
                                    select row;
                ChatItems = new ObservableCollection<ChatItem>(chatItemsInDB);

                if (ChatItems.Count > 0)
                {
                    foreach (var g in chatItemsInDB)
                    {
                        // show the list name and latest content
                        ShowList(g);
                    }
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString());
            }
            base.OnNavigatedTo(e);
        }

        private void ShowList(ChatItem item)
        {
            try
            {
                StackPanel tempStack = new StackPanel();
                TextBlock otherName = new TextBlock { Text = item.ItemThere, Margin = new Thickness(10), FontWeight = FontWeights.Bold, FontSize = 24 };
                TextBlock tempBlock = new TextBlock { Text = item.ItemContent, Margin = new Thickness(20, 0, 20, 10), FontSize = 20 };
                tempStack.Children.Add(otherName);
                tempStack.Children.Add(tempBlock);
                tempStack.Tap += (o, e) =>
                {
                    NavigationService.Navigate(new Uri("/ChatRoom.xaml?uid=" + uID + "&rid=" + item.ItemThere, UriKind.Relative));
                };
                Border tempBorder = new Border { Margin = new Thickness(10), BorderThickness = new Thickness(4), BorderBrush = new SolidColorBrush(Colors.White) };
                if (item.IsRead == false)
                    tempBorder.BorderBrush = new SolidColorBrush(App.currentAccentColorHex);
                tempBorder.Child = tempStack;
                ChatStack.Children.Add(tempBorder);
            }
            catch (Exception)
            {
                //MessageBox.Show(e.ToString());
            }
        }

        private void ChatStack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double d = ((StackPanel)sender).ActualHeight;
            ChatScroll.ScrollToVerticalOffset(d);
        }
    }
}