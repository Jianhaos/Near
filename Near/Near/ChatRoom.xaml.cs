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
using System.Text;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Coding4Fun.Toolkit.Controls;

namespace Near
{
    public partial class ChatRoom : PhoneApplicationPage
    {
        private string uID = "", receiverID = "";

        private IMobileServiceTable<near_chat> chatTable = App.MobileService.GetTable<near_chat>();

        private MobileServiceCollection<near_chat, near_chat> chatItems;

        ContextMenu contextMenu = new ContextMenu();
        MenuItem menuItem1 = new MenuItem() { Header = "Copy", Tag = "Copy" };

        //StringBuilder messages = null;

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

        public ChatRoom()
        {
            InitializeComponent();
            App.Messages.CollectionChanged += Messages_CollectionChanged;
            //messages = new StringBuilder();

            // Data context and observable collection are children of the main page.
            this.DataContext = this;
            contextMenu.Items.Add(menuItem1);

            this.Loaded += ChatRoom_Loaded;
        }

        void ChatRoom_Loaded(object sender, RoutedEventArgs e)
        {
            // Define the query to gather all of the to-do items.
            var chatItemsInDB = from ChatItem chat in chatDB.ChatItems
                                where chat.ItemThere == receiverID || chat.ItemThere == uID
                                orderby chat.ItemTime
                                select chat;

            // Execute the query and place the results into a collection.
            ChatItems = new ObservableCollection<ChatItem>(chatItemsInDB);

            foreach (ChatItem chatItem in ChatItems)
            {
                if (chatItem.ItemSender)
                    ShowChat(chatItem.ItemContent, false);
                else
                    ShowChat(chatItem.ItemContent, true);
                chatItem.IsRead = true;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                uID = NavigationContext.QueryString["uid"];
                receiverID = NavigationContext.QueryString["rid"];
                Receiver.Text = receiverID;
                // Connect to the database and instantiate data context.
                chatDB = new ChatDataContext(ChatDataContext.DBConnectionString + "Chat" + uID.Replace(":", "") + ".sdf");
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

        void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (var item in e.NewItems)
                    {
                        var chatMessage = item as near_chat;
                        App.Messages.Remove(chatMessage);
                        if (chatMessage.Sender.TrimEnd(':') != uID)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() => ShowChat(chatMessage.Content, true));
                            ChatItem newChat = new ChatItem { ItemContent = chatMessage.Content, ItemThere = chatMessage.Sender, ItemTime = DateTime.Now, ItemSender = false, IsRead = true };
                            // Add a to-do item to the observable collection.
                            //ChatItems.Add(newChat);

                            chatDB.ChatItems.InsertOnSubmit(newChat);
                            // Save changes to the database.
                            chatDB.SubmitChanges();
                            chatTable.DeleteAsync(chatMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        private async void InsertChatMessage(near_chat message)
        {
            try
            {
                ShowChat(message.Content, false);
                await chatTable.InsertAsync(message);
                MessageTextBox.IsEnabled = Send.IsEnabled = Clear.IsEnabled = true;
            }
            catch (Exception)
            {
                //MessageBox.Show(e.ToString());
            }
        }

        private void ShowChat(string content, bool isLeft)
        {
            try
            {
                Coding4Fun.Toolkit.Controls.ChatBubble tempBubble = new ChatBubble { Background = new SolidColorBrush(App.currentAccentColorHex), Margin = new Thickness(10), MaxWidth = ContentPanel.ActualWidth * 3 / 4, HorizontalAlignment = HorizontalAlignment.Right, ChatBubbleDirection = ChatBubbleDirection.LowerRight };
                tempBubble.Hold += (o, e) =>
                {
                    ContextMenuService.SetContextMenu(tempBubble, contextMenu);
                    menuItem1.Click += (ob, ev) =>
                    {
                        Clipboard.SetText(content);
                    };
                };
                tempBubble.ManipulationCompleted += (o, e) =>
                {
                    if (isLeft)
                        tempBubble.Opacity = 0.7;
                };
                TextBlock tempBlock = new TextBlock { Text = content, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(10), FontSize = 24 };
                //Border tempBorder = new Border { Background = new SolidColorBrush(App.currentAccentColorHex), HorizontalAlignment = HorizontalAlignment.Right, MaxWidth = ContentPanel.ActualWidth * 3 / 4, Margin = new Thickness(10) };
                if (isLeft)
                {
                    tempBubble.ChatBubbleDirection = ChatBubbleDirection.UpperLeft;
                    tempBubble.HorizontalAlignment = HorizontalAlignment.Left;
                    tempBubble.Opacity = 0.7;
                }
                //tempBorder.HorizontalAlignment = HorizontalAlignment.Left;
                tempBubble.Content = tempBlock;
                MessageStack.Children.Add(tempBubble);
            }
            catch (Exception)
            {
                //MessageBox.Show(e.ToString());
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageTextBox.Text.Length < 1)
                    MessageBox.Show("Please say something...");
                else
                {
                    if (MessageTextBox.Text.Length > 200)
                        MessageBox.Show("Message is too long. Please make it under 200 characters.");
                    else
                    {
                        MessageTextBox.IsEnabled = Send.IsEnabled = Clear.IsEnabled = false;
                        // Create a new to-do item based on the text box.
                        ChatItem newChat = new ChatItem { ItemContent = MessageTextBox.Text.Trim(), ItemThere = receiverID, ItemTime = DateTime.Now, ItemSender = true, IsRead = true };

                        // Add a to-do item to the observable collection.
                        ChatItems.Add(newChat);

                        // Add a to-do item to the local database.
                        chatDB.ChatItems.InsertOnSubmit(newChat);

                        chatDB.SubmitChanges();
                        var chatMessage = new near_chat()
                        {
                            Sender = uID,
                            uName = "",
                            Receiver = receiverID,
                            Content = MessageTextBox.Text.Trim(),
                            Channel = App.CurrentChannel.ChannelUri.ToString()
                        };
                        InsertChatMessage(chatMessage);
                        MessageTextBox.Text = "";
                    }

                }
            }
            catch (Exception)
            { }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to clear this session? It will be deleted permanently!", "Warning！", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                // Remove the to-do item from the observable collection.
                ChatItems.Clear();

                var chatForDelete = from ChatItem chat in chatDB.ChatItems
                                    where chat.ItemThere == receiverID
                                    select chat;

                // Execute the query and place the results into a collection.
                // ChatItems = new ObservableCollection<ChatItem>(chatForDelete);

                // Remove the to-do item from the local database.
                chatDB.ChatItems.DeleteAllOnSubmit(chatForDelete);

                // Save changes to the database.
                chatDB.SubmitChanges();

                MessageStack.Children.Clear();

                // Put the focus back to the main page.
                this.Focus();
            }
        }

        private void MessageStack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double d = ((StackPanel)sender).ActualHeight;
            MessageScroll.ScrollToVerticalOffset(d);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                // Call the base method.
                base.OnNavigatedFrom(e);
                if (e.NavigationMode == NavigationMode.Back)
                {
                    RefreshChatItems();
                    // Save changes to the database.
                    chatDB.SubmitChanges();
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        private async void RefreshChatItems()
        {
            try
            {
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
                    await chatTable.DeleteAsync(chatItems[0]);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error loading chatItems", MessageBoxButton.OK);
            }
        }
    }
}