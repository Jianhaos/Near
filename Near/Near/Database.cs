using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Near
{
    [Table]
    public class ChatItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        // Define ID: private field, public property and database column.
        private int _chatItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int ChatItemId
        {
            get
            {
                return _chatItemId;
            }
            set
            {
                if (_chatItemId != value)
                {
                    NotifyPropertyChanging("ChatItemId");
                    _chatItemId = value;
                    NotifyPropertyChanged("ChatItemId");
                }
            }
        }

        // Define item content: private field, public property and database column.
        private string _itemContent;

        [Column]
        public string ItemContent
        {
            get
            {
                return _itemContent;
            }
            set
            {
                if (_itemContent != value)
                {
                    NotifyPropertyChanging("ItemContent");
                    _itemContent = value;
                    NotifyPropertyChanged("ItemContent");
                }
            }
        }

        // Define item content: private field, public property and database column.
        private string _itemThere;

        [Column]
        public string ItemThere
        {
            get
            {
                return _itemThere;
            }
            set
            {
                if (_itemThere != value)
                {
                    NotifyPropertyChanging("ItemThere");
                    _itemThere = value;
                    NotifyPropertyChanged("ItemThere");
                }
            }
        }

        private DateTime _itemTime;

        [Column]
        public DateTime ItemTime
        {
            get
            {
                return _itemTime;
            }
            set
            {
                if (_itemTime != value)
                {
                    NotifyPropertyChanging("ItemTime");
                    _itemTime = value;
                    NotifyPropertyChanged("ItemTime");
                }
            }
        }

        private bool _itemSender;

        [Column]
        public bool ItemSender
        {
            get
            {
                return _itemSender;
            }
            set
            {
                if (_itemSender != value)
                {
                    NotifyPropertyChanging("ItemSender");
                    _itemSender = value;
                    NotifyPropertyChanged("ItemSender");
                }
            }
        }

        // Define read value: private field, public property and database column.
        private bool _isRead;

        [Column]
        public bool IsRead
        {
            get
            {
                return _isRead;
            }
            set
            {
                if (_isRead != value)
                {
                    NotifyPropertyChanging("IsRead");
                    _isRead = value;
                    NotifyPropertyChanged("IsRead");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    public class ChatDataContext : DataContext
    {
        // Specify the connection string as a static, used in main page and app.xaml.
        public static string DBConnectionString = "Data Source=isostore:/";

        // Pass the connection string to the base class.
        public ChatDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify a single table for the to-do items.
        public Table<ChatItem> ChatItems;
    }
}
