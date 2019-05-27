using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Client.ServiceReference;

namespace Client
{
    public class GroupItem : DependencyObject
    {
        public static readonly DependencyProperty IsSelectedGroupProperty =
            DependencyProperty.Register("IsSelectedGroup", typeof(bool), typeof(GroupItem),
                new PropertyMetadata(false));

        public static UserWCF LoginedUser;

        public static readonly DependencyProperty UserImageProperty =
            DependencyProperty.Register("GroupImage", typeof(BitmapSource), typeof(GroupItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty LastMessageTimeProperty =
            DependencyProperty.Register("LastMessageTime", typeof(DateTime), typeof(GroupItem),
                new PropertyMetadata(new DateTime()));

        public static readonly DependencyProperty NotReadMessagesCountProperty =
            DependencyProperty.Register("NotReadMessagesCount", typeof(int), typeof(GroupItem),
                new PropertyMetadata(0));

        public GroupItem()
        {

        }
        public GroupItem(GroupWCF group, bool isSelectedGroup = false)
        {
            Group = group;
            IsSelectedGroup = isSelectedGroup;
            if (group.LastMessage != null)
            {
                LastMessageTime = group.LastMessage.DateTime;
            }

            if (group.Type.Equals(GroupType.SingleUser))
            {
                if (group.Users.FirstOrDefault(u => u.Login != LoginedUser.Login) is UserBaseWCF anotherUser)
                {
                    Group.Name = anotherUser.DisplayName ?? anotherUser.Login;
                }
            }
            else if (group.Type.Equals(GroupType.MultyUser))
            {
                Group.Name = group.Name;
            }
        }

        public bool IsSelectedGroup
        {
            get => (bool) GetValue(IsSelectedGroupProperty);
            set => SetValue(IsSelectedGroupProperty, value);
        }

        public GroupWCF Group { get; set; }

        public BitmapSource GroupImage
        {
            get => (BitmapSource) GetValue(UserImageProperty);
            set => SetValue(UserImageProperty, value);
        }

        public DateTime LastMessageTime
        {
            get => (DateTime) GetValue(LastMessageTimeProperty);
            set => SetValue(LastMessageTimeProperty, value);
        }

        public int NotReadMessagesCount
        {
            get => (int) GetValue(NotReadMessagesCountProperty);
            set => SetValue(NotReadMessagesCountProperty, value);
        }

        public void SetAvatar(BitmapSource groupImage)
        {
            GroupImage = groupImage;
        }
    }
}