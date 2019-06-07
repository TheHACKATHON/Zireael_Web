using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows.Data;
using Client.ServiceReference;

namespace Client
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class MainWindow : ICeadChatServiceCallback
    {
        public async void DeleteMessageCallback(MessageWCF message, string connectionId)
        {
            if (LoginedUser != null)
            {
                if (_groupItems.SingleOrDefault(gItem =>
                        gItem.Group.Id.Equals(message.GroupId)) is GroupItem groupItem &&
                    groupItem.IsSelectedGroup &&
                    _messageItems.SingleOrDefault(mItem =>
                        mItem.Message.Id.Equals(message.Id)) is MessageItem messageItem)
                {
                    _messageItems.Remove(messageItem);
                }
                else if (_groupItems.SingleOrDefault(gItem =>
                             gItem.Group.Id.Equals(message.GroupId)) is GroupItem groupItem2 &&
                         !groupItem2.IsSelectedGroup && message.Sender.Id != LoginedUser.Id && !message.IsRead)
                {
                    Dictionary<int, int> keyValues = null;
                    if (!await CheckConnection(async () =>
                        keyValues = await _client.GetDontReadMessagesFromGroupsAsync(new[] {groupItem2.Group.Id})))
                    {
                        return;
                    }

                    groupItem2.NotReadMessagesCount = keyValues[groupItem2.Group.Id];
                }

                UpdateOnlineStatus(new[] {message.Sender});
            }
        }

        public void NewLastMessageCallback(MessageWCF message, string connectionId)
        {
            if (LoginedUser != null)
            {
                if (_groupItems.SingleOrDefault(gItem => gItem.Group.Id.Equals(message.GroupId)) is GroupItem groupItem)
                {
                    groupItem.Group.LastMessage = message;
                    groupItem.LastMessageTime = message.DateTime;
                }

                UpdateOnlineStatus(new[] {message.Sender});
            }
        }

        public void RemoveGroupCallback(GroupWCF group, string connectionId)
        {
            if (LoginedUser != null)
            {
                if (_groupItems.SingleOrDefault(gItem => gItem.Group.Id.Equals(group.Id)) is GroupItem groupItem)
                {
                    if (SelectedGroupId.Equals(group.Id))
                    {
                        SelectedGroupId = -1;
                    }

                    _groupItems.Remove(groupItem);
                }

                UpdateOnlineStatus(group.Users);
            }
        }

        public void SetAvatarCallback(AvatarWCF avatar, UserBaseWCF user)
        {
            // TODO set avater callback
            if (LoginedUser != null)
            {
                UpdateOnlineStatus(new[] {user});
            }
        }

        public void SetAvatarForGroupCallback(AvatarWCF avatar, GroupWCF group)
        {
            // TODO set avater callback
            if (LoginedUser != null)
            {
                UpdateOnlineStatus(group.Users);
            }
        }

        public void GiveIdToMessageCallback(KeyValuePair<long, int>[] messageHashId, string sessionId)
        {
            if (LoginedUser != null)
            {
                foreach (var hashId in messageHashId)
                {
                    var message = _messageItems.FirstOrDefault(m => m.Message.DateTime.Ticks.Equals(hashId.Key));
                    if (message != null)
                    {
                        message.Message.Id = hashId.Value;
                    }
                }
            }
        }
    

    public async void CreateChatCallback(GroupWCF group, int creatorId, string connectionId)
            {
                if (LoginedUser != null)
                {
                    var bmp = _defaultImage;

                    if (group.Type.Equals(GroupType.SingleUser))
                    {
                        var avatar = _avatars.FirstOrDefault(ava =>
                            ava.AvatarWCF is AvatarUserWCF wcf &&
                            wcf.User.Id.Equals(group.Users.Single(u => u.Id != LoginedUser.Id).Id));

                        if (avatar != null)
                        {
                            bmp = avatar.BitmapSource;
                        }
                    }

                    var groupItem = new GroupItem(group);
                    groupItem.SetAvatar(bmp);
                    _groupItems.Add(groupItem);

                    if (LoginedUser.Id.Equals(creatorId))
                    {
                        await SelectGroup(groupItem);
                        IsContactsDialogOpen = false;
                        MenuToggleButton.IsChecked = false;
                    }

                    UpdateOnlineStatus(group.Users);
                }
            }

            public async void CreateMessageCallback(MessageWCF message, long hash, string connectionId)
            {
                if (LoginedUser != null && message != null)
                {
                    // todo: получить аватарки
                    if (_groupItems.FirstOrDefault(g => g.Group.Id.Equals(message.GroupId)) is GroupItem groupItem)
                    {
                        if (message.GroupId.Equals(SelectedGroupId))
                        {
                            var isNew = false;

                            if (_messageItems.Any(mes =>
                                mes.Message.Sender.Login.Equals(message.Sender.Login) &&
                                mes.Message.GroupId.Equals(message.GroupId)))
                            {
                                if (message is MessageFileWCF messageFile)
                                {
                                    if (_messageItems.SingleOrDefault(mes =>
                                        mes.Message is MessageFileWCF messageFileItem &&
                                        messageFileItem.File.Name.Equals(messageFile.File.Name) &&
                                        messageFileItem.File.Hash.Equals(messageFile.File.Hash) &&
                                        messageFileItem.File.Lenght.Equals(messageFile.File.Lenght) &&
                                        mes.Message.Id.Equals((int)hash)) is MessageItem currentMessage)
                                    {
                                        currentMessage.IsMessageCome = true;
                                        currentMessage.FileDownloadState = FileDownloadState.Downloaded;
                                        currentMessage.Message.Id = message.Id;
                                        currentMessage.Message.DateTime = message.DateTime;
                                        currentMessage.Message.IsRead = message.IsRead;
                                    }
                                    else
                                    {
                                        isNew = true;
                                    }
                                }
                                else
                                {
                                    if (_messageItems.SingleOrDefault(mes =>
                                        mes.Message.Text.Equals(message.Text) &&
                                        mes.Message.DateTime.Ticks.Equals(hash)) is MessageItem currentMessage)
                                    {
                                        currentMessage.IsMessageCome = true;
                                        //currentMessage.Message.Id = message.Id;
                                        //currentMessage.Message.DateTime = message.DateTime;
                                        currentMessage.Message.IsRead = message.IsRead;
                                    }
                                    else
                                    {
                                        isNew = true;
                                    }
                                }
                            }
                            else
                            {
                                isNew = true;
                            }

                            if (isNew)
                            {
                                var avatar = _avatars.FirstOrDefault(ava =>
                                    ava.AvatarWCF is AvatarUserWCF wcf && wcf.User.Id.Equals(message.Sender.Id));
                                var bmp = GetBitmap(avatar);

                                _messageItems.Add(new MessageItem(message, message.Sender.Id.Equals(LoginedUser.Id), bmp)
                                { IsMessageCome = true });
                            }

                            await CheckConnection(async () => await _client.ReadAllMessagesInGroupAsync(message.GroupId));
                        }
                        else
                        {
                            if (LoginedUser.Id != message.Sender.Id)
                            {
                                groupItem.NotReadMessagesCount += 1;
                            }
                        }

                        CollectionViewSource
                            .GetDefaultView(_messageItems).Refresh();
                        CollectionViewSource
                            .GetDefaultView(_groupItems).Refresh();
                    }

                    UpdateOnlineStatus(new[] { message.Sender });
                }
            }

            public void AddFriendToGroupCallback(UserBaseWCF user, GroupWCF group, string connectionId)
            {
                if (LoginedUser != null)
                {
                    var groupItem = _groupItems.SingleOrDefault(gItem => gItem.Group.Id.Equals(group.Id));

                    if (groupItem != null)
                    {
                        var list = groupItem.Group.Users.ToList();
                        list.Add(user);
                        groupItem.Group.Users = list.ToArray();
                    }

                    UpdateOnlineStatus(group.Users);
                }
            }

            public async void ReadedMessagesCallback(GroupWCF group, UserBaseWCF sender, string connectionId)
            {
                if (LoginedUser != null)
                {
                    if (_groupItems.FirstOrDefault(g => g.Group.Id.Equals(group.Id)) is GroupItem groupItem)
                    {
                        Dictionary<int, int> keyValues = null;
                        if (!await CheckConnection(async () =>
                            keyValues = await _client.GetDontReadMessagesFromGroupsAsync(new[] { group.Id })))
                        {
                            return;
                        }

                        groupItem.NotReadMessagesCount = keyValues[group.Id];

                        if (SelectedGroupId.Equals(group.Id))
                        {
                            if (sender.Id.Equals(LoginedUser.Id))
                            {
                                foreach (var messageItem in _messageItems.Where(mItem =>
                                    mItem.Message.Sender.Id != LoginedUser.Id))
                                {
                                    messageItem.Message.IsRead = true;
                                }
                            }
                            else
                            {
                                foreach (var messageItem in _messageItems.Where(mItem =>
                                    mItem.Message.Sender.Id.Equals(LoginedUser.Id)))
                                {
                                    messageItem.Message.IsRead = true;
                                }
                            }
                        }
                    }

                    UpdateOnlineStatus(group.Users);
                }
            }

            public void SendedPackageCallback(int msgId, int numberPackage)
            {
                if (LoginedUser != null)
                {
                    if (_messageItems.SingleOrDefault(mItem => mItem.Message.Id.Equals(msgId)) is MessageItem messageItem)
                    {
                        messageItem.PackagesDownloaded++;
                    }
                }
            }

            public void ChangeOnlineStatusCallback(UserBaseWCF user, string connectionId)
            {
                if (LoginedUser != null)
                {
                    UpdateOnlineStatus(new[] { user });
                }
            }

            public async void IsOnlineCallback(string connectionId)
            {
                var users = new List<UserBaseWCF>();

                UserBaseWCF[] blacklist = null;
                if (!await CheckConnection(async () => blacklist = await _client.GetBlackListAsync()))
                {
                    return;
                }

                if (blacklist != null)
                {
                    foreach (var userBaseWcf in blacklist)
                    {
                        if (users.All(u => u.Id != userBaseWcf.Id))
                        {
                            users.Add(userBaseWcf);
                        }
                    }
                }

                UserBaseWCF[] contacts = null;
                if (!await CheckConnection(async () => contacts = await _client.GetFriendsAsync()))
                {
                    return;
                }

                if (contacts != null)
                {
                    foreach (var userBaseWcf in contacts)
                    {
                        if (users.All(u => u.Id != userBaseWcf.Id))
                        {
                            users.Add(userBaseWcf);
                        }
                    }
                }

                foreach (var groupWcf in _groupItems.Select(gItem => gItem.Group))
                {
                    foreach (var userBaseWcf in groupWcf.Users.Where(u => users.All(user => user.Id != u.Id)))
                    {
                        UserBaseWCF user = null;
                        if (!await CheckConnection(async () => user = await _client.FindAsync(userBaseWcf.Login)))
                        {
                            return;
                        }

                        if (user != null)
                        {
                            if (users.All(u => u.Id != user.Id))
                            {
                                users.Add(user);
                            }
                        }
                    }
                }

                UpdateOnlineStatus(users);
            }

            public void ChangeTextInMessageCallback(MessageWCF message, string connectionId)
            {
                // todo change text in message
                if (LoginedUser != null)
                {
                    UpdateOnlineStatus(new[] { message.Sender });
                }
            }

            public void RemoveOrExitUserFromGroupCallback(int groupId, UserBaseWCF user, string connectionId)
            {
                // todo: RemoveOrExitUserFromGroupCallback
                if (LoginedUser != null)
                {
                    UpdateOnlineStatus(new[] { user });
                }
            }

            public async void LogOutCallback(string connectionId)
            {
                await LogOut();
                MessageBox("Сессия завершена");
            }

            public async void AddContactCallback(UserBaseWCF user, string connectionId)
            {
                if (LoginedUser != null)
                {
                    var bmp = _defaultImage;

                    var avatar = _avatars.FirstOrDefault(ava => ava.AvatarWCF is AvatarUserWCF wcf &&
                                                                wcf.User.Id.Equals(user.Id));
                    if (avatar != null)
                    {
                        bmp = avatar.BitmapSource;
                    }
                    else
                    {
                        AvatarUserWCF[] avatars = null;
                        if (!await CheckConnection(async () => avatars = await _client.GetAvatarUsersAsync(new[] { user.Id })))
                        {
                            return;
                        }

                        foreach (var ava in avatars)
                        {
                            var newFilePath =
                                new FileInfo($"{Properties.Resources.GroupImagePath}\\{ava.User.Id}{ava.Format}").FullName;
                            using (var stream = File.OpenWrite(newFilePath))
                            {
                                stream.Write(ava.SmallData, 0, ava.SmallData.Length);
                            }

                            bmp = GetBitmapFromFile(newFilePath);
                            _avatars.Add(new UserImageItem { AvatarWCF = ava, BitmapSource = bmp });
                        }
                    }

                    _contactItems.Add(new ContactItem { UserImage = bmp, Contact = user });
                    UpdateOnlineStatus(new[] { user });
                }
            }

            public void RemoveContactCallback(UserBaseWCF user, string connectionId)
            {
                if (LoginedUser != null)
                {
                    var contact = _contactItems.SingleOrDefault(contactItem => contactItem.Contact.Id.Equals(user.Id));
                    if (contact != null)
                    {
                        _contactItems.Remove(contact);
                    }

                    UpdateOnlineStatus(new[] { user });
                }
            }

            public async void AddUserToBlackListCallback(UserBaseWCF user, string connectionId)
            {
                if (LoginedUser != null)
                {
                    var bmp = _defaultImage;

                    var avatar = _avatars.FirstOrDefault(ava => ava.AvatarWCF is AvatarUserWCF wcf &&
                                                                wcf.User.Id.Equals(user.Id));

                    if (avatar != null)
                    {
                        bmp = avatar.BitmapSource;
                    }
                    else
                    {
                        AvatarUserWCF[] avatars = null;
                        if (!await CheckConnection(async () => avatars = await _client.GetAvatarUsersAsync(new[] { user.Id })))
                        {
                            return;
                        }

                        foreach (var ava in avatars)
                        {
                            var newFilePath =
                                new FileInfo($"{Properties.Resources.GroupImagePath}\\{ava.User.Id}{ava.Format}").FullName;
                            using (var stream = File.OpenWrite(newFilePath))
                            {
                                stream.Write(ava.SmallData, 0, ava.SmallData.Length);
                            }

                            bmp = GetBitmapFromFile(newFilePath);
                            _avatars.Add(new UserImageItem { AvatarWCF = ava, BitmapSource = bmp });
                        }
                    }

                    _blackListItems.Add(new BlackListItem(user) { BlockedUserImage = bmp });
                    UpdateOnlineStatus(new[] { user });
                }
            }

            public void RemoveUserFromBlackListCallback(UserBaseWCF user, string connectionId)
            {
                if (LoginedUser != null)
                {
                    var blockedUser = _blackListItems.SingleOrDefault(blocked => blocked.User.Id.Equals(user.Id));
                    if (blockedUser != null)
                    {
                        _blackListItems.Remove(blockedUser);
                    }

                    UpdateOnlineStatus(new[] { user });
                }
            }
        }
    }