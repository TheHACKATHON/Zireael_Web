using System.Windows;
using System.Windows.Media.Imaging;
using Client.ServiceReference;

namespace Client
{
    public class ContactItem : DependencyObject
    {
        public static readonly DependencyProperty UserImageProperty =
            DependencyProperty.Register("UserImage", typeof(BitmapSource), typeof(ContactItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelectedContact", typeof(bool), typeof(ContactItem),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsDisabledContactProperty =
            DependencyProperty.Register("IsDisabledContact", typeof(bool), typeof(ContactItem),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ContactProperty =
            DependencyProperty.Register("Contact", typeof(UserBaseWCF), typeof(ContactItem),
                new PropertyMetadata(null));

        public bool IsDisabledContact
        {
            get => (bool) GetValue(IsDisabledContactProperty);
            set => SetValue(IsDisabledContactProperty, value);
        }

        public UserBaseWCF Contact
        {
            get => (UserBaseWCF) GetValue(ContactProperty);
            set => SetValue(ContactProperty, value);
        }

        public BitmapSource UserImage
        {
            get => (BitmapSource) GetValue(UserImageProperty);
            set => SetValue(UserImageProperty, value);
        }

        public bool IsSelectedContact
        {
            get => (bool) GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
    }
}