using System.Windows;
using System.Windows.Media.Imaging;
using Client.ServiceReference;

namespace Client
{
    public class BlackListItem : DependencyObject
    {
        public static readonly DependencyProperty BlockedUserImageProperty =
            DependencyProperty.Register("BlockedUserImage", typeof(BitmapSource), typeof(BlackListItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty UserProperty =
            DependencyProperty.Register("User", typeof(UserBaseWCF), typeof(BlackListItem), new PropertyMetadata(null));

        public BlackListItem(UserBaseWCF user)
        {
            User = user;
        }
        
        public UserBaseWCF User
        {
            get => (UserBaseWCF) GetValue(UserProperty);
            set => SetValue(UserProperty, value);
        }
        
        public BitmapSource BlockedUserImage
        {
            get => (BitmapSource) GetValue(BlockedUserImageProperty);
            set => SetValue(BlockedUserImageProperty, value);
        }
    }
}