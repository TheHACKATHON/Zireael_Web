using System.Windows;
using Client.ServiceReference;

namespace Client
{
    public class GroupInfoItem : DependencyObject
    {
        public static readonly DependencyProperty HaveSelectedMessagesProperty =
            DependencyProperty.Register("HaveSelectedMessages", typeof(bool), typeof(GroupInfoItem),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsUserBlockedInfoProperty =
            DependencyProperty.Register("IsUserBlockedInfo", typeof(bool), typeof(GroupInfoItem),
                new PropertyMetadata(false));

        public static readonly DependencyProperty UserProperty =
            DependencyProperty.Register("User", typeof(UserBaseWCF), typeof(GroupItem), new PropertyMetadata(null));

        public static readonly DependencyProperty GroupProperty =
            DependencyProperty.Register("Group", typeof(GroupWCF), typeof(GroupInfoItem), new PropertyMetadata(null));

        public bool IsUserBlockedInfo
        {
            get => (bool) GetValue(IsUserBlockedInfoProperty);
            set => SetValue(IsUserBlockedInfoProperty, value);
        }

        public UserBaseWCF User
        {
            get => (UserBaseWCF) GetValue(UserProperty);
            set => SetValue(UserProperty, value);
        }
        
        public GroupWCF Group
        {
            get => (GroupWCF) GetValue(GroupProperty);
            set => SetValue(GroupProperty, value);
        }
        
        public bool HaveSelectedMessages
        {
            get => (bool) GetValue(HaveSelectedMessagesProperty);
            set => SetValue(HaveSelectedMessagesProperty, value);
        }
    }
}