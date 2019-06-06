using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Client.ServiceReference;
using Microsoft.Win32;
using Image = System.Drawing.Image;

namespace Client
{
    public partial class MainWindow
    {
        #region converters

        private UserBaseWCF ConvertUserToUserBase_WCF(UserWCF userWcf)
        {
            return new UserBaseWCF {DisplayName = userWcf.DisplayName, Id = userWcf.Id, Login = userWcf.Login};
        }

        #endregion

        private async Task<bool> CheckConnection(AsyncDelegate action)
        {
            if (_client.ChannelFactory.State.Equals(CommunicationState.Opened) ||
                _client.ChannelFactory.State.Equals(CommunicationState.Created))
            {
                try
                {
                    await action.Invoke();
                    return true;
                }
                catch
                {
                    CommunicationErrorDialog.IsActive = true;

                    if (_client.State == CommunicationState.Faulted)
                    {
                        _client.Abort();
                        _client.ChannelFactory.Abort();
                        _client.InnerChannel.Abort();
                        _client.InnerDuplexChannel.Abort();
                    }
                    else
                    {
                        _client.Close();
                    }

                    return false;
                }
            }

            CommunicationErrorDialog.IsActive = true;
            if (_client.State == CommunicationState.Faulted)
            {
                _client.Abort();
                _client.ChannelFactory.Abort();
                _client.InnerChannel.Abort();
                _client.InnerDuplexChannel.Abort();
            }
            else
            {
                _client.Close();
            }

            return false;
        }

        private void CloseAllInnerSettings()
        {
            SettingsHeader = "Настройки";
            IsMainSettingsOpen = true;
            IsProfileSettingsOpen = false;
            IsSettingsPrivacyOpen = false;
        }

        private OpenFileDialog GetOFD(string title, string[] allowExtensions, string filterTitle)
        {
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = $"{filterTitle}|*" + string.Join(";*", allowExtensions) + ";",
                Multiselect = false,
                RestoreDirectory = true,
                Title = title
            };

            return ofd;
        }

        private async Task LogOut()
        {
            Title = "Zireael";
            IsLoadingOpen = true;
            SaveToConfig(LoginedUser != null ? LoginedUser.Login : LoginTextBox.Text, "");
            LoginedUserImage = null;
            foreach (var message in _messageItems)
            {
                message.UserImage = null;
            }

            foreach (var group in _groupItems)
            {
                group.GroupImage = null;
            }

            LoginedUser = null;
            _groupItems = null;
            _messageItems.Clear();
            MessagesItemsControl.ItemsSource = null;

            SelectedGroupId = -1;
            SideMenuDrawerHost.IsLeftDrawerOpen = false;
            AddUserNameTextBox.Text = string.Empty;
            ContactLoginTextBox.Text = string.Empty;
            MessageTextBox.Text = string.Empty;

            await CheckConnection(async () => await _client.LogOutAsync());

            GC.Collect();
            GC.WaitForPendingFinalizers();

            ShowGrid(LoginGrid);
            IsLoadingOpen = false;
        }

        private void OpenImage(MessageItem messageItem)
        {
            if (_avatars.SingleOrDefault(ava =>
                    ava.AvatarWCF is AvatarUserWCF avaUser &&
                    avaUser.User.Id.Equals(messageItem.Message.Sender.Id))
                is UserImageItem userImageItem)
            {
                var userId = ((AvatarUserWCF) userImageItem.AvatarWCF).User.Id;
                var imagePath = $"{Properties.Resources.UserImagesPath}\\{userId}.big.png";
                File.WriteAllBytes(imagePath, userImageItem.AvatarWCF.BigData);
                Process.Start(imagePath);
            }
        }

        private async Task Login(string login, string password, string token = null)
        {
            _groupItems = new ObservableCollection<GroupItem>();
            _messageItems = new ObservableCollection<MessageItem>();

            CollectionViewSource
                .GetDefaultView(_groupItems).SortDescriptions.Clear();

            CollectionViewSource
                .GetDefaultView(_messageItems).SortDescriptions.Clear();

            CollectionViewSource
                .GetDefaultView(_groupItems).SortDescriptions
                .Add(new SortDescription("LastMessageTime", ListSortDirection.Descending));

            CollectionViewSource
                .GetDefaultView(_messageItems).SortDescriptions
                .Add(new SortDescription("Message.DateTime", ListSortDirection.Ascending));


            LoginGrid.IsEnabled = false;
            LoginProgressBar.Visibility = Visibility.Visible;

            _haveInternetConnection = await CheckForInternetConnection(); // пока не нужно

            if (token == null)
            {
                if (!await CheckConnection(async () => LoginedUser = await _client.LogInAsync(login, password, null)))
                {
                    LoginGrid.IsEnabled = true;
                    LoginProgressBar.Visibility = Visibility.Collapsed;
                    return;
                }
            }
            else
            {
                if (!await CheckConnection(async () => LoginedUser = await _client.LogInAsync(null, null, token)))
                {
                    ShowGrid(LoginGrid);
                    LoginGrid.IsEnabled = true;
                    LoginProgressBar.Visibility = Visibility.Collapsed;

                    return;
                }
            }


            if (LoginedUser != null)
            {
                Title = $"Zireael - {LoginedUser.Login}";

                GroupItem.LoginedUser = LoginedUser;
                SettingsDialog.DataContext = this;
                SettingsNewEmailDialog.DataContext = this;
                SettingsLoginDialog.DataContext = this;
                SettingsChangePasswordDialog.DataContext = this;
                SideMenuDrawerHost.DataContext = this;
                ContactsDialog.DataContext = this;
                CreateGroupDialog.DataContext = this;
                AddContactsToGroupDialog.DataContext = this;
                BlackListDialog.DataContext = this;

                if (LoginedUser.DisplayName == null || string.IsNullOrWhiteSpace(LoginedUser.DisplayName))
                {
                    LoginedUser.DisplayName = LoginedUser.Login;
                }

                SelectedGroupId = -1;
                if (LoginedUser.Groups != null)
                {
                    foreach (var group in LoginedUser.Groups)
                    {
                        _groupItems.Add(new GroupItem(group));
                    }

                    _blackListItems.Clear();
                    UserBaseWCF[] blockedUsers = null;
                    if (!await CheckConnection(async () => blockedUsers = await _client.GetBlackListAsync()))
                    {
                        return;
                    }

                    foreach (var user in blockedUsers)
                    {
                        _blackListItems.Add(new BlackListItem(user));
                    }

                    _contactItems.Clear();
                    UserBaseWCF[] contacts = null;
                    if (!await CheckConnection(async () => contacts = await _client.GetFriendsAsync()))
                    {
                        return;
                    }

                    await GetAvatars(contacts.ToList());

                    foreach (var contact in contacts)
                    {
                        var avatar = _avatars.FirstOrDefault(ava =>
                            ava.AvatarWCF is AvatarUserWCF wcf && wcf.User.Id.Equals(contact.Id));

                        var bmp = GetBitmap(avatar);

                        _contactItems.Add(new ContactItem
                        {
                            Contact = contact,
                            UserImage = bmp
                        });
                    }

                    var loginedUserAvatar = _avatars.SingleOrDefault(av =>
                        av.AvatarWCF is AvatarUserWCF wcf && wcf.User.Id.Equals(LoginedUser.Id));
                    LoginedUserImage = loginedUserAvatar != null ? loginedUserAvatar.BitmapSource : _defaultImage;

                    Dictionary<int, int> dictionary = null;
                    if (!await CheckConnection(async () =>
                        dictionary = await _client.GetDontReadMessagesFromGroupsAsync(LoginedUser.Groups
                            .Select(g => g.Id)
                            .ToArray())))
                    {
                        return;
                    }

                    foreach (var group in _groupItems)
                    {
                        UserImageItem avatar = null;


                        if (group.Group.Type.Equals(GroupType.SingleUser))
                        {
                            avatar = _avatars.FirstOrDefault(ava => ava.AvatarWCF is AvatarUserWCF wcf &&
                                                                    wcf.User.Id.Equals(group.Group.Users
                                                                        .SingleOrDefault(u => u.Id != LoginedUser.Id)
                                                                        ?.Id));
                        }
                        else if (group.Group.Type.Equals(GroupType.MultyUser) ||
                                 group.Group.Type.Equals(GroupType.Channel))
                        {
                            avatar = _avatars.FirstOrDefault(ava => ava.AvatarWCF is AvatarGroupWCF wcf &&
                                                                    wcf.Group.Id.Equals(group.Group.Id));
                        }

                        var bmp = GetBitmap(avatar);
                        group.SetAvatar(bmp);
                        if (dictionary.ContainsKey(group.Group.Id))
                        {
                            group.NotReadMessagesCount = dictionary[group.Group.Id];
                        }
                    }

                    GroupsItemsControl.ItemsSource = _groupItems;
                    BlackListItemsControl.ItemsSource = _blackListItems;
                }

                foreach (var blockedUser in _blackListItems)
                {
                    var avatar = _avatars.FirstOrDefault(ava =>
                        ava.AvatarWCF is AvatarUserWCF wcf && wcf.User.Id.Equals(blockedUser.User.Id));

                    blockedUser.BlockedUserImage = GetBitmap(avatar);
                }

                ContactsItemsControl.ItemsSource = _contactItems;
                CreateGroupContactsItemsControl.ItemsSource = _contactItems;
                AddContactsToGroupItemsControl.ItemsSource = _contactItems;

                SaveToConfig(LoginedUser.Login, LoginedUser.Token);
                ShowGrid(MainGrid);
            }
            else
            {
                ShowGrid(LoginGrid);
                MessageBox("Неверный логин или пароль");
            }

            LoginProgressBar.Visibility = Visibility.Hidden;
            LoginGrid.IsEnabled = true;
            LoginPasswordBox.Password = string.Empty;
        }

        private async Task SelectGroup(object sender)
        {
            GroupItem useGroup = null;
            {
                if (sender is Grid tmpGrid && tmpGrid.DataContext is GroupItem tmpGroup)
                {
                    useGroup = tmpGroup;
                }
            }
            {
                if (sender is GroupItem tmpGroup)
                {
                    useGroup = tmpGroup;
                }
            }

            if (useGroup != null)
            {
                if (SelectedGroupId != useGroup.Group.Id)
                {
                    //IsLoadingOpen = true;

                    foreach (var groupItem in _groupItems)
                    {
                        groupItem.IsSelectedGroup = false;
                    }

                    SelectedGroupId = useGroup.Group.Id;
                    useGroup.IsSelectedGroup = true;

                    _messageItems.Clear();

                    MessageWCF[] messages = null;
                    if (!await CheckConnection(async () =>
                        messages = await _client.GetMessagesFromGroupAsync(useGroup.Group)))
                    {
                        return;
                    }

                    if (messages == null)
                    {
                        _client.Abort();
                        _client.ChannelFactory.Abort();
                        _client.InnerDuplexChannel.Abort();
                        CommunicationErrorDialog.IsActive = true;
                        return;
                    }


                    lock (this)
                    {
                        _messageItems.Clear();

                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        foreach (var message in messages)
                        {
                            if (message.GroupId != SelectedGroupId)
                            {
                                return;
                            }

                            var avatar = _avatars.FirstOrDefault(ava =>
                                ava.AvatarWCF is AvatarUserWCF wcf && wcf.User.Id.Equals(message.Sender.Id));
                            var bmp = GetBitmap(avatar);

                            var item = new MessageItem(message, message.Sender.Id.Equals(LoginedUser.Id), bmp)
                                {IsMessageCome = true};
                            _messageItems.Add(item);
                            if (message is MessageFileWCF messageFile)
                            {
                                item.CancellationTokenSource = new CancellationTokenSource();
                                item.PackagesCount = messageFile.File.CountPackages;

                                item.FileDownloadState =
                                    _savedFiles.Where(saved => saved.MessageId.Equals(message.Id))
                                        .Any(saved => saved.Equals(messageFile))
                                        ? FileDownloadState.Downloaded
                                        : FileDownloadState.None;
                            }

                            if (_blackListItems.Any(blocked => blocked.User.Id.Equals(message.Sender.Id)))
                            {
                                item.IsUserBlocked = true;
                            }

                            if (_contactItems.Any(contactItem => contactItem.Contact.Id.Equals(message.Sender.Id)))
                            {
                                item.IsContact = true;
                            }
                        }

                        MessagesItemsControl.ItemsSource = _messageItems;
                    }

                    if (!await CheckConnection(async () =>
                        await _client.ReadAllMessagesInGroupAsync(useGroup.Group.Id)))
                    {
                        return;
                    }
                    // todo: зависает ^

                    if (useGroup.Group.Type.Equals(GroupType.SingleUser))
                    {
                        UserBaseWCF user = null;

                        if (!await CheckConnection(async () =>
                            user = await _client.FindAsync(useGroup.Group.Users.Single(u => u.Id != LoginedUser.Id)
                                .Login)))
                        {
                            return;
                        }

                        GroupInfoGrid.DataContext = new GroupInfoItem
                        {
                            User = user,
                            Group = useGroup.Group,
                            HaveSelectedMessages = false,
                            IsUserBlockedInfo = _blackListItems.Any(blackListItem =>
                                blackListItem.User.Id.Equals(user.Id))
                        };
                    }
                    else if (useGroup.Group.Type.Equals(GroupType.MultyUser) ||
                             useGroup.Group.Type.Equals(GroupType.Channel))
                    {
                        GroupInfoGrid.DataContext = new GroupInfoItem
                        {
                            HaveSelectedMessages = false,
                            Group = useGroup.Group
                        };
                    }

                    MessagesScrollViewer.ScrollToEnd();
                }
            }

            //IsLoadingOpen = false;
        }

        private void SerializeSavedFiles()
        {
            CreateDirectoryIfNotExist(Properties.Resources.DataPath);

            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, _savedFiles);
                File.WriteAllBytes(
                    $"{Properties.Resources.DataPath}\\{Properties.Resources.SavedFileSerializeFileName}",
                    StringCipher.Encrypt(ms.ToArray(), Properties.Resources.Salt));
            }
        }

        private ObservableCollection<SavedFileInfo> DeserializeSavedFiles()
        {
            CreateDirectoryIfNotExist(Properties.Resources.DataPath);
            try
            {
                using (var ms = new MemoryStream(StringCipher.DecryptToBytes(File.ReadAllBytes(
                        $"{Properties.Resources.DataPath}\\{Properties.Resources.SavedFileSerializeFileName}"),
                    Properties.Resources.Salt)))
                {
                    return (ObservableCollection<SavedFileInfo>) new BinaryFormatter().Deserialize(ms);
                }
            }
            catch
            {
                return new ObservableCollection<SavedFileInfo>();
            }
        }

        private void CreateDirectoryIfNotExist(string path)
        {
            var dataDirectory = new DirectoryInfo(path);
            if (!dataDirectory.Exists)
            {
                Directory.CreateDirectory(dataDirectory.FullName);
            }
        }

        private void MessageBox(string message)
        {
            MessageDialogTextBlock.Text = message;
            IsMessageBoxOpen = true;
        }

        private async Task GetAvatars(List<UserBaseWCF> contacts)
        {
            var users = new List<UserBaseWCF> {ConvertUserToUserBase_WCF(LoginedUser)};

            foreach (var contact in contacts)
            {
                if (users.All(u => u.Id != contact.Id))
                {
                    users.Add(contact);
                }
            }

            foreach (var group in _groupItems)
            {
                if (group.Group.Users != null)
                {
                    foreach (var user in group.Group.Users)
                    {
                        if (users.All(u => u.Id != user.Id))
                        {
                            users.Add(user);
                        }
                    }
                }
            }

            foreach (var blockedItem in _blackListItems)
            {
                if (users.All(u => u.Id != blockedItem.User.Id))
                {
                    users.Add(blockedItem.User);
                }
            }

            AvatarWCF[] avatarsGroup = null;
            if (!await CheckConnection(async () =>
                avatarsGroup =
                    await _client.GetAvatarGroupsAsync(_groupItems.Select(grItem => grItem.Group.Id).ToArray())))
            {
                return;
            }

            AvatarUserWCF[] avatars = null;
            if (!await CheckConnection(async () => avatars = await _client.GetAvatarUsersAsync(users.Select(u => u.Id).ToArray())))
            {
                return;
            }

            _avatars = new ObservableCollection<UserImageItem>();

            if (!Directory.Exists(Properties.Resources.UserImagesPath))
            {
                Directory.CreateDirectory(Properties.Resources.UserImagesPath);
            }

            foreach (var ava in avatars)
            {
                var newFilePath = new FileInfo($"{Properties.Resources.UserImagesPath}\\{ava.User.Id}{ava.Format}")
                    .FullName;
                using (var stream = File.OpenWrite(newFilePath))
                {
                    stream.Write(ava.SmallData, 0, ava.SmallData.Length);
                }

                _avatars.Add(new UserImageItem {AvatarWCF = ava, BitmapSource = GetBitmapFromFile(newFilePath)});
            }

            if (!Directory.Exists(Properties.Resources.GroupImagePath))
            {
                Directory.CreateDirectory(Properties.Resources.GroupImagePath);
            }

            foreach (var ava in avatarsGroup)
            {
                if(ava is AvatarGroupWCF groupAvatar)
                {
                    var newFilePath = new FileInfo($"{Properties.Resources.GroupImagePath}\\{groupAvatar.Group.Id}{ava.Format}")
                   .FullName;
                    using (var stream = File.OpenWrite(newFilePath))
                    {
                        stream.Write(ava.SmallData, 0, ava.SmallData.Length);
                    }

                    _avatars.Add(new UserImageItem { AvatarWCF = ava, BitmapSource = GetBitmapFromFile(newFilePath) });
                }
               
            }
        }

        private async Task<bool> CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (await client.OpenReadTaskAsync(new Uri("http://clients3.google.com/generate_204")))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void ShowGrid(Grid grid)
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            RegistrationGrid.Visibility = Visibility.Collapsed;
            MainGrid.Visibility = Visibility.Collapsed;
            AuthGrid.Visibility = Visibility.Collapsed;

            grid.Visibility = Visibility.Visible;
        }

        private async Task SetDisplayname(bool isProfileSettingsClosedNow)
        {
            if (isProfileSettingsClosedNow)
            {
                if (Regex.IsMatch(ProfileDisplayNameTextBlock.Text, Properties.Resources.RegexDisplayNamePattern))
                {
                    var succsess = false;
                    if (!await CheckConnection(async () =>
                        succsess = await _client.ChangeProfileInfoAsync(ProfileDisplayNameTextBlock.Text, null)))
                    {
                        return;
                    }

                    if (succsess)
                    {
                        LoginedUser.DisplayName = ProfileDisplayNameTextBlock.Text;
                    }
                }
            }
        }

        private void SelectMessage(MessageItem messageItem)
        {
            messageItem.IsSelectedMessage = !messageItem.IsSelectedMessage;
            if (_messageItems.Any(mItem => mItem.IsSelectedMessage) &&
                GroupInfoGrid.DataContext is GroupInfoItem userItem)
            {
                userItem.HaveSelectedMessages = true;
            }
            else if (GroupInfoGrid.DataContext is GroupInfoItem userItem2)
            {
                userItem2.HaveSelectedMessages = false;
            }
        }

        private async Task LoginByToken()
        {
            var configData = LoadFromConfig();
            LoginTextBox.Text = configData[0];

            if (!string.IsNullOrWhiteSpace(configData[1]))
            {
                AuthLoginTextBox.Text = $"Вход в {configData[0]}";
                ShowGrid(AuthGrid);
                await Login(null, null, configData[1]);
            }
        }

        private void UpdateOnlineStatus(ICollection<UserBaseWCF> users)
        {
            foreach (var userBaseWcf in users)
            {
                var contactItem = _contactItems.SingleOrDefault(c => c.Contact.Id.Equals(userBaseWcf.Id));

                if (contactItem?.Contact != null)
                {
                    contactItem.Contact.LastTimeOnline = userBaseWcf.LastTimeOnline;
                    contactItem.Contact.IsOnline = userBaseWcf.IsOnline;

                    contactItem.Contact.LastTimeOnline += new TimeSpan(1);
                    //нужно для обновления биндинга
                }

                var gItem = _groupItems.SingleOrDefault(g =>
                    g.Group.Type.Equals(GroupType.SingleUser) && g.Group.Users
                        .SingleOrDefault(u => u.Id != LoginedUser.Id).Id.Equals(userBaseWcf.Id));

                if (gItem != null)
                {
                    var user = gItem.Group.Users.SingleOrDefault(u => u.Id != LoginedUser.Id);
                    user.LastTimeOnline = userBaseWcf.LastTimeOnline;
                    user.IsOnline = userBaseWcf.IsOnline;

                    if (gItem.IsSelectedGroup)
                    {
                        if (GroupInfoGrid.DataContext is GroupInfoItem userItem)
                        {
                            if (userItem.Group != null)
                            {
                                if (userItem.Group.Type.Equals(GroupType.SingleUser) &&
                                    userItem.User.Id.Equals(userBaseWcf.Id))
                                {
                                    userItem.User.LastTimeOnline = userBaseWcf.LastTimeOnline;
                                    userItem.User.IsOnline = userBaseWcf.IsOnline;

                                    userItem.User.LastTimeOnline += new TimeSpan(1);
                                    //нужно для обновления биндинга
                                }
                            }
                        }
                    }

                    var blacklist = _blackListItems.SingleOrDefault(b => b.User.Id.Equals(userBaseWcf.Id));

                    if (blacklist?.User != null)
                    {
                        blacklist.User.IsOnline = userBaseWcf.IsOnline;
                        blacklist.User.LastTimeOnline = userBaseWcf.LastTimeOnline;

                        blacklist.User.LastTimeOnline += new TimeSpan(1);
                    }

                    //нужно для обновления биндинга
                }
            }
        }

        private delegate Task AsyncDelegate();

        #region image

        private void GetImageFromFile(out string newImagePath, out string newBigImagePath)
        {
            var allowExtensions = new[] {".jpg", ".jpeg", ".png"};
            var ofd = GetOFD("Выбери аватар профиля", allowExtensions, "Image Files");

            newImagePath = null;
            newBigImagePath = null;

            if (ofd.ShowDialog().Equals(true))
            {
                var fileInfo = new FileInfo(ofd.FileName);
                if (fileInfo.Exists && allowExtensions.Any(ex => ex.Equals(fileInfo.Extension)))
                {
                    newImagePath =
                        new FileInfo($"{Properties.Resources.UserImagesPath}\\{LoginedUser.Id}.png")
                            .FullName;

                    newBigImagePath =
                        new FileInfo($"{Properties.Resources.UserImagesPath}\\{LoginedUser.Id}.big.png")
                            .FullName;

                    var dirInfo = new DirectoryInfo(newBigImagePath).Parent;
                    if (dirInfo != null && !dirInfo.Exists)
                    {
                        Directory.CreateDirectory(dirInfo.FullName);
                    }

                    if (File.Exists(newImagePath))
                    {
                        LoginedUserImage = null;
                        foreach (var message in _messageItems)
                        {
                            if (message.Message.Sender.Id.Equals(LoginedUser.Id))
                            {
                                message.UserImage = null;
                            }
                        }

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        File.Delete(newImagePath);
                    }

                    using (var image = Image.FromFile(fileInfo.FullName))
                    {
                        var width = image.Width;
                        var height = image.Height;

                        int newHeight;
                        int newWidth;
                        if (width > height)
                        {
                            newWidth = height;
                            newHeight = height;
                        }
                        else
                        {
                            newWidth = width;
                            newHeight = width;
                        }

                        var newImage = Crop(fileInfo.FullName,
                            new Rectangle(width / 2 - newWidth / 2, height / 2 - newHeight / 2, newWidth, newHeight));

                        var bigImage = ResizeImage(newImage, 512);
                        bigImage.Save(newBigImagePath, ImageFormat.Png);
                        var smallImage = ResizeImage(newImage, 64);
                        smallImage.Save(newImagePath, ImageFormat.Png);

                        newImage.Dispose();
                        smallImage.Dispose();
                        bigImage.Dispose();
                    }
                }
            }
        }

        private Image Crop(string path, Rectangle selection)
        {
            using (var bmp = new Bitmap(Image.FromFile(path)))
            {
                return bmp.Clone(selection, bmp.PixelFormat);
            }
        }

        private Image ResizeImage(Image sourceImage, int newSize)
        {
            var res = new Bitmap(newSize, newSize);
            using (var gr = Graphics.FromImage(res))
                gr.DrawImage(sourceImage, 0, 0, res.Width, res.Height);

            return res;
        }

        private BitmapSource GetBitmap(UserImageItem avatar)
        {
            var bmp = _defaultImage;
            if (avatar != null)
            {
                bmp = avatar.BitmapSource;
            }

            return bmp;
        }

        private BitmapSource GetBitmapFromStream(FileStream imageStream)
        {
            using (var img = Image.FromStream(imageStream))
            {
                return Imaging.CreateBitmapSourceFromHBitmap(((Bitmap) img).GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }

        private BitmapSource GetBitmapFromFile(string imagePath)
        {
            using (var stream = File.OpenRead(imagePath))
            {
                return GetBitmapFromStream(stream);
            }
        }

        #endregion

        #region config

        private void SaveToConfig(string login, string token)
        {
            CreateDirectoryIfNotExist(Properties.Resources.DataPath);

            File.WriteAllBytes($"{Properties.Resources.DataPath}\\{Properties.Resources.UsernamePath}",
                StringCipher.Encrypt(login, Properties.Resources.Salt));
            File.WriteAllBytes($"{Properties.Resources.DataPath}\\{Properties.Resources.AuthDataPath}",
                StringCipher.Encrypt(token, Properties.Resources.Salt));
        }

        private string[] LoadFromConfig()
        {
            string login = null;
            string token = null;

            var loginPath = $"{Properties.Resources.DataPath}\\{Properties.Resources.UsernamePath}";
            var tokenPath = $"{Properties.Resources.DataPath}\\{Properties.Resources.AuthDataPath}";

            if (File.Exists(loginPath))
            {
                login = StringCipher.DecryptToString(File.ReadAllBytes(loginPath),
                    Properties.Resources.Salt);
            }

            if (File.Exists(tokenPath))
            {
                token = StringCipher.DecryptToString(File.ReadAllBytes(tokenPath),
                    Properties.Resources.Salt);
            }

            return new[] {login?.Replace("\0", string.Empty), token?.Replace("\0", string.Empty)};
        }

        #endregion
    }
}