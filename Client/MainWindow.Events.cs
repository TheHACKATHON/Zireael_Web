using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using AdditionsLibrary;
using Client.ServiceReference;
using Microsoft.WindowsAPICodePack.Shell;

namespace Client
{
    public partial class MainWindow
    {
        private async void ChatOpen_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UserBaseWCF user = null;
            if (sender is MenuItem mi)
            {
                if (mi.DataContext is ContactItem contactItem && contactItem.Contact is UserBaseWCF user1)
                {
                    user = user1;
                }
                else if (mi.DataContext is UserBaseWCF user2)
                {
                    user = user2;
                }
                else if (mi.DataContext is BlackListItem blocked && blocked.User is UserBaseWCF user3)
                {
                    user = user3;
                }
                else if (mi.DataContext is MessageItem messageItem && messageItem.Message.Sender is UserBaseWCF user4)
                {
                    user = user4;
                }
                else
                {
                    throw new ActionNotSupportedException();
                }
            }
            else if (sender is Grid grid)
            {
                if (grid.DataContext is ContactItem contactItem &&
                    contactItem.Contact is UserBaseWCF user1)
                {
                    user = user1;
                }
                else if (grid.DataContext is BlackListItem blockedItem &&
                         blockedItem.User is UserBaseWCF user2)
                {
                    user = user2;
                }
                else if (grid.DataContext is GroupItem groupItem1)
                {
                    await SelectGroup(groupItem1);
                    return;
                }
                else
                {
                    throw new ActionNotSupportedException();
                }
            }

            GroupItem groupItem;
            if ((groupItem = _groupItems.FirstOrDefault(gr =>
                    gr.Group.Type.Equals(GroupType.SingleUser) &&
                    gr.Group.Users.Any(usr => usr.Id.Equals(user.Id)))) != null)
            {
                IsContactsDialogOpen = false;
                IsSettingsOpen = false;
                IsBlackListOpen = false;
                MenuToggleButton.IsChecked = false;
                await SelectGroup(groupItem);
                MessageTextBox.Focus();
            }
            else
            {
                UserBaseWCF findUser = null;
                if (!await CheckConnection(async () => findUser = await _client.FindAsync(user.Login)))
                {
                    return;
                }

                if (findUser != null)
                {
                    IsLoadingOpen = true;
                    await CheckConnection(async () => await _client.CreateChatAsync(findUser));
                    IsLoadingOpen = false;
                }
                else
                {
                    MessageBox("Нет пользователя с таким логином");
                }
            }
        }

        private void Login_Grid_Click(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                LoginTextBox.Focus();
            }
            else
            {
                LoginPasswordBox.Focus();
            }
        }

        private async void ReadAllMessages_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            await CheckConnection(async () => await _client.ReadAllMessagesInAllGroupsAsync());
            SubMenuPopup.IsPopupOpen = false;
        }

        private void SavedFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SerializeSavedFiles();
        }

        private async void DeleteGroup_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mItem && mItem.DataContext is GroupItem gItem)
            {
                if (gItem.Group.Type.Equals(GroupType.SingleUser))
                {
                    await CheckConnection(async () => await _client.RemoveGroupAsync(gItem.Group));
                }
                else if (gItem.Group.Type.Equals(GroupType.MultyUser))
                {
                    await CheckConnection(async () =>
                        await _client.RemoveOrExitFromGroupAsync(gItem.Group.Id, LoginedUser.Id));
                }
            }
        }

        private void MessageBoxOk_Button_Click(object sender, RoutedEventArgs e)
        {
            IsMessageBoxOpen = false;
        }

        private async void SearchItem_Button_Click(object sender, MouseButtonEventArgs e)
        {
            // todo: клик по результату поиска
        }

        private async void SearchUsers_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // todo поиск юзеров
            if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            {
                _searchItems.Clear();
                // todo: CheckConnection
                var collection = await _client.FindAllAsync(textBox.Text, 5);
                var avatars = await _client.GetAvatarUsersAsync(collection.Select(c => c.Id).ToArray());

                foreach (var user in collection)
                {
                    _searchItems.Add(new GroupItem());
                }
            }
        }

        private async void Reconnect_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _client = new CeadChatServiceClient(new InstanceContext(this));
                _client.Endpoint.Binding.SendTimeout = new TimeSpan(1, 0, 0);
                _client.Open();
                CommunicationErrorDialog.IsActive = false;
                if (LoginedUser != null)
                {
                    await LoginByToken();
                }
            }
            catch
            {
                // ignored
            }
        }

        #region set avatar

        private async void ChangeUserImage_Button_Click(object sender, RoutedEventArgs e)
        {
            GetImageFromFile(out var newImagePath, out var newBigImagePath);

            if (newImagePath == null || newBigImagePath == null)
            {
                return;
            }

            var avat = new AvatarUserWCF
            {
                User = ConvertUserToUserBase_WCF(LoginedUser),
                DateTime = DateTime.Now,
                Format = ".png",
                SmallData = HashCode.GetBytesFromFile(newImagePath),
                BigData = HashCode.GetBytesFromFile(newBigImagePath)
            };

            if (!await CheckConnection(async () => await _client.SetAvatarUserAsync(avat)))
            {
                return;
            }

            var avatar = _avatars.SingleOrDefault(ava =>
                ava.AvatarWCF is AvatarUserWCF wcf &&
                wcf.User.Id.Equals(LoginedUser.Id));

            if (avatar == null)
            {
                avatar = new UserImageItem {AvatarWCF = avat};
                _avatars.Add(avatar);
            }

            avatar.BitmapSource = GetBitmapFromFile(newImagePath);
            LoginedUserImage = avatar.BitmapSource;

            foreach (var item in _messageItems)
            {
                if (item.Message.Sender.Id.Equals(LoginedUser.Id))
                {
                    item.UserImage = avatar.BitmapSource;
                }
            }
        }

        private async void SetGroupImage_Button_Click(object sender, RoutedEventArgs e)
        {
            GetImageFromFile(out var newImagePath, out var newBigImagePath);

            if (newImagePath == null || newBigImagePath == null)
            {
                return;
            }

            var selectedGroup = _groupItems.SingleOrDefault(gro => gro.IsSelectedGroup);
            if (selectedGroup == null)
            {
                return;
            }

            var avatarGroup = new AvatarGroupWCF
            {
                Group = selectedGroup.Group,
                DateTime = DateTime.Now,
                Format = ".png",
                SmallData = HashCode.GetBytesFromFile(newImagePath),
                BigData = HashCode.GetBytesFromFile(newBigImagePath)
            };

            if (!await CheckConnection(async () => await _client.SetAvatarGroupAsync(avatarGroup)))
            {
                return;
            }

            var avatarItem = _avatars.SingleOrDefault(ava =>
                ava.AvatarWCF is AvatarGroupWCF wcf &&
                wcf.Group.Id.Equals(selectedGroup.Group.Id));

            if (avatarItem == null)
            {
                avatarItem = new UserImageItem {AvatarWCF = avatarGroup};
                _avatars.Add(avatarItem);
            }

            avatarItem.BitmapSource = GetBitmapFromFile(newImagePath);
            selectedGroup.GroupImage = avatarItem.BitmapSource;
        }

        #endregion

        #region messages

        private void Message_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is MessageItem mItem)
            {
                if (e.OriginalSource is Ellipse)
                {
                    return;
                }

                if (!mItem.IsRight)
                {
                    e.Handled = true;
                }
            }
        }

        private async void MessageDelete_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is MessageItem messageItem)
            {
                await CheckConnection(async () => await _client.DeleteMessageAsync(messageItem.Message));
            }
        }

        private async void MessageFile_IconButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is MessageItem messageItem &&
                messageItem.Message is MessageFileWCF messageFile)
            {
                switch (messageItem.FileDownloadState)
                {
                    case FileDownloadState.Downloaded:
                    {
                        if (_savedFiles.Where(saved => saved.MessageId.Equals(messageFile.Id))
                                .FirstOrDefault(saved => saved.Equals(messageFile)) is SavedFileInfo
                            savedFileInfo)
                        {
                            if (File.Exists($"{savedFileInfo.FullName}"))
                            {
                                Process.Start(savedFileInfo.FullName);
                            }
                            else
                            {
                                _savedFiles.Remove(savedFileInfo);
                                messageItem.FileDownloadState = FileDownloadState.None;
                                goto case FileDownloadState.None;
                            }
                        }
                    }
                        break;
                    case FileDownloadState.Downloading:
                    case FileDownloadState.Uploadind:
                    {
                        messageItem.CancellationTokenSource.Cancel();
                    }
                        break;
                    case FileDownloadState.None:
                    {
                        var packageNumber = 0;
                        var packages = new List<Package>();

                        messageItem.FileDownloadState = FileDownloadState.Downloading;
                        while (true)
                        {
                            if (messageItem.CancellationTokenSource.Token.IsCancellationRequested)
                            {
                                break;
                            }

                            Package pack = null;

                            if (!await CheckConnection(async () =>
                                pack = await _client.GetPackageFromFileAsync(messageItem.Message.Id, packageNumber)))
                            {
                                return;
                            }

                            if (pack == null)
                            {
                                break;
                            }

                            packages.Add(pack);
                            packageNumber++;
                        }

                        if (messageItem.CancellationTokenSource.Token.IsCancellationRequested)
                        {
                            messageItem.FileDownloadState = FileDownloadState.None;
                            messageItem.CancellationTokenSource = new CancellationTokenSource();
                        }
                        else
                        {
                            var orderedPackages = packages.OrderBy(p => p.Number);
                            using (var stream = new MemoryStream())
                            {
                                var downloadDir =
                                    new DirectoryInfo(
                                        $"{KnownFolders.Downloads.Path}\\{Properties.Resources.DownloadPath}");
                                var fileInfo = new FileInfo($"{downloadDir.FullName}\\{messageFile.File.Name}");
                                foreach (var pack in orderedPackages)
                                {
                                    stream.Write(pack.Data, 0, pack.Data.Length);
                                }

                                CreateDirectoryIfNotExist(downloadDir.FullName);
                                var counter = 1;
                                var fileName = fileInfo.Name;
                                var flag = false;

                                while (!flag)
                                {
                                    var fullName = $"{downloadDir.FullName}\\{fileName}";
                                    if (!File.Exists(fullName))
                                    {
                                        flag = true;
                                        File.WriteAllBytes(fullName, stream.ToArray());
                                        _savedFiles.Add(new SavedFileInfo(fullName, fileName,
                                            new FileInfo(fullName).LastWriteTime, (int) stream.Length, messageFile.Id));
                                    }
                                    else
                                    {
                                        fileName =
                                            $"{fileInfo.Name.Remove(fileInfo.Name.IndexOf(fileInfo.Extension, StringComparison.OrdinalIgnoreCase))}" +
                                            $"({counter++}){fileInfo.Extension}";
                                    }
                                }
                            }

                            messageItem.FileDownloadState = FileDownloadState.Downloaded;
                        }
                    }
                        break;
                }
            }
        }

        private async void MessagesDelete_Button_Click(object sender, RoutedEventArgs e)
        {
            var messagesToDelete = _messageItems.Where(mItem => mItem.IsSelectedMessage && mItem.Message.Id != 0).ToList();
            for (var i = messagesToDelete.Count - 1; i >= 0; i--)
            {
                if (!await CheckConnection(async () => await _client.DeleteMessageAsync(messagesToDelete[i].Message)))
                {
                    return;
                }
            }

            if (_messageItems.All(mItem => !mItem.IsSelectedMessage) &&
                GroupInfoGrid.DataContext is GroupInfoItem userItem2)
            {
                userItem2.HaveSelectedMessages = false;
            }
        }

        private async void SendFile_Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = GetOFD("Выбери файл", new[] {".*"}, "Все файлы");

            var result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var bytes = HashCode.GetBytesFromFile(openFileDialog.FileName);
                var partsize = int.Parse(Properties.Resources.PackageSize); // 256 Кбайт
                var fileInfo = new FileInfo(openFileDialog.FileName);

                var countParts = bytes.Length / (double) partsize;
                if (countParts > (int) countParts)
                {
                    countParts = (int) countParts + 1;
                }

                var msg = new MessageFileWCF
                {
                    File = new FileChatWCF
                    {
                        Name = fileInfo.Name,
                        Lenght = bytes.Length,
                        Hash = HashCode.ComputeFromBytes(bytes),
                        CountPackages = (int) countParts
                    },
                    DateTime = DateTime.Now,
                    IsRead = false,
                    Sender = LoginedUser,
                    GroupId = SelectedGroupId,
                    Text = string.Empty,
                    Type = FileType.File
                };

                var avatar = _avatars.FirstOrDefault(ava =>
                    ava.AvatarWCF is AvatarUserWCF wcf && wcf.User.Id.Equals(LoginedUser.Id));
                var bmp = GetBitmap(avatar);
                var item = new MessageItem(msg, true, bmp)
                {
                    CancellationTokenSource = new CancellationTokenSource(),
                    FileDownloadState = FileDownloadState.Uploadind,
                    IsMessageCome = false,
                    PackagesCount = (int) countParts
                };
                _messageItems.Add(item);

                if (!await CheckConnection(async () =>
                    msg.Id = await _client.SendMessageAsync(msg, msg.DateTime.Ticks)))
                {
                    return;
                }

                var position = 0;
                var partNumber = 0;
                for (var i = 0; i < bytes.Length; i += partsize)
                {
                    if (item.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    var partbytes = new byte[Math.Min(partsize, bytes.Length - i)];
                    for (var j = 0; j < partbytes.Length; j++)
                    {
                        partbytes[j] = bytes[position++];
                    }

                    if (!await CheckConnection(async () =>
                        await _client.SendPackageToFileAsync(msg.Id,
                            new Package {Number = partNumber, Data = partbytes})))
                    {
                        return;
                    }

                    partNumber++;
                }

                if (item.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    _messageItems.Remove(item);
                }
                else
                {
                    _savedFiles.Add(new SavedFileInfo(openFileDialog.FileName, fileInfo.Name, fileInfo.LastWriteTime,
                        bytes.Length, msg.Id));
                    item.FileDownloadState = FileDownloadState.Downloaded;
                }
            }
        }

        private async void SendMessage_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                if (_groupItems != null &&
                    _groupItems.SingleOrDefault(group => group.IsSelectedGroup) is GroupItem groupItem)
                {
                    var msg = new MessageWCF
                    {
                        Text = MessageTextBox.Text,
                        GroupId = groupItem.Group.Id,
                        DateTime = DateTime.Now,
                        Sender = LoginedUser
                    };
                    MessageTextBox.Text = string.Empty;

                    var avatar = _avatars.FirstOrDefault(ava =>
                        ava.AvatarWCF is AvatarUserWCF wcf &&
                        wcf.User.Id.Equals(LoginedUser.Id));

                    _messageItems.Add(new MessageItem(msg, true, GetBitmap(avatar)) {IsMessageCome = false});

                    await CheckConnection(async () => await _client.SendMessageAsync(msg, msg.DateTime.Ticks));
                }

                MessagesScrollViewer.ScrollToEnd();
            }
        }

        private void Message_Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            {
                if (e.Source is Ellipse ellipse && ellipse.DataContext is MessageItem messageItem)
                {
                    OpenImage(messageItem);
                    return;
                }
            }
            {
                if (sender is Grid grid && grid.DataContext is MessageItem messageItem && messageItem.IsRight &&
                    messageItem.IsMessageCome)
                {
                    if (_messageItems.All(mItem => !mItem.IsSelectedMessage))
                    {
                        _holdMouse.Stop();
                    }
                    else
                    {
                        if (_holdMouse.SelectedNow)
                        {
                            grid.ReleaseMouseCapture();
                            _holdMouse.SelectedNow = false;
                        }
                        else
                        {
                            SelectMessage(messageItem);
                        }
                    }
                }
            }
        }

        private async void Message_Item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_messageItems.All(mItem => !mItem.IsSelectedMessage))
            {
                if (await _holdMouse.Start())
                {
                    if (sender is Grid grid && grid.DataContext is MessageItem messageItem && messageItem.IsRight &&
                        messageItem.IsMessageCome)
                    {
                        grid.ReleaseMouseCapture();
                        if (GroupInfoGrid.DataContext is GroupInfoItem userItem)
                        {
                            userItem.HaveSelectedMessages = true;
                        }

                        Mouse.Capture(grid, CaptureMode.SubTree);
                        _holdMouse.SelectedNow = true;
                        messageItem.IsSelectedMessage = true;
                    }
                }
            }
        }

        private void MessageSelect_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is MessageItem messageItem)
            {
                SelectMessage(messageItem);
            }
        }

        private void OpenImage_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is MessageItem messageItem)
            {
                OpenImage(messageItem);
            }
        }

        #endregion

        #region header

        private void GridHeader_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonFechar_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ExpandWindow_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Equals(WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void Collapse_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        #endregion

        #region contacts

        private void ContactsOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            IsContactsDialogOpen = true;
            SideMenuDrawerHost.IsLeftDrawerOpen = false;
        }

        private void ContactsClose_Button_Click(object sender, RoutedEventArgs e)
        {
            IsContactsDialogOpen = false;
        }

        private async void ContactAdd_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ContactLoginTextBox.Text) &&
                !ContactLoginTextBox.Text.Equals(LoginedUser.Login, StringComparison.OrdinalIgnoreCase))
            {
                UserBaseWCF gItem;
                if ((gItem = _contactItems.FirstOrDefault(g =>
                            g.Contact.Login.Equals(ContactLoginTextBox.Text, StringComparison.OrdinalIgnoreCase))
                        ?.Contact) != null)
                {
                    MessageBox($"{gItem.Login} уже в контактах");
                }
                else
                {
                    IsLoadingOpen = true;
                    var userExist = false;
                    if (!await CheckConnection(async () =>
                        userExist = await _client.LoginExistAsync(ContactLoginTextBox.Text)))
                    {
                        return;
                    }

                    if (!userExist)
                    {
                        MessageBox("Нет пользователя с таким логином");
                    }
                    else
                    {
                        if (!await CheckConnection(async () => await _client.AddFriendAsync(ContactLoginTextBox.Text)))
                        {
                            return;
                        }
                    }

                    IsLoadingOpen = false;
                }
            }
            else
            {
                ContactLoginTextBox.Focus();
            }
        }

        private async void ContactDelete_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                if (mi.DataContext is ContactItem contactItem && contactItem.Contact is UserBaseWCF user1)
                {
                    IsLoadingOpen = true;
                    await CheckConnection(async () => await _client.RemoveFriendAsync(user1));
                    IsLoadingOpen = false;
                }
                else if (mi.DataContext is BlackListItem blocked && blocked.User is UserBaseWCF user2)
                {
                    IsLoadingOpen = true;
                    await CheckConnection(async () => await _client.RemoveFromBlackListAsync(user2));
                    IsLoadingOpen = false;
                }
            }
        }

        private void ContactSelect_Item_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid tmpGrid && tmpGrid.DataContext is ContactItem contactItem)
            {
                contactItem.IsSelectedContact = !contactItem.IsSelectedContact;
            }
        }

        private void AddContactToGroupOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            var groupItem = _groupItems.SingleOrDefault(gItem => gItem.IsSelectedGroup);

            if (groupItem != null)
            {
                foreach (var user in groupItem.Group.Users)
                {
                    var contactItem = _contactItems.SingleOrDefault(cItem => cItem.Contact.Id.Equals(user.Id));

                    if (contactItem != null)
                    {
                        contactItem.IsSelectedContact = true;
                        contactItem.IsDisabledContact = true;
                    }
                }

                IsAddContactsToGroupDialogOpen = true;
            }
        }

        private async void AddContactToGroup_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_contactItems.All(contactItem =>
                !contactItem.IsSelectedContact || contactItem.IsSelectedContact && contactItem.IsDisabledContact))
            {
                MessageBox("Выберите хотя бы одного друга");
                return;
            }

            IsLoadingOpen = true;

            await CheckConnection(async () => await _client.AddFriendsToGroupAsync(
                _groupItems.SingleOrDefault(gItem => gItem.IsSelectedGroup)?.Group,
                _contactItems.Where(cItem => cItem.IsSelectedContact && !cItem.IsDisabledContact)
                    .Select(cItem2 => cItem2.Contact).ToArray()));

            IsLoadingOpen = false;
            IsAddContactsToGroupDialogOpen = false;
        }

        private void ContactsAddToGroupClose_Button_Click(object sender, RoutedEventArgs e)
        {
            IsAddContactsToGroupDialogOpen = false;
        }

        private void ContactItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var messageItem in _messageItems)
            {
                messageItem.IsContact =
                    _contactItems.Any(contact => contact.Contact.Id.Equals(messageItem.Message.Sender.Id));
            }

            CollectionViewSource
                .GetDefaultView(_messageItems).Refresh();
        }

        private async void ContactAddOrRemove_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is MessageItem messageItem &&
                !messageItem.Message.Sender.Id.Equals(LoginedUser.Id))
            {
                if (_contactItems.Any(contactItem => contactItem.Contact.Id.Equals(messageItem.Message.Sender.Id)))
                {
                    await CheckConnection(async () => await _client.RemoveFriendAsync(messageItem.Message.Sender));
                }
                else
                {
                    await CheckConnection(async () => await _client.AddFriendAsync(messageItem.Message.Sender.Login));
                }
            }
        }

        #endregion

        #region mainwindow

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoginByToken();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                ShowGrid(LoginGrid);
            }
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            await CheckConnection(async () => await _client.LogOutAsync());
        }

        #endregion

        #region account

        private async void AccountLogIn_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LoginTextBox.Text)
                && !string.IsNullOrWhiteSpace(LoginPasswordBox.Password))
            {
                await Login(LoginTextBox.Text, LoginPasswordBox.Password);
            }
            else
            {
                MessageBox("Заполните поля");
            }
        }

        private async void AccountLogOut_Button_Click(object sender, RoutedEventArgs e)
        {
            await LogOut();
        }

        #endregion

        #region registration

        private void RegistrationOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            ShowGrid(RegistrationGrid);
        }

        private void RegistrationClose_Button_Click(object sender, RoutedEventArgs e)
        {
            ShowGrid(LoginGrid);
        }

        private async void RegistrationSave_Button_Click(object sender, RoutedEventArgs e)
        {
            RegistrationProgressBar.Visibility = Visibility.Visible;
            SettintsStackPanel.IsEnabled = false;

            var isError = false;

            if (!Regex.IsMatch(RegistrationLoginTextBox.Text, Properties.Resources.RegexLoginPattern))
            {
                isError = true;
                MessageBox(Properties.Resources.LoginRequiredString);
            }

            if (!isError && !string.IsNullOrWhiteSpace(RegistrationEmailTextBox.Text))
            {
                if (!Regex.IsMatch(RegistrationEmailTextBox.Text, Properties.Resources.RegexEmailPattern))
                {
                    MessageBox("Введите корректный почтовый адрес");
                    isError = true;
                }
            }

            if (!isError && !Regex.IsMatch(RegistrationPasswordBox.Password, Properties.Resources.RegexPasswordPattern))
            {
                isError = true;
                MessageBox(Properties.Resources.PasswordRequiredString);
            }

            if (!isError && !RegistrationPasswordBox.Password.Equals(RegistrationConfirmPasswordBox.Password))
            {
                isError = true;
                MessageBox("Пароли не совпадают");
            }

            var loginExist = false;
            if (!await CheckConnection(async () =>
                loginExist = await _client.LoginExistAsync(RegistrationLoginTextBox.Text.Trim())))
            {
                return;
            }

            if (!isError && loginExist)
            {
                isError = true;
                MessageBox("Такой логин уже существует");
            }

            if (!isError)
            {
                var tempUser = new UserWCF
                {
                    Login = RegistrationLoginTextBox.Text,
                    PasswordHash = RegistrationPasswordBox.Password,
                    Email = RegistrationEmailTextBox.Text
                };

                // todo: сделать проверку логина по FocusLost()
                var registerSuccsess = false;
                if (!await CheckConnection(async () => registerSuccsess = await _client.RegistrationAsync(tempUser)))
                {
                    return;
                }

                if (registerSuccsess)
                {
                    LoginTextBox.Text = RegistrationLoginTextBox.Text;

                    RegistrationLoginTextBox.Text = string.Empty;
                    RegistrationEmailTextBox.Text = string.Empty;
                    RegistrationPasswordBox.Password = string.Empty;
                    RegistrationConfirmPasswordBox.Password = string.Empty;

                    ShowGrid(LoginGrid);
                }
                else
                {
                    MessageBox("Неизвестная ошибка");
                }

                ShowGrid(LoginGrid);
            }

            SettintsStackPanel.IsEnabled = true;
            RegistrationProgressBar.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region settings

        private void SettingsOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            MenuToggleButton.IsChecked = false;
            IsSettingsOpen = true;
        }

        private async void SettingsClose_Button_Click(object sender, RoutedEventArgs e)
        {
            IsSettingsOpen = false;
            await SetDisplayname(IsProfileSettingsOpen);
        }

        private async void SettingsClose_Background_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid background && background.Parent is Grid settings)
            {
                if (settings.Tag is DependencyProperty isOpen)
                {
                    SetValue(isOpen, false);
                    if (settings.Name.Equals("SettingsDialog"))
                    {
                        //var isProfileSettingsClosedNow = IsProfileSettingsOpen;
                        //CloseAllInnerSettings();

                        await SetDisplayname(IsProfileSettingsOpen);
                    }

                    // так надо {
                    IsEmailSettingsOpen = IsEmailSettingsOpen;
                    IsSettingsPasswordOpen = IsSettingsPasswordOpen;
                    IsRestorePasswordOpen = IsRestorePasswordOpen;
                    IsCreateGroupDialogOpen = IsCreateGroupDialogOpen;
                    IsBlackListOpen = IsBlackListOpen;
                    IsContactsDialogOpen = IsContactsDialogOpen;
                    // }
                }
            }
        }

        private async void SettingsBack_Button_Click(object sender, RoutedEventArgs e)
        {
            var isProfileSettingsClosedNow = IsProfileSettingsOpen;
            CloseAllInnerSettings();

            await SetDisplayname(isProfileSettingsClosedNow);
        }

        #endregion

        #region profile settings

        private void ProfileSettingsOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog.DataContext = this;
            IsMainSettingsOpen = false;
            IsProfileSettingsOpen = true;
            SettingsHeader = "Профиль";
        }

        #region email change dialog

        private void EmailSettingsOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            IsEmailSettingsOpen = true;
        }

        private void EmailSettingsClose_Button_Click(object sender, RoutedEventArgs e)
        {
            IsEmailSettingsOpen = false;
        }

        private async void EmailSettingsSendCodeToNewMail_Button_Click(object sender, RoutedEventArgs e)
        {
            var isCodeSended = false;
            if (!await CheckConnection(async () =>
                isCodeSended =
                    await _client.SendCodeForSetNewEmailAsync(NewEmailTextBox.Text,
                        NewEmailPasswordPasswordBox.Password)))
            {
                return;
            }

            if (isCodeSended)
            {
                NewEmailTextBox.IsReadOnly = true;
                SettingsEmailCodeTextBox.Visibility = Visibility.Visible;
                MessageBox("Код отправлен");
            }
            else
            {
                MessageBox("Неверный пароль");
            }
        }

        private async void EmailSettingsSaveNewEmail_Button_Click(object sender, RoutedEventArgs e)
        {
            var isEmailDone = false;
            if (!await CheckConnection(async () =>
                isEmailDone = await _client.SetNewEmailAsync(SettingsEmailCodeTextBox.Text)))
            {
                return;
            }

            if (isEmailDone)
            {
                LoginedUser.Email = NewEmailTextBox.Text;
                IsEmailSettingsOpen = false;
                MessageBox("Почта изменена");
            }
            else
            {
                MessageBox("Неверный код");
                NewEmailTextBox.IsReadOnly = false;
            }
        }

        private void EmailSettingaNewEmailTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                IsEmailVerifiedForEmailSettings = Regex.IsMatch(textBox.Text, Properties.Resources.RegexEmailPattern);
            }
        }

        private void EmailSettingsPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                IsPasswordVerifiedForEmailSettings =
                    Regex.IsMatch(passwordBox.Password, Properties.Resources.RegexPasswordPattern);
            }
        }

        #endregion

        #region change login dialog

        private void LoginSettingsOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            LoginSettingsLoginTextBox.Text = LoginedUser.Login;
            IsSettingsLoginOpen = true;
        }

        private void LoginSettingsClose_Button_Click(object sender, RoutedEventArgs e)
        {
            IsSettingsLoginOpen = false;
        }

        private async void LoginSettingsSaveNewLogin_Button_Click(object sender, RoutedEventArgs e)
        {
            LoginSettingsLoginTextBox.IsReadOnly = true;

            var isProfileSaved = false;
            if (!await CheckConnection(async () =>
                isProfileSaved = await _client.ChangeProfileInfoAsync(null, LoginSettingsLoginTextBox.Text)))
            {
                return;
            }

            if (isProfileSaved)
            {
                LoginedUser.Login = LoginSettingsLoginTextBox.Text;
                MessageBox("Логин изменен");
            }
            else
            {
                MessageBox("Ошибка");
            }

            LoginSettingsLoginTextBox.IsReadOnly = false;
        }

        private async void LoginSettingsLogin_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (Regex.IsMatch(textBox.Text, Properties.Resources.RegexLoginPattern))
                {
                    if (textBox.Text.Equals(LoginedUser.Login, StringComparison.OrdinalIgnoreCase))
                    {
                        IsLoginVerifiedForLoginSettings = true;
                        return;
                    }

                    var isLoginExist = false;
                    if (!await CheckConnection(async () => isLoginExist = await _client.LoginExistAsync(textBox.Text)))
                    {
                        return;
                    }

                    IsLoginVerifiedForLoginSettings = !isLoginExist;
                }
                else
                {
                    IsLoginVerifiedForLoginSettings = false;
                }
            }
        }

        #endregion

        #endregion

        #region privacy dialog

        private void PrivacySettingsOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog.DataContext = this;
            IsMainSettingsOpen = false;
            IsSettingsPrivacyOpen = true;
            SettingsHeader = "Конфиденциальность";
        }

        private void BlockedUsersDialogOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            IsBlackListOpen = true;
        }

        #region change password dialog

        private void PasswordChangeDialogOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            IsSettingsPasswordOpen = true;
        }

        private void ChangePasswordDialogClose_Button_Click(object sender, RoutedEventArgs e)
        {
            IsSettingsPasswordOpen = false;
        }

        private async void ChangePasswordSaveNewPassword_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsPasswordNewPasswordBox.Password.Equals(SettingsPasswordConfirmNewPasswordBox.Password))
            {
                var isPasswordChanged = false;
                if (!await CheckConnection(async () =>
                    isPasswordChanged = await _client.ChangePasswordAsync(SettingsPasswordNewPasswordBox.Password,
                        SettingsPasswordCurrentPasswordBox.Password)))
                {
                    return;
                }

                if (isPasswordChanged)
                {
                    MessageBox("Пароль успешно изменен");
                    IsSettingsPasswordOpen = false;
                }
                else
                {
                    MessageBox("Текущий пароль введен неверно");
                }
            }
        }

        private void ChangePasswordCurrentPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Regex.IsMatch(SettingsPasswordCurrentPasswordBox.Password, Properties.Resources.RegexPasswordPattern))
            {
                if (Regex.IsMatch(SettingsPasswordNewPasswordBox.Password, Properties.Resources.RegexPasswordPattern))
                {
                    if (Regex.IsMatch(SettingsPasswordConfirmNewPasswordBox.Password,
                        Properties.Resources.RegexPasswordPattern))
                    {
                        IsPasswordsVerifiedForPasswordSettings = true;
                        return;
                    }
                }
            }

            IsPasswordsVerifiedForPasswordSettings = false;
        }

        #endregion

        #endregion

        #region restore password dialog

        private void RestorePasswordOpen_TextBlock_Click(object sender, MouseButtonEventArgs e)
        {
            IsRestorePasswordOpen = true;
        }

        private void RestorePasswordDialogClose_Button_Click(object sender, RoutedEventArgs e)
        {
            RestorePasswordLoginTextBox.Text = string.Empty;
            IsRestorePasswordOpen = false;
        }

        private async void RestorePasswordSendEmailCode_Button_Click(object sender, RoutedEventArgs e)
        {
            var isCodeSended = false;
            if (!await CheckConnection(async () =>
                isCodeSended = await _client.SendCodeForRestorePasswordAsync(RestorePasswordLoginTextBox.Text)))
            {
                return;
            }

            if (isCodeSended)
            {
                RestorePasswordLoginTextBox.IsReadOnly = true;
                RestorePasswordEmailCodeTextBox.Visibility = Visibility.Visible;
                MessageBox("Код отправлен");
            }
            else
            {
                RestorePasswordLoginTextBox.IsReadOnly = false;
                MessageBox("Код не был отправлен\nПроверьте введенные данные");
            }
        }

        private async void RestorePasswordSave_Button_Click(object sender, RoutedEventArgs e)
        {
            if (RestorePasswordNewPasswordBox.Password.Equals(RestorePasswordConfirmNewPasswordBox.Password))
            {
                RestorePasswordNewPasswordBox.IsEnabled = false;
                RestorePasswordConfirmNewPasswordBox.IsEnabled = false;
                RestorePasswordEmailCodeTextBox.IsReadOnly = true;

                var isPasswordChanged = false;
                if (!await CheckConnection(async () => isPasswordChanged = await _client.RestorePasswordAsync(
                    RestorePasswordLoginTextBox.Text,
                    RestorePasswordEmailCodeTextBox.Text,
                    RestorePasswordNewPasswordBox.Password)))
                {
                    return;
                }

                if (isPasswordChanged)
                {
                    MessageBox("Пароль успешно изменен");
                    IsRestorePasswordOpen = false;
                }
                else
                {
                    MessageBox("Код введен не верно");
                }

                RestorePasswordNewPasswordBox.IsEnabled = true;
                RestorePasswordConfirmNewPasswordBox.IsEnabled = true;
                RestorePasswordEmailCodeTextBox.IsReadOnly = false;
            }
            else
            {
                MessageBox("Пароли не совпадают");
            }
        }

        private void RestorePasswordNewPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Regex.IsMatch(RestorePasswordNewPasswordBox.Password, Properties.Resources.RegexPasswordPattern))
            {
                if (Regex.IsMatch(RestorePasswordConfirmNewPasswordBox.Password,
                    Properties.Resources.RegexPasswordPattern))
                {
                    IsPasswordsVerifiedForRestorePasword = true;
                    return;
                }
            }

            IsPasswordsVerifiedForRestorePasword = false;
        }

        #endregion

        #region create group dialog

        private void CreateGroupClose_Button_Click(object sender, RoutedEventArgs e)
        {
            IsCreateGroupDialogOpen = false;
        }

        private async void CreateGroup_Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewGroupNameTextBox.Text))
            {
                NewGroupNameTextBox.Focus();
                return;
            }

            if (_contactItems.All(contactItem => !contactItem.IsSelectedContact))
            {
                MessageBox("Выберите хотя бы одного друга");
                return;
            }

            IsLoadingOpen = true;

            await CheckConnection(async () => await _client.CreateGroupAsync(
                _contactItems.Where(cItem => cItem.IsSelectedContact).Select(cItem2 => cItem2.Contact).ToArray(),
                NewGroupNameTextBox.Text));

            IsLoadingOpen = false;
            IsCreateGroupDialogOpen = false;
        }

        private void CreateGroupOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            IsCreateGroupDialogOpen = true;
            SideMenuDrawerHost.IsLeftDrawerOpen = false;
        }

        #endregion

        #region blacklist

        private async void BlackListAddOrRemove_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            GroupInfoSubMenu.IsPopupOpen = false;
            UserBaseWCF user = null;
            if (sender is MenuItem menuItem)
            {
                if (menuItem.DataContext is MessageItem messageItem &&
                    !messageItem.Message.Sender.Id.Equals(LoginedUser.Id))
                {
                    user = messageItem.Message.Sender;
                }
                else if (menuItem.DataContext is GroupInfoItem groupInfoItem)
                {
                    user = groupInfoItem.User;
                }
            }

            if (user != null)
            {
                if (_blackListItems.Any(blocked => blocked.User.Id.Equals(user.Id)))
                {
                    await CheckConnection(async () => await _client.RemoveFromBlackListAsync(user));
                }
                else
                {
                    await CheckConnection(async () => await _client.AddToBlackListAsync(user));
                }
            }
        }

        private async void BlackListRemoveUser_TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender is TextBlock textBlock && textBlock.DataContext is BlackListItem blocked)
            {
                await CheckConnection(async () => await _client.RemoveFromBlackListAsync(blocked.User));
            }
        }

        private void BlackListItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var messageItem in _messageItems)
            {
                messageItem.IsUserBlocked = _blackListItems.Any(blackListItem =>
                    blackListItem.User.Id.Equals(messageItem.Message.Sender.Id));
            }

            if (GroupInfoGrid.DataContext is GroupInfoItem userItem)
            {
                var group = _groupItems.SingleOrDefault(gItem => gItem.IsSelectedGroup);
                if (group != null)
                {
                    if (group.Group.Type.Equals(GroupType.SingleUser) &&
                        group.Group.Users.SingleOrDefault(user => user.Id != LoginedUser.Id) is UserBaseWCF userBase &&
                        userBase.Id.Equals(userItem.User.Id))
                    {
                        userItem.IsUserBlockedInfo = _blackListItems.Any(blackListItem =>
                            blackListItem.User.Id.Equals(userBase.Id));
                    }
                }
            }

            CollectionViewSource
                .GetDefaultView(_messageItems).Refresh();
        }

        #endregion
    }
}