using System;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Client.ServiceReference;

namespace Client
{
    public partial class MainWindow
    {
        private const int Plus7 = 7;

        public static readonly DependencyProperty LoginedUserProperty =
            DependencyProperty.Register("LoginedUser", typeof(UserWCF), typeof(MainWindow),
                new PropertyMetadata(new UserWCF()));

        public static readonly DependencyProperty SelectedGroupIdProperty =
            DependencyProperty.Register("SelectedGroupId", typeof(int), typeof(MainWindow), new PropertyMetadata(-1));

        public static readonly DependencyProperty LoginedUserImageProperty =
            DependencyProperty.Register("LoginedUserImage", typeof(BitmapSource), typeof(MainWindow),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsSettingsOpenProperty =
            DependencyProperty.Register("IsSettingsOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsEmailSettingsOpenProperty =
            DependencyProperty.Register("IsEmailSettingsOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsProfileSettingsOpenProperty =
            DependencyProperty.Register("IsProfileSettingsOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsMainSettingsOpenProperty =
            DependencyProperty.Register("IsMainSettingsOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(true));

        public static readonly DependencyProperty SettingsHeaderProperty =
            DependencyProperty.Register("SettingsHeader", typeof(string), typeof(MainWindow),
                new PropertyMetadata("Настройки"));

        public static readonly DependencyProperty IsEmailVerifiedForEmailSettingsProperty =
            DependencyProperty.Register("IsEmailVerifiedForEmailSettings", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsPasswordVerifiedForEmailSettingsProperty =
            DependencyProperty.Register("IsPasswordVerifiedForEmailSettings", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsLoginVerifiedForLoginSettingsProperty =
            DependencyProperty.Register("IsLoginVerifiedForLoginSettings", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsSettingsLoginOpenProperty =
            DependencyProperty.Register("IsSettingsLoginOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsSettingsPrivacyOpenProperty =
            DependencyProperty.Register("IsSettingsPrivacyOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsSettingsPasswordOpenProperty =
            DependencyProperty.Register("IsSettingsPasswordOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsPasswordsVerifiedForPasswordSettingsProperty =
            DependencyProperty.Register("IsPasswordsVerifiedForPasswordSettings", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsLoadingOpenProperty =
            DependencyProperty.Register("IsLoadingOpen", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty IsMessageBoxOpenProperty =
            DependencyProperty.Register("IsMessageBoxOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsRestorePasswordOpenProperty =
            DependencyProperty.Register("IsRestorePasswordOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsPasswordsVerifiedForRestorePaswordProperty =
            DependencyProperty.Register("IsPasswordsVerifiedForRestorePasword", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsContactsDialogOpenProperty =
            DependencyProperty.Register("IsContactsDialogOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsCreateGroupDialogOpenProperty =
            DependencyProperty.Register("IsCreateGroupDialogOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsAddContactsToGroupDialogOpenProperty =
            DependencyProperty.Register("IsAddContactsToGroupDialogOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsBlackListOpenProperty =
            DependencyProperty.Register("IsBlackListOpen", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false));

        private readonly ObservableCollection<BlackListItem> _blackListItems;
        private readonly ObservableCollection<ContactItem> _contactItems;
        private readonly BitmapSource _defaultImage;
        private readonly HoldMouseButton _holdMouse;

        private readonly ObservableCollection<SavedFileInfo> _savedFiles;
        private readonly ObservableCollection<GroupItem> _searchItems;

        private ObservableCollection<UserImageItem> _avatars;

        private CeadChatServiceClient _client;
        private ObservableCollection<GroupItem> _groupItems;

        private bool _haveInternetConnection;
        private ObservableCollection<MessageItem> _messageItems;


        public MainWindow()
        {
            InitializeComponent();

            MaxWidth = SystemParameters.WorkArea.Width + Plus7;
            MaxHeight = SystemParameters.WorkArea.Height + Plus7;

            _blackListItems = new ObservableCollection<BlackListItem>();
            _blackListItems.CollectionChanged += BlackListItems_CollectionChanged;

            _contactItems = new ObservableCollection<ContactItem>();
            _contactItems.CollectionChanged += ContactItems_CollectionChanged;

            try
            {
                _client = new CeadChatServiceClient(new InstanceContext(this));
                _client.Endpoint.Binding.SendTimeout = new TimeSpan(1, 0, 0);
                _client.Open();
            }
            catch
            {
                CommunicationErrorDialog.IsActive = true;
            }


            _savedFiles = DeserializeSavedFiles();
            _savedFiles.CollectionChanged += SavedFiles_CollectionChanged;
            _searchItems = new ObservableCollection<GroupItem>();

            SelectedGroupId = -1;

            SideMenuDrawerHost.DataContext = this;
            MessagesGrid.DataContext = this;

            SettingsDialog.Tag = IsSettingsOpenProperty;
            SettingsNewEmailDialog.Tag = IsEmailSettingsOpenProperty;
            SettingsLoginDialog.Tag = IsSettingsLoginOpenProperty;
            RestorePasswordDialog.Tag = IsRestorePasswordOpenProperty;
            ContactsDialog.Tag = IsContactsDialogOpenProperty;
            CreateGroupDialog.Tag = IsCreateGroupDialogOpenProperty;
            AddContactsToGroupDialog.Tag = IsAddContactsToGroupDialogOpenProperty;
            BlackListDialog.Tag = IsBlackListOpenProperty;
            SettingsChangePasswordDialog.Tag = IsSettingsPasswordOpenProperty;

            LoadingDialog.DataContext = this;
            MessageDialog.DataContext = this;
            RestorePasswordDialog.DataContext = this;

            _defaultImage =
                GetBitmapFromFile(new FileInfo($"{Properties.Resources.UserImagesPath}\\png\\Zireael_logo.64.png")
                    .FullName);

            ShowGrid(LoginGrid);
            _holdMouse = new HoldMouseButton();
        }

        public bool IsBlackListOpen
        {
            get => (bool) GetValue(IsBlackListOpenProperty);
            set => SetValue(IsBlackListOpenProperty, value);
        }

        public bool IsContactsDialogOpen
        {
            get => (bool) GetValue(IsContactsDialogOpenProperty);
            set
            {
                SetValue(IsContactsDialogOpenProperty, value);
                if (value.Equals(false))
                {
                    ContactLoginTextBox.Text = string.Empty;
                }
            }
        }

        public bool IsCreateGroupDialogOpen
        {
            get => (bool) GetValue(IsCreateGroupDialogOpenProperty);
            set
            {
                SetValue(IsCreateGroupDialogOpenProperty, value);
                if (value.Equals(false))
                {
                    NewGroupDescriptionTextBox.Text = string.Empty;
                    NewGroupNameTextBox.Text = string.Empty;
                    foreach (var contactItem in _contactItems)
                    {
                        contactItem.IsSelectedContact = false;
                        contactItem.IsDisabledContact = false;
                    }
                }
            }
        }

        public bool IsAddContactsToGroupDialogOpen
        {
            get => (bool) GetValue(IsAddContactsToGroupDialogOpenProperty);
            set
            {
                SetValue(IsAddContactsToGroupDialogOpenProperty, value);
                if (value.Equals(false))
                {
                    foreach (var contactItem in _contactItems)
                    {
                        contactItem.IsSelectedContact = false;
                        contactItem.IsDisabledContact = false;
                    }
                }
            }
        }

        public bool IsPasswordsVerifiedForRestorePasword
        {
            get => (bool) GetValue(IsPasswordsVerifiedForRestorePaswordProperty);
            set => SetValue(IsPasswordsVerifiedForRestorePaswordProperty, value);
        }

        public bool IsRestorePasswordOpen
        {
            get => (bool) GetValue(IsRestorePasswordOpenProperty);
            set
            {
                SetValue(IsRestorePasswordOpenProperty, value);
                if (value.Equals(false))
                {
                    RestorePasswordLoginTextBox.IsReadOnly = false;
                    RestorePasswordNewPasswordBox.IsEnabled = true;
                    RestorePasswordConfirmNewPasswordBox.IsEnabled = true;
                    RestorePasswordEmailCodeTextBox.IsReadOnly = false;

                    RestorePasswordNewPasswordBox.Password = string.Empty;
                    RestorePasswordConfirmNewPasswordBox.Password = string.Empty;
                    RestorePasswordEmailCodeTextBox.Text = string.Empty;
                    RestorePasswordLoginTextBox.Text = string.Empty;

                    RestorePasswordEmailCodeTextBox.Visibility = Visibility.Collapsed;
                }
            }
        }

        public bool IsMessageBoxOpen
        {
            get => (bool) GetValue(IsMessageBoxOpenProperty);
            set => SetValue(IsMessageBoxOpenProperty, value);
        }

        public bool IsLoadingOpen
        {
            get => (bool) GetValue(IsLoadingOpenProperty);
            set => SetValue(IsLoadingOpenProperty, value);
        }

        public bool IsPasswordsVerifiedForPasswordSettings
        {
            get => (bool) GetValue(IsPasswordsVerifiedForPasswordSettingsProperty);
            set => SetValue(IsPasswordsVerifiedForPasswordSettingsProperty, value);
        }

        public bool IsSettingsPasswordOpen
        {
            get => (bool) GetValue(IsSettingsPasswordOpenProperty);
            set
            {
                SetValue(IsSettingsPasswordOpenProperty, value);
                if (value.Equals(false))
                {
                    SettingsPasswordCurrentPasswordBox.Password = string.Empty;
                    SettingsPasswordConfirmNewPasswordBox.Password = string.Empty;
                    SettingsPasswordNewPasswordBox.Password = string.Empty;

                    IsPasswordsVerifiedForPasswordSettings = false;
                }
            }
        }

        public bool IsSettingsPrivacyOpen
        {
            get => (bool) GetValue(IsSettingsPrivacyOpenProperty);
            set => SetValue(IsSettingsPrivacyOpenProperty, value);
        }

        public bool IsLoginVerifiedForLoginSettings
        {
            get => (bool) GetValue(IsLoginVerifiedForLoginSettingsProperty);
            set => SetValue(IsLoginVerifiedForLoginSettingsProperty, value);
        }

        public bool IsSettingsLoginOpen
        {
            get => (bool) GetValue(IsSettingsLoginOpenProperty);
            set => SetValue(IsSettingsLoginOpenProperty, value);
        }

        public bool IsEmailVerifiedForEmailSettings
        {
            get => (bool) GetValue(IsEmailVerifiedForEmailSettingsProperty);
            set => SetValue(IsEmailVerifiedForEmailSettingsProperty, value);
        }

        public bool IsPasswordVerifiedForEmailSettings
        {
            get => (bool) GetValue(IsPasswordVerifiedForEmailSettingsProperty);
            set => SetValue(IsPasswordVerifiedForEmailSettingsProperty, value);
        }

        public bool IsSettingsOpen
        {
            get => (bool) GetValue(IsSettingsOpenProperty);
            set
            {
                SetValue(IsSettingsOpenProperty, value);
                if (value.Equals(true))
                {
                    CloseAllInnerSettings();
                }
            }
        }

        public bool IsEmailSettingsOpen
        {
            get => (bool) GetValue(IsEmailSettingsOpenProperty);
            set
            {
                SetValue(IsEmailSettingsOpenProperty, value);
                if (value.Equals(false))
                {
                    IsPasswordVerifiedForEmailSettings = false;
                    IsEmailVerifiedForEmailSettings = false;
                    NewEmailTextBox.IsReadOnly = false;
                    NewEmailTextBox.Text = string.Empty;
                    NewEmailPasswordPasswordBox.Password = string.Empty;
                    SettingsEmailCodeTextBox.Text = string.Empty;
                    SettingsEmailCodeTextBox.Visibility = Visibility.Collapsed;
                }
            }
        }

        public bool IsProfileSettingsOpen
        {
            get => (bool) GetValue(IsProfileSettingsOpenProperty);
            set => SetValue(IsProfileSettingsOpenProperty, value);
        }

        public bool IsMainSettingsOpen
        {
            get => (bool) GetValue(IsMainSettingsOpenProperty);
            set => SetValue(IsMainSettingsOpenProperty, value);
        }

        public string SettingsHeader
        {
            get => (string) GetValue(SettingsHeaderProperty);
            set => SetValue(SettingsHeaderProperty, value);
        }

        public UserWCF LoginedUser
        {
            get => (UserWCF) GetValue(LoginedUserProperty);
            set => SetValue(LoginedUserProperty, value);
        }

        public int SelectedGroupId
        {
            get => (int) GetValue(SelectedGroupIdProperty);
            set => SetValue(SelectedGroupIdProperty, value);
        }

        public BitmapSource LoginedUserImage
        {
            get => (BitmapSource) GetValue(LoginedUserImageProperty);
            set => SetValue(LoginedUserImageProperty, value);
        }

        private void Blacklist_Cansel_Button_Click(object sender, RoutedEventArgs e)
        {
            IsBlackListOpen = false;
        }
    }
}