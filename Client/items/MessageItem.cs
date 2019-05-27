using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Client.ServiceReference;

namespace Client
{
    public class MessageItem : DependencyObject
    {
        public static readonly DependencyProperty IsSelectedMessageProperty =
            DependencyProperty.Register("IsSelectedMessage", typeof(bool), typeof(MessageItem),
                new PropertyMetadata(false));

        public static readonly DependencyProperty UserImageProperty =
            DependencyProperty.Register("UserImage", typeof(BitmapSource), typeof(MessageItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsMessageComeProperty =
            DependencyProperty.Register("IsMessageCome", typeof(bool), typeof(MessageItem),
                new PropertyMetadata(false));

        public static readonly DependencyProperty FileDownloadStateProperty =
            DependencyProperty.Register("FileDownloadState", typeof(FileDownloadState), typeof(MessageItem),
                new PropertyMetadata(FileDownloadState.None));

        public static readonly DependencyProperty PackagesCountProperty =
            DependencyProperty.Register("PackagesCount", typeof(int), typeof(MessageItem), new PropertyMetadata(0));


        public static readonly DependencyProperty PackagesDownloadedProperty =
            DependencyProperty.Register("PackagesDownloaded", typeof(int), typeof(MessageItem),
                new PropertyMetadata(0));
        
        public bool IsContact
        {
            get { return (bool)GetValue(IsContactProperty); }
            set { SetValue(IsContactProperty, value); }
        }
        
        public static readonly DependencyProperty IsContactProperty =
            DependencyProperty.Register("IsContact", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        
        public bool IsUserBlocked
        {
            get { return (bool)GetValue(IsUserBlockedProperty); }
            set { SetValue(IsUserBlockedProperty, value); }
        }

        public static readonly DependencyProperty IsUserBlockedProperty =
            DependencyProperty.Register("IsUserBlockedInfo", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        
        public MessageItem(MessageWCF message, bool isRight, BitmapSource userImage, bool isSelectedMessage = false)
        {
            Message = message;
            IsRight = isRight;
            IsSelectedMessage = isSelectedMessage;
            UserImage = userImage;
        }

        public int PackagesDownloaded
        {
            get => (int) GetValue(PackagesDownloadedProperty);
            set => SetValue(PackagesDownloadedProperty, value);
        }

        public int PackagesCount
        {
            get => (int) GetValue(PackagesCountProperty);
            set => SetValue(PackagesCountProperty, value);
        }

        public FileDownloadState FileDownloadState
        {
            get => (FileDownloadState) GetValue(FileDownloadStateProperty);
            set => SetValue(FileDownloadStateProperty, value);
        }

        public bool IsMessageCome
        {
            get => (bool) GetValue(IsMessageComeProperty);
            set => SetValue(IsMessageComeProperty, value);
        }

        public bool IsSelectedMessage
        {
            get => (bool) GetValue(IsSelectedMessageProperty);
            set => SetValue(IsSelectedMessageProperty, value);
        }

        public bool IsRight { get; set; }
        public MessageWCF Message { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public BitmapSource UserImage
        {
            get => (BitmapSource) GetValue(UserImageProperty);
            set => SetValue(UserImageProperty, value);
        }
    }
}