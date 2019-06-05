using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using System.Timers;
using System.Configuration;
using Liphsoft.Crypto.Argon2;

//                bool saveFailed;
//                do
//                {
//                    saveFailed = false;
//                    try
//                    {
//                        context.SaveChanges();
//                    }
//                    catch (DbUpdateException ex)
//                    {
//                        saveFailed = true;
//                    }
//                } while (saveFailed);


namespace Wcf_CeadChat_ServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]

    public class CeadChatService : ICeadChatService
    {
        private Dictionary<IUserChanged, User> _onlineUsers;//онлайн пользователи
        private Timer _timer;
        private delegate object ExceptionDelegate();
        private static Random _random = new Random((int)DateTime.Now.Ticks);

        private string _mailLogin = ConfigurationManager.AppSettings.Get("CorporateMail");
        private string _mailPassword = ConfigurationManager.AppSettings.Get("CorporateMailPass");
        private string _mailHost = ConfigurationManager.AppSettings.Get("CorporateMailHost");
        private string _salt = ConfigurationManager.AppSettings.Get("PasswordSalt");
        private int _countHoursWorkingRecoveryCode;
        private int _portForEmail;
        private int _lenghtRecoveryCode;
        private int _checkTimeOnline;
        private int _tokenLifetime;//количество дней

        public CeadChatService()
        {
            var str = ConfigurationManager.AppSettings.Get("CheckTimeOnline");
            _checkTimeOnline = int.Parse(str);
            _lenghtRecoveryCode = int.Parse(ConfigurationManager.AppSettings.Get("LenghtRecoveryCode"));
            _countHoursWorkingRecoveryCode = int.Parse(ConfigurationManager.AppSettings.Get("CountHoursWorkingRecoveryCode"));
            _portForEmail = int.Parse(ConfigurationManager.AppSettings.Get("PortForEmail"));
            _tokenLifetime = int.Parse(ConfigurationManager.AppSettings.Get("TokenLifetime"));
            _onlineUsers = new Dictionary<IUserChanged, User>();
            _timer = new Timer();
            _timer.Interval = _checkTimeOnline;
            _timer.Elapsed += Timer_Elapsed;
            _timer.Enabled = true;
            _timer.Start();
        }

        #region system

        ChatContext Context(IUserChanged userChanged)
        {
            try
            {

                ChatContext context = null;
                if (_onlineUsers.FirstOrDefault(o => o.Key == userChanged).Value != null)//получаем учетку по текущему подключению, для проверки может ли юзер выполнять команды
                {
                    context = new ChatContext();
                    context.Database.Log += WriteLog;
                }
                return context;
            }
            catch (Exception ex)
            {
                WriteLog("ERROR " + ex.Message);
                return null;
            }
            //((IContextChannel)context).OperationTimeout = new TimeSpan(0, 3, 0);
            //var users = context.Users.ToList();
            //foreach (var item in users)
            //{
            //    context.Entry(item).Collection("Groups").Load();
            //    //context.Entry(item).Collection("Friends").Load();
            //}
            //var group = context.Groups.ToList();
            //foreach (var item in group)
            //{
            //    context.Entry(item).Collection("Messages").Load();
            //    context.Entry(item).Reference("LastMessage").Load();
            //}
            //var messages = context.Messages.ToList();
            //foreach (var item in messages)
            //{
            //    context.Entry(item).Reference("Sender").Load();
            //    if (item is MessageFile)
            //    {
            //        context.Entry(item).Reference("File").Load();
            //}

        }//получения нового контекста 
        User GetCurrentUser()
        {
            var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
            var uc = _onlineUsers.FirstOrDefault(o => o.Key == userChanged);
            return uc.Value;//получаем учетку по текущему подключению
        }
    
        private string GetConnectionId(IUserChanged user, string sessionId)
        {
            var context = new ChatContext();
            var sessions = context.Sessions.ToList().Where(s => s.User.Id.Equals(_onlineUsers[user].Id));
            var session = sessions.FirstOrDefault(s => s.SessionId.Equals(sessionId))?.ConnectionId;
            return session;
        }

        public UserWCF Connect(string sessionId, string connectionId)
        {
            var result = TryExecute(() =>
            {
                var context = new ChatContext();
                var session = context.Sessions.FirstOrDefault(s => s.SessionId.Equals(sessionId, StringComparison.OrdinalIgnoreCase));
                session.ConnectionId = connectionId;
                context.SaveChanges();
                return new UserWCF(session.User);
            }, true);
            if (result is UserWCF userWcf)
            {
                return userWcf;
            }
            return null;
        }
        private string GeneratePasswordHash(string password)
        {
            //var hasherConfig = new Argon2Config
            //{
            //    MemoryCost = 8192,
            //    Lanes = 1,
            //    Salt = Encoding.UTF8.GetBytes(_salt),
            //    Password = Encoding.UTF8.GetBytes(password)
            //};

            //using (var hashA = new Argon2(hasherConfig).Hash())
            //{
            //    var passwordHash = hasherConfig.EncodeString(hashA.Buffer);
            //    return passwordHash.Remove(passwordHash.Length - 1);
            //}
            return new PasswordHasher().Hash(password, _salt);
        }

        private string CreateToken(UserBase user)
        {
            var str = $"{DateTime.Now}{user.Login}{RandomString(_tokenLifetime)}";
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                var builder = new StringBuilder();
                foreach (var b in provider.ComputeHash(Encoding.UTF8.GetBytes(str)))
                {
                    builder.Append(b.ToString("x2").ToLower());
                }
                return builder.ToString();
            }
        }
        private object TryExecute(ExceptionDelegate action, bool returnNull = false)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}" + " ERROR: " + ex.Message);
                if (returnNull)
                {
                    return null;
                }
                return false;
            }
        }
        //private int GetDay(DateTime dateTime)
        //{
        //    return dateTime.TimeOfDay.Days;
        //}
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var list = new List<KeyValuePair<IUserChanged, User>>();
                ChatContext context = new ChatContext();
                context.Database.Log += WriteLog;
                foreach (var item in _onlineUsers)
                {
                    if (item.Key != null && item.Value != null)
                    {
                        try
                        {
                            item.Key.IsOnlineCallback(GetConnectionId(item.Key, item.Value.SessionId));
                        }
                        catch
                        {
                            var senderOnline = _onlineUsers.FirstOrDefault(o => o.Key == item.Key).Value;//получаем учетку по текущему подключению
                            NotificationAboutChangeOnlineStatus(senderOnline);
                            list.Add(item);
                        }
                    }
                }
                foreach (var item in list)
                {
                    _onlineUsers.Remove(item.Key);
                    context = new ChatContext();
                    context.Database.Log += WriteLog;
                    var senderOnline = _onlineUsers.FirstOrDefault(o => o.Value.Id == item.Value.Id).Value;//получаем учетку по текущему подключению
                    senderOnline = context.Users.FirstOrDefault(u => u.Id == senderOnline.Id);
                    senderOnline.IsOnline = false;
                    foreach (var item3 in _onlineUsers)
                    {
                        if (item3.Value.Id == senderOnline.Id)
                        {
                            senderOnline.IsOnline = true;
                        }
                    }
                    context.SaveChanges();
                    NotificationAboutChangeOnlineStatus(senderOnline);
                }
                foreach (var item in context.Users.ToList().Where(u => u.IsOnline))
                {
                    if (_onlineUsers.All(u => u.Value.Id != item.Id))
                    {
                        item.IsOnline = false;
                        context.SaveChanges();
                        NotificationAboutChangeOnlineStatus(item);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("ERROR " + ex.Message);
            }
        }

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        private void CallUsersInGroup(ICollection<User> usersCollection, Action<IUserChanged> action)
        {
            foreach (var item in usersCollection.ToList())
            {
                var users = _onlineUsers.Where(o => o.Value.Id == item.Id);
                foreach (var user in users)
                {
                    if (user.Key != null || user.Value != null)
                    {
                        try
                        {
                            action.Invoke(user.Key);
                        }
                        catch(Exception e)
                        {
                            ChatContext context = new ChatContext();
                            context.Database.Log += WriteLog;
                            var us = context.Users.FirstOrDefault(u => u.Id == user.Value.Id);
                            us.IsOnline = false;
                            context.SaveChanges();
                            _onlineUsers.Remove(user.Key);
                            NotificationAboutChangeOnlineStatus(user.Value);
                        }
                    }
                }
            }
        }

        private static void WriteLog(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }//записать лог на хост
        #endregion

        #region messages
        public IEnumerable<MessageWCF> GetMessagesFromGroup(GroupWCF group)
        {
            var result = TryExecute(() =>
            {
                var sender = GetCurrentUser();
                var messages = new List<MessageWCF>();
                foreach (var message in Messenger.GetMessagesFromGroup(group.Id, sender.Id))
                {
                    if (message is MessageFile)
                    {
                        messages.Add(new MessageFileWCF((MessageFile)message));
                    }
                    else
                    {
                        messages.Add(new MessageWCF(message));
                    }
                }
                return messages;
            });
            if (result is IEnumerable<MessageWCF>)
            {
                return result as IEnumerable<MessageWCF>;
            }
            return null;
        }//получить сообщения из группы
        public int SendMessage(MessageWCF message, long hash)
        {
            var result = TryExecute(() =>
                        {
                            Group group = new Group();
                            Message msg = null;
                            bool isSend = false;

                            message.DateTime = DateTime.Now;
                            var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                            var s = OperationContext.Current.SessionId;
                            var sender = _onlineUsers.FirstOrDefault(o => o.Key == userChanged).Value;//получаем учетку по текущему подключению
                            sender = GetCurrentUser();
                            group = Messenger.GetGroupById(message.GroupId, sender.Id);//получаем группу с контекста по id в сообщении\
                            if(group is null)
                            {
                                return null;
                            }
                            msg = Messenger.SendMessage(message, sender.Id);
                            if (msg != null)
                            {
                                isSend = true;
                            }
                            if (isSend)
                            {
                                CallUsersInGroup(group.Users, (user) =>
                              {
                                  if (!(message is MessageFileWCF))
                                  {
                                      user.CreateMessageCallback(new MessageWCF(msg), hash, GetConnectionId(user, _onlineUsers[user].SessionId));//передаем сообщение всем пользователям которые онлайн(в этой группе)
                                      user.NewLastMessageCallback(new MessageWCF(msg), GetConnectionId(user, _onlineUsers[user].SessionId));
                                  }
                              });
                            }
                            return msg.Id;
                        });
            if (result is int)
            {
                return (int)result;
            }
            return -1;
        }//отправить сообщение
         //private Exception HandleDbUpdateException(DbUpdateException dbu)
         //{
         //    var builder = new StringBuilder("A DbUpdateException was caught while saving changes. ");

        //    try
        //    {
        //        foreach (var result in dbu.Entries)
        //        {
        //            builder.AppendFormat("Type: {0} was part of the problem. ", result.Entity.GetType().Name);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        builder.Append("Error parsing DbUpdateException: " + e.ToString());
        //    }

        //    string message = builder.ToString();
        //    return new Exception(message, dbu);
        //}
        public bool DeleteMessage(MessageWCF message)
        {
            var result = TryExecute(() =>
            {
                Group group = null;
                Message newLastMessage = null, messageFromContex = null;
                var sender = GetCurrentUser();//получаем учетку по текущему подключению
                messageFromContex = Messenger.DeleteMessage(message, sender.Id);
                group = Messenger.GetGroupById(message.GroupId, sender.Id);
                CallUsersInGroup(group.Users, (user) =>
                {
                    user.DeleteMessageCallback(new MessageWCF(messageFromContex), GetConnectionId(user, _onlineUsers[user].SessionId));//передаем сообщение всем пользователям которые онлайн(в этой группе)
                });
                if (group.LastMessage.Id == message.Id)
                {
                    newLastMessage = Messenger.GetLastMessage(group.Id);
                    CallUsersInGroup(group.Users, (user) =>
                    {
                        user.NewLastMessageCallback(new MessageWCF(newLastMessage), GetConnectionId(user, _onlineUsers[user].SessionId));//передаем новое последнее сообщение всем пользователям которые онлайн(в этой группе)
                    });
                }
                return true;
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }//удалить сообщение
        public Package GetPackageFromFile(int messageId, int packNumber)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                var sender = GetCurrentUser();
                var pack = Messenger.GetPackageFromFile(messageId, packNumber, sender.Id);
                userChanged.SendedPackageCallback(messageId, pack.Number);
                return pack;
            });
            if (result is Package)
            {
                return result as Package;
            }
            return null;
        }//получить данные с файла(в сообщеии)
        public bool SendPackageToFile(int messageId, Package package)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                var sender = GetCurrentUser();
                MessageFile message = Messenger.SendPackageToFile(messageId, package, sender.Id);
                userChanged.SendedPackageCallback(messageId, package.Number);

                if (message.File.CountPackages == message.File.CountReadyPackages)
                {
                    var group = Messenger.GetGroupById(message.Group.Id, sender.Id);
                    Messenger.SetLastMessage(group.Id, message.Id, sender.Id);
                    foreach (var item in group.Users.ToList())
                    {
                        var users = _onlineUsers.Where(o => o.Value.Id == item.Id);
                        CallUsersInGroup(group.Users, (user) =>
                        {
                            Messenger.SetVisible(message.Id, true, sender.Id);
                            user.CreateMessageCallback(new MessageFileWCF(message), messageId, GetConnectionId(user, _onlineUsers[user].SessionId));//передаем новое последнее сообщение всем пользователям которые онлайн(в этой группе)
                            user.NewLastMessageCallback(new MessageFileWCF(message), GetConnectionId(user, _onlineUsers[user].SessionId));
                        });
                    }
                }
                return true;
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        public Dictionary<int, int> GetDontReadMessagesFromGroups(IEnumerable<int> groupsId)
        {
            var result = TryExecute(() =>
            {
                var sender = GetCurrentUser();
                if (sender != null)
                {
                    return Messenger.GetDontReadMessagesFromGroups(groupsId, sender.Id);
                }
                else
                {
                    return null;
                }
            });
            if (result is Dictionary<int, int>)
            {
                return result as Dictionary<int, int>;
            }
            return null;
        }//получить количество не прочитанных сообщений со всех групп
        public bool ReadAllMessagesInGroup(int groupId)//прочитать все сообщения в группе
        {
            var result = TryExecute(() =>
            {
                var sender = GetCurrentUser();
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение
                var group = Messenger.ReadAllMessagesInGroup(groupId, sender.Id);
                CallUsersInGroup(group.Users, (user) =>
                {
                    user.ReadedMessagesCallback(new GroupWCF(group), new UserBaseWCF(sender), GetConnectionId(user, _onlineUsers[user].SessionId));
                });
                return true;
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        public bool ReadAllMessagesInAllGroups()//прочитать все сообщения во всех группах для отправляющего
        {
            var result = TryExecute(() =>
            {
                var sender = GetCurrentUser();
                foreach (var group in Messenger.ReadAllMessagesInAllGroups(sender.Id).ToList())
                {
                    CallUsersInGroup(group.Users, (user) =>
                    {
                        user.ReadedMessagesCallback(new GroupWCF(group), new UserBaseWCF(sender), GetConnectionId(user, _onlineUsers[user].SessionId));
                    });
                }
                return true;
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        public IEnumerable<MessageWCF> GetMessagesBetween(int groupId, int startIdx, int count)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var messages = new List<MessageWCF>();
                    var sender = _onlineUsers.FirstOrDefault(u => u.Key == userChanged).Value;
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);

                    if(!context.Groups.ToList().Any(g => g.Id == groupId && g.Users.Any(u => u.Id == sender.Id)))
                    {
                        return null;
                    }

                    var selectedMessages = context.Messages.ToList().Where(m => m.Group.Id == groupId && m.Group.Users.Contains(sender) && m.IsVisible)
                                                           .Skip(startIdx)
                                                           .Take(count);
                    foreach (var item in selectedMessages)
                    {
                        messages.Add(new MessageWCF(item));
                    }
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    context.SaveChanges();
                    return messages;
                }
                else
                {
                    return null;
                }
            });
            if (result is IEnumerable<MessageWCF>)
            {
                return result as IEnumerable<MessageWCF>;
            }
            return null;
        }

        public bool ChangeTextInMessage(MessageWCF message)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var messages = new List<MessageWCF>();
                    var sender = _onlineUsers.FirstOrDefault(u => u.Key == userChanged).Value;
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    var selectedMessage = context.Messages
                                                 .FirstOrDefault(m => m.Id == message.Id
                                                                   && m.Group.Id == message.GroupId
                                                                   && m.Group.Users.Contains(sender));
                    if (selectedMessage.Text != message.Text)
                    {
                        selectedMessage.Text = message.Text;
                        selectedMessage.IsChanged = true;
                    }
                    CallUsersInGroup(selectedMessage.Group.Users, (user) =>
                    {
                        user.ChangeTextInMessageCallback(new MessageWCF(selectedMessage), GetConnectionId(user, _onlineUsers[user].SessionId));
                    });
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    context.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }

        #endregion

        #region chat
        public bool CreateGroup(IEnumerable<UserBaseWCF> newUsers, string nameGroup)
        {
            var result = TryExecute(() =>
            {
                bool saveFailed;
                Group group = null;
                Message systemMessage = null;
                ChatContext context = null;
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение
                context = Context(userChanged);
                if (context != null)
                {
                    do
                    {
                        saveFailed = false;
                        try
                        {
                            context = Context(userChanged);
                            //желательно не менять порядок действий и не трогать SaveChanges() - будет падать
                            group = context.Groups.Add(new Group { Type = GroupType.MultyUser, Name = nameGroup, });
                            var system = context.Users.SingleOrDefault(sys => sys.Login.Equals("system", StringComparison.OrdinalIgnoreCase));
                            systemMessage = new Message
                            {
                                DateTime = DateTime.Now,
                                Group = group,
                                IsRead = true,
                                IsVisible = true,
                                Sender = system,
                                Text = "создана группа"
                            };
                            context.Messages.Add(systemMessage);
                            context.SaveChanges();
                        }
                        catch (DbUpdateException ex)
                        {
                            saveFailed = true;
                        }
                    } while (saveFailed);

                    group.LastMessage = systemMessage;

                    User creator = _onlineUsers.FirstOrDefault(u => u.Key == userChanged).Value;//создатель группы
                    var sender = context.Users.FirstOrDefault(u => u.Id == creator.Id);
                    group.Creator = sender;
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    group.Users.Add(context.Users.SingleOrDefault(us => us.Id == sender.Id));
                    context.SaveChanges();

                    AddFriendsToGroup(new GroupWCF(group), newUsers);
                    group = Context(userChanged).Groups.FirstOrDefault(g => g.Id == group.Id);
                    // что бы коллбек пришел всем пользователям а не только создателю

                    CallUsersInGroup(group.Users, (user) =>
                         {
                             user.CreateChatCallback(new GroupWCF(group), sender.Id, context.Sessions.ToList().FirstOrDefault(s => s.User.Id.Equals(_onlineUsers[user].Id))?.ConnectionId);
                         });

                    return true;
                }
                return false;
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }//создание группы(с множеством учасников)

        public bool CreateChat(UserBaseWCF addingUser)
        {
            var result = TryExecute(() =>
            {
                bool saveFailed;
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(o => o.Key == userChanged).Value;//получаем учетку по текущему подключению
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    do
                    {
                        saveFailed = false;
                        try
                        {
                            context = Context(userChanged);

                            var system = context.Users.SingleOrDefault(sys => sys.Login.Equals("system", StringComparison.OrdinalIgnoreCase));

                            var oldGroup = context.Groups.FirstOrDefault(g => g.Type == GroupType.SingleUser
                            && g.Users.FirstOrDefault(u => u.Id == sender.Id) != null
                            && g.Users.FirstOrDefault(u => u.Id == addingUser.Id) != null);//проверка на существование чата между пользователями
                            if (oldGroup == null)
                            {
                                Group group = context.Groups.Add(new Group { Type = GroupType.SingleUser });

                                var systemMessage = new Message
                                {
                                    DateTime = DateTime.Now,
                                    Group = group,
                                    IsRead = true,
                                    IsVisible = true,
                                    Sender = system,
                                    Text = "создана группа"
                                };

                                context.Messages.Add(systemMessage);
                                context.SaveChanges();

                                group.LastMessage = systemMessage;
                                group.Users.Add(context.Users.SingleOrDefault(u => u.Id == sender.Id));
                                group.Users.Add(context.Users.SingleOrDefault(u => u.Id == addingUser.Id));

                                context.SaveChanges();
                                CallUsersInGroup(group.Users, (user) =>
                                {
                                    user.CreateChatCallback(new GroupWCF(group), sender.Id, Context(userChanged).Sessions.FirstOrDefault(s => s.User.Id.Equals(sender.Id))?.ConnectionId);
                                });
                            }
                            else
                            {
                                oldGroup.IsVisible = true;
                                var systemMessage = new Message
                                {
                                    DateTime = DateTime.Now,
                                    Group = oldGroup,
                                    IsRead = true,
                                    IsVisible = true,
                                    Sender = system,
                                    Text = "создана группа"
                                };
                                context.Messages.Add(systemMessage);
                                oldGroup.LastMessage = systemMessage;
                                context.SaveChanges();
                                CallUsersInGroup(oldGroup.Users, (user) =>
                                {
                                    user.CreateChatCallback(new GroupWCF(oldGroup), sender.Id, Context(userChanged).Sessions.FirstOrDefault(s => s.User.Id.Equals(sender.Id))?.ConnectionId);
                                });
                            }
                        }
                        catch (DbUpdateException ex)
                        {
                            saveFailed = true;
                        }
                    } while (saveFailed);
                    return true;
                }
                return false;
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }//создание чата с пользователем

        public bool RemoveGroup(GroupWCF group)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(o => o.Key == userChanged).Value;//получаем учетку по текущему подключению
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    var groupContext = context.Groups.FirstOrDefault(g => g.Id == group.Id);//получаем группу с контекста по id в сообщении

                    switch (groupContext.Type)
                    {
                        case GroupType.SingleUser:
                            {
                                foreach (var message in context.Messages.Where(msg => msg.Group.Id == group.Id))//временно
                                {
                                    message.IsVisible = false;//скрываем видимость
                                }

                                groupContext.IsVisible = false;
                                context.SaveChanges();
                                CallUsersInGroup(groupContext.Users, (user) =>
                                {
                                    user.RemoveGroupCallback(group, GetConnectionId(user, _onlineUsers[user].SessionId));
                                });
                                return true;
                            }
                        case GroupType.MultyUser:
                            {
                                if (groupContext.Creator.Id == sender.Id)
                                {
                                    foreach (var message in context.Messages.Where(msg => msg.Group.Id == group.Id))//временно
                                    {
                                        message.IsVisible = false;//скрываем видимость
                                    }

                                    groupContext.IsVisible = false;
                                    context.SaveChanges();
                                    CallUsersInGroup(groupContext.Users, (user) =>
                                    {
                                        user.RemoveGroupCallback(group, GetConnectionId(user, _onlineUsers[user].SessionId));
                                    });
                                }
                                return true;
                            }
                        case GroupType.Channel:
                            {
                                return true;
                            }
                        default:
                            return false;
                    }

                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }//удаление группы
        public bool AddFriendsToGroup(GroupWCF group, IEnumerable<UserBaseWCF> friends)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(o => o.Key == userChanged).Value;//получаем учетку по текущему подключению
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    var groupContext = context.Groups.FirstOrDefault(g => g.Id == group.Id);//получаем группу с контекста по id в сообщении
                    var system = context.Users.SingleOrDefault(sys => sys.Login.Equals("system", StringComparison.OrdinalIgnoreCase));

                    foreach (var item in friends)
                    {
                        var friendContext = context.Users.FirstOrDefault(u => u.Id == item.Id);//получаем учетку добавляемого пользователя
                        if (!groupContext.Users.Contains(friendContext))
                        {
                            groupContext.Users.Add(friendContext);// добавляем в групу пользователя
                            var systemMessage = new Message
                            {
                                DateTime = DateTime.Now,
                                Group = groupContext,
                                IsRead = true,
                                IsVisible = true,
                                Sender = system,
                                Text = $"{sender.DisplayName} добавил {friendContext.DisplayName}"
                            };
                            context.Messages.Add(systemMessage);
                            context.SaveChanges();
                            var list = new List<User>();
                            list.Add(friendContext);
                            CallUsersInGroup(list, (user) =>
                            {
                                user.CreateChatCallback(new GroupWCF(groupContext), sender.Id, Context(userChanged).Sessions.FirstOrDefault(s => s.User.Id.Equals(sender.Id))?.ConnectionId);
                            });
                            CallUsersInGroup(groupContext.Users.Where(g => g.Id != friendContext.Id).ToList(), (user) =>
                              {
                                  user.AddFriendToGroupCallback(new UserBaseWCF(friendContext), new GroupWCF(groupContext), GetConnectionId(user, _onlineUsers[user].SessionId));
                              });
                            CallUsersInGroup(groupContext.Users.Where(g => g.Id != friendContext.Id).ToList(), (user) =>
                            {
                                user.CreateMessageCallback(new MessageWCF(systemMessage), systemMessage.Id, GetConnectionId(user, _onlineUsers[user].SessionId));
                            });
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }//добавление списка пользователей в группу
        public bool RemoveOrExitFromGroup(int groupId, int userIdForRemove)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(o => o.Key == userChanged).Value;//получаем учетку по текущему подключению
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    Message systemMessage = null;
                    var system = context.Users.SingleOrDefault(sys => sys.Login.Equals("system", StringComparison.OrdinalIgnoreCase));
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    var groupContext = context.Groups.FirstOrDefault(g => g.Id == groupId);//получаем группу с контекста по id в сообщении
                    var userContext = context.Users.FirstOrDefault(u => u.Id == userIdForRemove);//получаем учетку удаляемого пользователя
                    if (sender.Id == groupContext.Creator?.Id)//если вызвал создатель группы
                    {
                        if (userContext != null && sender.Id != userContext.Id)//если создатель и удаляемый юзер разные пользователи
                        {
                            if (groupContext.Users.Contains(userContext))
                            {
                                groupContext.Users.Remove(userContext);
                                systemMessage = new Message
                                {
                                    DateTime = DateTime.Now,
                                    Group = groupContext,
                                    IsRead = true,
                                    IsVisible = true,
                                    Sender = system,
                                    Text = $"{sender.DisplayName} исключил {userContext.DisplayName}"
                                };
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (userContext == null || userContext.Id == sender.Id)//если создатель тот же юзер что и удаляемый
                        {
                            if (groupContext.Users.Contains(sender))
                            {
                                groupContext.Users.Remove(sender);
                                groupContext.Creator = null;
                                systemMessage = new Message
                                {
                                    DateTime = DateTime.Now,
                                    Group = groupContext,
                                    IsRead = true,
                                    IsVisible = true,
                                    Sender = system,
                                    Text = $"{sender.DisplayName} покинул группу"
                                };
                            }
                        }
                    }
                    else//если вызвал не создатель группы
                    {
                        if (groupContext.Users.Contains(sender))
                        {
                            groupContext.Users.Remove(sender);
                            systemMessage = new Message
                            {
                                DateTime = DateTime.Now,
                                Group = groupContext,
                                IsRead = true,
                                IsVisible = true,
                                Sender = system,
                                Text = $"{sender.DisplayName} покинул группу"
                            };
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (systemMessage != null)
                    {
                        context.Messages.Add(systemMessage);
                        context.SaveChanges();
                        CallUsersInGroup(groupContext.Users, (user) =>
                        {
                            user.RemoveOrExitUserFromGroupCallback(groupContext.Id, new UserBaseWCF(userContext), GetConnectionId(user, _onlineUsers[user].SessionId));
                        });
                        CallUsersInGroup(groupContext.Users, (user) =>
                        {
                            user.CreateMessageCallback(new MessageWCF(systemMessage), systemMessage.Id, GetConnectionId(user, _onlineUsers[user].SessionId));
                        });
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        #endregion

        #region friend && blackList
        public bool AddFriend(string friendLogin)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(o => o.Key == userChanged).Value;//получаем учетку по текущему подключению
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    var friend = context.Users.FirstOrDefault(u => u.Login.Equals(friendLogin, StringComparison.OrdinalIgnoreCase));
                    if (friend != null)
                    {
                        var senderContext = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                        //senderContext.Friends.Add(friend);//добавляем друга
                        context.Friends.Add(new Friends { Sender = senderContext, User2 = friend });
                        context.SaveChanges();// сохраняем изменения

                        var users = _onlineUsers.Where(o => o.Value.Id == sender.Id);
                        foreach (var user in users)
                        {
                            if (user.Key != null && user.Value != null)
                            {
                                try
                                {
                                    user.Key.AddContactCallback(new UserBaseWCF(friend), GetConnectionId(user.Key, user.Value.SessionId));
                                }
                                catch
                                {
                                    _onlineUsers.Remove(user.Key);
                                    NotificationAboutChangeOnlineStatus(user.Value);
                                }
                            }
                        }

                        return new UserBaseWCF(friend);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            });

            return result is UserBaseWCF;
        }//добавить пользователя в список друзей
        public bool RemoveFriend(UserBaseWCF friend)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(o => o.Key == userChanged).Value;//получаем учетку по текущему подключению
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    //var friendContext = sender.Friends.FirstOrDefault(f => f.Id == friend.Id);
                    //sender.Friends.Remove(friendContext);//удаляем друга
                    context.Friends.Remove(context.Friends.FirstOrDefault(uu => uu.Sender.Id == sender.Id && uu.User2.Id == friend.Id));
                    context.SaveChanges();// сохраняем изменения

                    var users = _onlineUsers.Where(o => o.Value.Id == sender.Id);
                    foreach (var user in users)
                    {
                        if (user.Key != null && user.Value != null)
                        {
                            try
                            {
                                user.Key.RemoveContactCallback(friend, GetConnectionId(user.Key, user.Value.SessionId));
                            }
                            catch
                            {
                                _onlineUsers.Remove(user.Key);
                                NotificationAboutChangeOnlineStatus(user.Value);
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }//удалить пользователя со списка друзей    

        public IEnumerable<UserBaseWCF> GetFriends()
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(u => u.Key == userChanged).Value;
                    var Uu = context.Friends.Where(u => u.Sender.Id == sender.Id).Select(uu => uu.User2).ToList();
                    List<UserBaseWCF> baseWCFs = new List<UserBaseWCF>();
                    foreach (var item in Uu)
                    {
                        baseWCFs.Add(new UserBaseWCF(item));
                    }
                    return baseWCFs;
                }
                else
                {
                    return null;
                }
            });
            return result as IEnumerable<UserBaseWCF>;
        }//получить список друзей

        public bool AddToBlackList(UserBaseWCF userForRemove)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(u => u.Key == userChanged).Value;
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    if (sender != null)
                    {
                        var blocked = context.Users.FirstOrDefault(u => u.Id == userForRemove.Id);
                        if (!context.BlackList.Any(bl => bl.Sender.Id == sender.Id && bl.Blocked.Id == blocked.Id))
                        {
                            context.BlackList.Add(new BlackList { Sender = sender, Blocked = blocked });
                            context.SaveChanges();
                        }

                        var users = _onlineUsers.Where(o => o.Value.Id == sender.Id);
                        foreach (var user in users)
                        {
                            if (user.Key != null && user.Value != null)
                            {
                                try
                                {
                                    user.Key.AddUserToBlackListCallback(new UserBaseWCF(blocked), GetConnectionId(user.Key, user.Value.SessionId));
                                }
                                catch
                                {
                                    _onlineUsers.Remove(user.Key);
                                    NotificationAboutChangeOnlineStatus(user.Value);
                                }
                            }
                        }
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }

        public bool RemoveFromBlackList(UserBaseWCF userIdForRemove)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(u => u.Key == userChanged).Value;
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    if (sender != null)
                    {
                        var blocked = context.Users.FirstOrDefault(u => u.Id == userIdForRemove.Id);
                        if (context.BlackList.Any(bl => bl.Sender.Id == sender.Id && bl.Blocked.Id == blocked.Id))
                        {
                            var blackList = context.BlackList.FirstOrDefault(bl => bl.Sender.Id == sender.Id && bl.Blocked.Id == blocked.Id);
                            context.BlackList.Remove(blackList);
                            context.SaveChanges();
                        }

                        var users = _onlineUsers.Where(o => o.Value.Id == sender.Id);
                        foreach (var user in users)
                        {
                            if (user.Key != null && user.Value != null)
                            {
                                try
                                {
                                    user.Key.RemoveUserFromBlackListCallback(new UserBaseWCF(blocked), GetConnectionId(user.Key, user.Value.SessionId));
                                }
                                catch
                                {
                                    _onlineUsers.Remove(user.Key);
                                    NotificationAboutChangeOnlineStatus(user.Value);
                                }
                            }
                        }

                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }

        public IEnumerable<UserBaseWCF> GetBlackList()
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(u => u.Key == userChanged).Value;
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    if (sender != null)
                    {
                        var blackUsers = new List<UserBaseWCF>();
                        foreach (var item in context.BlackList.ToList().Where(bl => bl.Sender.Id == sender.Id))
                        {
                            blackUsers.Add(new UserBaseWCF(item.Blocked));
                        }
                        return blackUsers;
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            });
            return result as IEnumerable<UserBaseWCF>;
        }
        #endregion

        #region login and profile

        public UserWCF CheckSession(string session)
        {
            var result = TryExecute(() =>
            {
                var context = new ChatContext();
                var user = context.Sessions.FirstOrDefault(s => s.SessionId.Equals(session)).User;
                var userWc = new UserWCF(user, session);
                if(userWc != null && !_onlineUsers.ContainsKey(OperationContext.Current.GetCallbackChannel<IUserChanged>()))
                {
                    user.SessionId = session;
                    _onlineUsers.Add(OperationContext.Current.GetCallbackChannel<IUserChanged>(), user);
                }
                return userWc;
            }, true);
            if(result is UserWCF userWcf)
            {
                return userWcf;
            }
            return null;
        }

        public UserWCF LogIn(string login, string password, string token) //user == null если нет совпадений
        {
            var result = TryExecute(() =>
             {
                 var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                 ChatContext context = new ChatContext();
                 string passwordHash = string.Empty;

                 context.Database.Log += WriteLog;
                 if (login == null || password == null)
                 {
                     login = string.Empty;
                     password = string.Empty;
                 }
                 else if (token == null)
                 {
                     token = string.Empty;
                     passwordHash = GeneratePasswordHash(password);
                 }
                 var user = context.Users.
                                   FirstOrDefault(u => (string.Equals(u.Login.ToLower(), login.ToLower()) && u.PasswordHash == passwordHash)
                                              || (u.Token == token && DbFunctions.DiffDays(u.TokenDate, DateTime.Now) < _tokenLifetime));
                 //получаем учетку пользователя, если данные совпадают

                 if (user == null)
                 {
                     WriteLog($"Не удалось найти пользователя с полученым логином[{login}] и паролем");
                     return null;
                 }
                 user.LastTimeOnline = DateTime.Now;
                 user.IsOnline = true;
                 user.Token = CreateToken(user);
                 context.SaveChanges();
                 
                 var session = $"{user.Id}{CreateToken(user)}";
                 context.Sessions.Add(new Session(user, session));
                 context.SaveChanges();
                 user.SessionId = session;
                 if (_onlineUsers.ContainsKey(userChanged))
                 {
                     _onlineUsers.Remove(userChanged);
                 }
                 _onlineUsers.Add(userChanged, user);
                 WriteLog($"Авторизация пользователя: {user.Login} УСПЕШНА");

                 var userWcf = new UserWCF(user, session);

                 NotificationAboutChangeOnlineStatus(user);

                 return userWcf;
             }, true);
            if (result is UserWCF)
            {
                return result as UserWCF;
            }
            return null;
        }
        public bool LogOut()
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var senderUChanged = _onlineUsers.FirstOrDefault(o => o.Key == userChanged);//получаем учетку по текущему подключению
                    userChanged.LogOutCallback(GetConnectionId(senderUChanged.Key, senderUChanged.Value.SessionId));
                    _onlineUsers.Remove(senderUChanged.Key);
                    var sender = context.Users.FirstOrDefault(u => u.Id == senderUChanged.Value.Id);
                    sender.IsOnline = false;
                    foreach (var item in _onlineUsers)
                    {
                        if (item.Value.Id == sender.Id)
                        {
                            sender.IsOnline = true;
                        }
                    }
                    sender.LastTimeOnline = DateTime.Now;
                    context.SaveChanges();
                    NotificationAboutChangeOnlineStatus(sender);
                    return true;
                }
                return false;
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        void NotificationAboutChangeOnlineStatus(User sender)
        {
            var friends = GetAllFriends(sender);
            foreach (var item in friends)
            {
                var users = _onlineUsers.Where(o => o.Value.Id == item.Id);
                foreach (var user in users)
                {
                    if (user.Key != null || user.Value != null)
                    {
                        try
                        {
                            user.Key.ChangeOnlineStatusCallback(new UserBaseWCF(sender), GetConnectionId(user.Key, user.Value.SessionId));//передаем сообщение всем пользователям которые онлайн(в этой группе)
                        }
                        catch
                        {
                            _onlineUsers.Remove(user.Key);
                            NotificationAboutChangeOnlineStatus(user.Value);
                        }
                    }
                }
            }
        }
        List<UserBase> GetAllFriends(User sender)//получение все связаных с sender пользователей
        {
            ChatContext context;
            context = new ChatContext();
            context.Database.Log += WriteLog;
            var friendsInGroup = context.Groups.Where(g => g.Users.Any(us => us.Id == sender.Id)).Select(gr => gr.Users).ToList();
            var userUser = context.Friends.Where(f => f.Sender.Id == sender.Id || f.User2.Id == sender.Id).ToList();
            List<UserBase> friends = new List<UserBase>();


            foreach (var groups in friendsInGroup)
            {
                foreach (var user in groups)
                {
                    if (!friends.Contains(user))
                    {
                        friends.Add(user);
                    }
                }
            }
            foreach (var item in userUser)
            {
                if (item.Sender.Id == sender.Id)
                {
                    if (!friends.Contains(item.User2))
                    {
                        friends.Add(item.User2);
                    }
                }
                else
                {
                    if (!friends.Contains(item.Sender))
                    {
                        friends.Add(item.Sender);
                    }
                }
            }
            return friends;
        }
        public bool Registration(UserWCF newUser)
        {
            var result = TryExecute(() =>
            {
                var context = new ChatContext();
                context.Database.Log += WriteLog;
                newUser.DateCreated = DateTime.Now;
                newUser.LastTimeOnline = DateTime.Now;
                newUser.IsOnline = true;
                newUser.PasswordHash = GeneratePasswordHash(newUser.PasswordHash);
                if (!LoginExist(newUser.Login) && !EmailExist(newUser.Email))
                {
                    context.Users.Add(new User(newUser));//добавляем в контекст
                    WriteLog($"Добавлен пользователь: {newUser.Login}");
                    context.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }//регистрация нового пользователя
        public bool ChangeProfileInfo(string displayName, string login)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(o => o.Key == userChanged).Value;//получаем учетку по текущему подключению
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    if (displayName != null && displayName.Length > 0 && displayName != sender.DisplayName)
                    {
                        sender.DisplayName = displayName;
                    }
                    if (login != null && login.Length >= 6 && login != sender.Login && !LoginExist(login))
                    {
                        sender.Login = login;
                    }
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    context.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        public bool ChangePassword(string newPassword, string oldPassword)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = _onlineUsers.FirstOrDefault(o => o.Key == userChanged).Value;//получаем учетку по текущему подключению
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    if (sender.PasswordHash == GeneratePasswordHash(oldPassword))
                    {
                        sender.PasswordHash = GeneratePasswordHash(newPassword);
                        sender.Token = CreateToken(sender);
                        context.SaveChanges();

                        CallUsersInGroup(_onlineUsers.Where(u => u.Key != userChanged && u.Value.Id == sender.Id).Select(u => u.Value).ToList(), (user) =>
                          {
                              user.LogOutCallback(GetConnectionId(user, _onlineUsers[user].SessionId));
                          });

                        var list = _onlineUsers.Where(u => u.Value.Id == sender.Id).ToList();
                        for (int i = 0; i < list.Count; i++)
                        {
                            _onlineUsers.Remove(list[i].Key);
                        }
                        return true;
                    }
                    else
                    {
                        context.SaveChanges();
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        public bool SendCodeForRestorePassword(string loginOrEmail)
        {
            var result = TryExecute(() =>
            {
                ChatContext context = new ChatContext();
                context.Database.Log += WriteLog;
                var sender = context.Users.FirstOrDefault(u =>
                    u.Login.Equals(loginOrEmail, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Equals(loginOrEmail, StringComparison.OrdinalIgnoreCase));//получаем учетку 

                if (sender != null && !string.IsNullOrWhiteSpace(sender.Email))
                {
                    var newRestoreCode = RandomString(_lenghtRecoveryCode);
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    SendMessageToMail(sender.Email,
                        $"Код подтверждения для восстановление пароля {sender.Login}",
                        $"Код подтверждения: {newRestoreCode}");
                    var recovery = new Recovery();
                    recovery.ConfirmationCode = newRestoreCode;
                    recovery.RecoveryTime = DateTime.Now;// бд не может принять null или 01.01.0001 в datetime
                    recovery.RequestTime = DateTime.Now;
                    recovery.IsWorking = true;
                    recovery.UserId = sender.Id;
                    recovery.NewInformation = sender.Email;

                    var recoveries = context.Recoveries.Where(r => r.IsWorking && r.UserId == sender.Id);
                    foreach (var item in recoveries)
                    {
                        item.IsWorking = false;
                    }
                    context.Recoveries.Add(recovery);
                    context.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        void SendMessageToMail(string email, string title, string text)
        {
            // отправитель - устанавливаем адрес и отображаемое в письме имя
            MailAddress from = new MailAddress(_mailLogin, "Zireael");
            // кому отправляем
            MailAddress to = new MailAddress(email);
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = title;
            // текст письма
            m.Body = text;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", _portForEmail);
            // логин и пароль
            smtp.Credentials = new NetworkCredential(_mailLogin, _mailPassword);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = true;
            smtp.Send(m);
        }
        public bool RestorePassword(string loginOrEmail, string code, string newPassword)
        {
            var result = TryExecute(() =>
            {
                ChatContext context = new ChatContext();
                context.Database.Log += WriteLog;
                var sender = context.Users.FirstOrDefault(u =>
                    u.Login.Equals(loginOrEmail, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Equals(loginOrEmail, StringComparison.OrdinalIgnoreCase));//получаем учетку 
                if (sender != null)
                {
                    sender = context.Users.FirstOrDefault(u => u.Id == sender.Id);
                    var recovery = context.Recoveries.FirstOrDefault(r => r.IsWorking
                                                               && r.ConfirmationCode == code
                                                               && r.UserId == sender.Id
                                                               && r.NewInformation == sender.Email
                                                               && DbFunctions.DiffHours(r.RequestTime, DateTime.Now) < _countHoursWorkingRecoveryCode);
                    if (recovery != null)
                    {
                        sender.PasswordHash = GeneratePasswordHash(newPassword);
                        recovery.RecoveryTime = DateTime.Now;
                        context.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        public bool SendCodeForSetNewEmail(string newEmail, string password)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var senderUChanged = _onlineUsers.FirstOrDefault(o => o.Key == userChanged);//получаем учетку по текущему подключению
                    var sender = context.Users.FirstOrDefault(u => u.Id == senderUChanged.Value.Id);
                    if (sender.PasswordHash == GeneratePasswordHash(password))
                    {
                        sender.IsOnline = true;
                        var newRestoreCode = RandomString(_lenghtRecoveryCode);
                        SendMessageToMail(newEmail,
                            $"Код подверждения для смены почты (пользователя: {sender.Login})",
                            $"Код подтверждения: {newRestoreCode}");
                        var recovery = new Recovery();

                        recovery.ConfirmationCode = newRestoreCode;
                        recovery.RequestTime = DateTime.Now;
                        recovery.RecoveryTime = DateTime.Now;
                        recovery.NewInformation = newEmail;
                        recovery.UserId = sender.Id;
                        recovery.IsWorking = true;

                        var recoveries = context.Recoveries.Where(r => r.IsWorking && r.UserId == sender.Id);

                        foreach (var item in recoveries)
                        {
                            item.IsWorking = false;
                        }
                        context.Recoveries.Add(recovery);
                        context.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        public bool SetNewEmail(string recoveryCode)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var senderUChanged = _onlineUsers.FirstOrDefault(o => o.Key == userChanged);//получаем учетку по текущему подключению
                    var sender = context.Users.FirstOrDefault(u => u.Id == senderUChanged.Value.Id);
                    sender.IsOnline = true;

                    var recovery = context.Recoveries.FirstOrDefault(r => r.IsWorking
                                                               && r.ConfirmationCode == recoveryCode
                                                               && r.UserId == sender.Id
                                                               && DbFunctions.DiffHours(r.RequestTime, DateTime.Now) < _countHoursWorkingRecoveryCode);

                    if (recovery != null)
                    {
                        if (DateTime.Now - recovery.RequestTime > new TimeSpan(_countHoursWorkingRecoveryCode, 0, 0))
                        {
                            return false;
                        }

                        sender.Email = recovery.NewInformation;
                        recovery.IsWorking = false;
                        recovery.RecoveryTime = DateTime.Now;
                        context.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }
        #endregion

        #region avatar

        public bool SetAvatarUser(AvatarUserWCF avatar)//установить аватарку пользователю
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var sender = context.Users.FirstOrDefault(u => u.Id == avatar.User.Id);
                    var oldAvatar = context.AvatarUsers.FirstOrDefault(u => u.User.Id == sender.Id);
                    if (oldAvatar == null)
                    {
                        oldAvatar = new AvatarUser();
                        context.AvatarUsers.Add(oldAvatar);
                        oldAvatar.User = sender;
                    }
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    oldAvatar.SmallData = avatar.SmallData;
                    oldAvatar.BigData = avatar.BigData;
                    oldAvatar.Format = avatar.Format;
                    oldAvatar.DateTime = avatar.DateTime;
                    context.SaveChanges();
                    foreach (var item in sender.Groups)
                    {
                        CallUsersInGroup(item.Users, (user) =>
                        {
                            user.SetAvatarCallback(avatar, new UserBaseWCF(sender));//передаем сообщение всем пользователям которые онлайн
                        });
                    }
                    foreach (var item in context.Friends.Where(uu => uu.User2.Id == sender.Id))
                    {
                        var users = _onlineUsers.Where(o => o.Value.Id == item.Sender.Id);
                        foreach (var user in users)
                        {
                            if (user.Key != null || user.Value != null)
                            {
                                try
                                {
                                    user.Key.SetAvatarCallback(avatar, new UserBaseWCF(sender));//передаем сообщение всем пользователям которые онлайн
                                }
                                catch
                                {
                                    _onlineUsers.Remove(user.Key);
                                    NotificationAboutChangeOnlineStatus(user.Value);
                                }
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }

        public string GetName(int id) => new ChatContext().Users.SingleOrDefault(u => u.Id.Equals(id))?.DisplayName;

        public IEnumerable<AvatarUserWCF> GetAvatarUsers(IEnumerable<UserBaseWCF> users)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента

                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    List<AvatarUserWCF> avatars = new List<AvatarUserWCF>();
                    foreach (var user in users)
                    {
                        foreach (var item in context.AvatarUsers.ToList().Where(a => a.User.Id == user.Id))
                        {
                            avatars.Add(new AvatarUserWCF(item));
                        }
                    }
                    return avatars;
                }
                else
                {
                    return null;
                }
            });
            if (result is IEnumerable<AvatarUserWCF> avatarlist)
            {
                return avatarlist;
            }
            return null;
        }//получить автарки пользователей

        public bool SetAvatarGroup(AvatarGroupWCF avatar)//установить аватарку группе
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var group = context.Groups.FirstOrDefault(g => g.Id == avatar.Group.Id);
                    var oldAvatar = context.AvatarGroups.FirstOrDefault(a => a.Group.Id == avatar.Group.Id);
                    if (oldAvatar == null)
                    {
                        oldAvatar = new AvatarGroup();
                        context.AvatarGroups.Add(oldAvatar);
                        oldAvatar.Group = group;
                    }
                    oldAvatar.SmallData = avatar.SmallData;
                    oldAvatar.BigData = avatar.BigData;
                    oldAvatar.Format = avatar.Format;
                    oldAvatar.DateTime = avatar.DateTime;
                    context.SaveChanges();

                    foreach (var us in group.Users)
                    {
                        var users = _onlineUsers.Where(o => o.Value.Id == us.Id);
                        foreach (var user in users)
                        {
                            if (user.Key != null || user.Value != null)
                            {
                                try
                                {
                                    user.Key.SetAvatarForGroupCallback(avatar, new GroupWCF(group));//передаем сообщение всем пользователям которые онлайн
                                }
                                catch
                                {
                                    _onlineUsers.Remove(user.Key);
                                    NotificationAboutChangeOnlineStatus(user.Value);
                                }
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }

        public IEnumerable<AvatarGroupWCF> GetAvatarGroups(IEnumerable<GroupWCF> groups)//получение аватаров всех групп
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    List<AvatarGroupWCF> avatars = new List<AvatarGroupWCF>();
                    foreach (var group in groups)
                    {
                        foreach (var item in context.AvatarGroups.Where(a => a.Group.Id == group.Id))
                        {
                            avatars.Add(new AvatarGroupWCF(item));
                        }
                    }
                    return avatars;
                }
                else
                {
                    return null;
                }
            });
            return result as IEnumerable<AvatarGroupWCF>;
        }

        #endregion

        #region find
        public IEnumerable<UserBaseWCF> FindAll(string login, int maxCount)
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var list = context.Users.Where(u => u.Login.StartsWith(login)).Take(maxCount).ToList();
                    var users = new List<UserBaseWCF>();
                    foreach (var item in list)
                    {
                        users.Add(new UserBaseWCF(item));
                    }
                    if (list.Count < maxCount)
                    {
                        var secondList = context.Users.Where(u => u.Login.Contains(login)).Take(maxCount - list.Count).ToList();
                        foreach (var item in secondList)
                        {
                            users.Add(new UserBaseWCF(item));
                        }
                    }
                    return users;
                }
                else
                {
                    return null;
                }
            });
            return result as IEnumerable<UserBaseWCF>;
        }//найти все совпадения по части логина

        public UserBaseWCF Find(string login)//найти точное совпадение группы
        {
            var result = TryExecute(() =>
            {
                var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
                ChatContext context = Context(userChanged);
                if (context != null)
                {
                    var user = context.Users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
                    if (user != null)
                    {
                        return new UserBaseWCF(user);
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            });
            return result as UserBaseWCF;
        }
        public bool LoginExist(string login)
        {
            var result = TryExecute(() =>
            {
                ChatContext context = new ChatContext();
                context.Database.Log += WriteLog;
                if (context.Users.FirstOrDefault(u => string.Equals(u.Login.ToLower(), login.ToLower())) != null)//проверка логина на существование
                {
                    WriteLog($"Логин: {login} - занят");
                    return true;
                }
                else
                {
                    WriteLog($"Логин: {login} - свободен");
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }//проверка логина на существование
        public bool EmailExist(string email)
        {
            var result = TryExecute(() =>
            {
                ChatContext context = new ChatContext();
                context.Database.Log += WriteLog;
                if (context.Users.FirstOrDefault(u => string.Equals(u.Email.ToLower(), email.ToLower())) != null)//проверка email на существование
                {
                    WriteLog($"Почта: {email} - занят");
                    return true;
                }
                else
                {
                    WriteLog($"Почта: {email} - свободен");
                    return false;
                }
            });
            if (result is bool)
            {
                return (bool)result;
            }
            return false;
        }//проверка email на существование

        #endregion

        public string SendCodeOnEmail(string email, string cookie)
        {
            var result = TryExecute(() =>
            {
                if (!EmailExist(email))
                {
                    var newRestoreCode = RandomString(_lenghtRecoveryCode);
                    SendMessageToMail(email,
                        $"Код подверждения для регистрации",
                        $"Код подтверждения: {newRestoreCode}");
                    var context = new ChatContext();

                    var emailCode = new EmailCode()
                    {
                        ConfirmationCode = newRestoreCode,
                        Cookie = cookie,
                        RequestTime = DateTime.Now,
                        Email = email
                    };
                    context.EmailCodes.Add(emailCode);
                    context.SaveChanges();
                    return newRestoreCode;
                }
                else
                {
                    //SendMessageToMail(email,
                    //        $"Код подверждения для регистрации",
                    //        $"Вы уже и так зарегистрированы!");
                    return "";
                }
            });
            if (result is string str)
            {
                return str;
            }
            return null;
        }

        public string GetEmailByCookieAndCode(string cookie, string code)
        {
            var result = TryExecute(() =>
            {
                var context = new ChatContext();
                var codes = context.EmailCodes.ToList();
                var email = codes.FirstOrDefault(e => e.Cookie.Equals(cookie, StringComparison.OrdinalIgnoreCase)
                    && e.ConfirmationCode == code.ToUpper());
                return email.Email;
            });
            if (result is string str)
            {
                return str;
            }
            return null;
        }
    }
}