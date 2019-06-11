using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wcf_CeadChat_ServiceLibrary
{
    internal static class Messenger
    {
        public class MessageHash
        {
            public Message Message { get; set; }
            public long Hash { get; set; }

            public MessageHash(Message message, long hash)
            {
                Message = message;
                Hash = hash;
            }
        }

        public static event Action<Dictionary<long, int>> MessagesReachedDB;
        private static readonly ConcurrentBag<MessageHash> UncommitedMessages;
        private const int ThreadTimeout = 3000;

        static Messenger()
        {
            UncommitedMessages = new ConcurrentBag<MessageHash>();
            Task.Factory.StartNew(async () =>
            {
                var stopwatch = new Stopwatch();
                while (true)
                {
                    stopwatch.Restart();
                    var context = new ChatContext();
                    var messagesId = new Stack<MessageHash>();
                    MessageHash messageHash = null;
                    do
                    {
                        UncommitedMessages.TryTake(out messageHash);
                        if (messageHash != null)
                        {
                            try
                            {
                                var message = messageHash.Message;
                                message.Group = context.Groups.Single(g => g.Id.Equals(message.Group.Id));
                                message.Sender = context.Users.Single(u => u.Id.Equals(message.Sender.Id));
                                context.Messages.Add(message);
                                messagesId.Push(messageHash);
                                if (message.Group.LastMessage.DateTime < message.DateTime)
                                {
                                    message.Group.LastMessage = message;
                                }

                                if (messagesId.Count > 100)
                                {
                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                        }
                    } while (messageHash != null);

                    if (messagesId.Count > 0)
                    {
                        await context.SaveChangesAsync();
                        MessagesReachedDB?.Invoke(messagesId.ToDictionary(k => k.Hash, v => v.Message.Id));
                    }
                    stopwatch.Stop();
                    if (stopwatch.ElapsedMilliseconds < ThreadTimeout)
                    {
                        Thread.Sleep(ThreadTimeout - (int)stopwatch.ElapsedMilliseconds);
                    }
                }
            });
        }

        #region messeges
        internal static IEnumerable<Message> GetMessagesFromGroup(int groupId, int senderId)
        {
            try
            {
                var context = new ChatContext();
                var sender = context.Users.FirstOrDefault(u => u.Id == senderId);
                bool inGroup = context.Groups.FirstOrDefault(g => g.Id == groupId).Users.Contains(sender);
                if (inGroup)//если пользователь в группе
                {
                    return context.Messages.ToList().Where(msg => msg.Group.Id == groupId
                                                               && msg.IsVisible);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }//получить сообщения из группы
        internal static Message SendMessage(MessageWCF message, int senderId, long hash)
        {
            try
            {
                Group group = new Group();
                Message msg = null;
                bool saveFailed;

                message.DateTime = DateTime.Now;
                do
                {
                    saveFailed = false;
                    try
                    {
                        var context = new ChatContext();
                        group = context.Groups.FirstOrDefault(g => g.Id == message.GroupId);//получаем группу с контекста по id в сообщении
                        var sender = context.Users.FirstOrDefault(u => u.Id == senderId);
                        
                        if (group.Type == GroupType.SingleUser)
                        {
                            var blockUserId = group.Users.FirstOrDefault(u => u.Id != sender.Id).Id;
                            if (context.BlackList.Any(bl => bl.Blocked.Id == sender.Id && bl.Sender.Id == blockUserId))
                            {
                                return null;
                            }
                        }

                        if (message is MessageFileWCF messageFileWcf)
                        {
                            msg = new MessageFile(messageFileWcf, group, context.Users.Single(u => u.Id == sender.Id))
                            {
                                IsVisible = false
                            };
                        }
                        else
                        {
                            msg = new Message(message, group, context.Users.Single(u => u.Id == sender.Id))
                            {
                                IsVisible = true
                            };
                            group.LastMessage = msg;
                        }
                        msg.IsRead = false;
                        sender.LastTimeOnline = DateTime.Now;
                        sender.IsOnline = true;
                        UncommitedMessages.Add(new MessageHash(msg, hash));
                        //context.Messages.Add(msg);
                        //context.SaveChanges();

                        //else
                        //{
                        //    return false;
                        //}
                    }
                    catch (DbUpdateException ex)
                    {
                        saveFailed = true;
                        // Update the values of the entity that failed to save from the store
                        //ex.Entries.Single().Reload();
                    }
                } while (saveFailed);
                return msg;
            }
            catch
            {
                return null;
            }
        }//отправить сообщение
        internal static Message DeleteMessage(int messageId, int senderId)
        {
            ChatContext context;
            Group group = null;
            Message messageFromContex = null;
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    context = new ChatContext();
                    var sender = context.Users.FirstOrDefault(u => u.Id == senderId);
                    messageFromContex = context.Messages.FirstOrDefault(m => m.Id == messageId);
                    if (messageFromContex.Sender.Id == sender.Id)
                    {
                        group = context.Groups.FirstOrDefault(g => g.Id == messageFromContex.Group.Id);//получаем группу с контекста по id в сообщении         
                                                                                                       //context.Messages.Remove(msg);//удаляем сообщение
                        messageFromContex.IsVisible = false;
                        sender.LastTimeOnline = DateTime.Now;
                        sender.IsOnline = true;
                        context.SaveChanges();// сохраняем изменения
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (DbUpdateException ex)
                {
                    saveFailed = true;
                    // Update the values of the entity that failed to save from the store
                    //ex.Entries.Single().Reload();
                }
            } while (saveFailed);
            return messageFromContex;
        }//удалить сообщение
        internal static Message GetLastMessage(int groupId)
        {
            var context = new ChatContext();
            Message lastMessage = null;
            var group = context.Groups.FirstOrDefault(g => g.Id == groupId);//получаем группу с контекста по id в сообщении         
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    context = new ChatContext();
                    var allMessagesInGroup = context.Messages.ToList().Where(m => m.Group.Id == groupId && m.IsVisible);
                    DateTime maxDateTime = allMessagesInGroup.Max(m => m.DateTime);
                    lastMessage = allMessagesInGroup.FirstOrDefault(m => m.Group.Id == groupId && m.DateTime == maxDateTime);
                    group = context.Groups.FirstOrDefault(g => g.Id == groupId);//получаем группу с контекста по id в сообщении         
                    group.LastMessage = lastMessage;
                    context.SaveChanges();// сохраняем изменения
                }
                catch (DbUpdateException ex)
                {
                    saveFailed = true;
                    // Update the values of the entity that failed to save from the store
                    //ex.Entries.Single().Reload();
                }
            } while (saveFailed);
            return lastMessage;
        }//получить последнее сообщение в группе
        internal static Group GetGroupById(int groupId, int senderId)
        {
            var context = new ChatContext();
            var group = context.Groups.FirstOrDefault(g => g.Id == groupId);//получаем группу с контекста по id   
            var sender = context.Users.FirstOrDefault(u => u.Id == senderId);
            if (group != null && sender != null && group.Users.Any(u => u.Id.Equals(sender.Id)))
            {
                return group;
            }
            return null;
        }//получить группу
        internal static Package GetPackageFromFile(int messageId, int packNumber, int senderId)
        {
            ChatContext context = new ChatContext();
            MessageFile message = context.Messages.FirstOrDefault(m => m.Id == messageId) as MessageFile;
            if (message.Sender.Id == senderId)
            {
                context.Entry(message.File).Collection(f => f.Packages)
                  .Query()
                  .Where(p => p.Number == packNumber)
                  .Load();
                var packages = message.File.Packages;

                var pack = packages.FirstOrDefault(p => p.Number == packNumber);
                return pack;
            }
            return null;
        }//получить данные с файла(в сообщеии)
        internal static MessageFile SendPackageToFile(int messageId, Package package, int senderId)
        {
            var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение клиента
            ChatContext context = new ChatContext();
            MessageFile message = context.Messages.FirstOrDefault(m => m.Id == messageId) as MessageFile;
            if (message.Sender.Id == senderId)
            {
                message.File.Packages.Add(package);
                message.File.CountReadyPackages++;
                context.SaveChanges();
                return message;
            }
            return null;
        }//отправить пакет для файла
        internal static bool SetLastMessage(int groupId, int messageId, int senderId)
        {
            var context = new ChatContext();
            var group = context.Groups.FirstOrDefault(g => g.Id == groupId);
            var message = context.Messages.FirstOrDefault(m => m.Id == messageId && m.Sender.Id == senderId);
            if (message != null)
            {
                group.LastMessage = message;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        internal static bool SetVisible(int messageId, bool isVisible, int senderId)
        {
            var context = new ChatContext();
            var message = context.Messages.FirstOrDefault(m => m.Id == messageId && m.Sender.Id == senderId);
            if (message != null)
            {
                message.IsVisible = isVisible;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        internal static Dictionary<int, int> GetDontReadMessagesFromGroups(IEnumerable<int> groupsId, int senderId)
        {
            bool saveFailed;
            Dictionary<int, int> CountDontReadsMessages = null;
            var userChanged = OperationContext.Current.GetCallbackChannel<IUserChanged>();//получаем текущее подключение
            ChatContext context = new ChatContext();
            do
            {
                saveFailed = false;
                try
                {
                    context = new ChatContext();
                    CountDontReadsMessages = new Dictionary<int, int>();//groupId, countMess
                    foreach (var item in groupsId)
                    {
                        var mess = context.Messages.Where(m => m.Group.Id == item && m.Sender.Id != senderId && !m.IsRead && m.IsVisible);
                        CountDontReadsMessages.Add(item, mess.Count());
                    }
                    var sender = context.Users.FirstOrDefault(u => u.Id == senderId);
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    saveFailed = true;
                }
            } while (saveFailed);
            return CountDontReadsMessages;
        }//получить количество не прочитанных сообщений со всех групп
        internal static Group ReadAllMessagesInGroup(int groupId, int senderId)//прочитать все сообщения в группе
        {
            ChatContext context;
            Group group = new Group();
            User sender = null;
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    context = new ChatContext();
                    sender = context.Users.FirstOrDefault(u => u.Id == senderId);
                    group = context.Groups.FirstOrDefault(g => g.Id == groupId);
                    if (group.Users.Contains(sender))
                    {
                        sender.LastTimeOnline = DateTime.Now;
                        sender.IsOnline = true;
                        foreach (var item in context.Messages.ToList().Where(m => m.Group.Id == groupId && m.Sender.Id != senderId))
                        {
                            item.IsRead = true;
                        }
                        context.SaveChanges();
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (DbUpdateException ex)
                {
                    saveFailed = true;
                }
            } while (saveFailed);
            return group;
        }
        internal static ICollection<Group> ReadAllMessagesInAllGroups(int senderId)//прочитать все сообщения во всех группах для отправляющего
        {
            ChatContext context = new ChatContext();
            var sender = context.Users.FirstOrDefault(u => u.Id == senderId);
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    context = new ChatContext();
                    sender = context.Users.FirstOrDefault(u => u.Id == senderId);
                    foreach (var group in sender.Groups)
                    {
                        foreach (var item in context.Messages.Where(m => m.Group.Id == group.Id && m.Sender.Id != sender.Id))
                        {
                            item.IsRead = true;
                        }
                    }
                    sender.LastTimeOnline = DateTime.Now;
                    sender.IsOnline = true;
                    context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    saveFailed = true;
                }
            } while (saveFailed);
            return sender.Groups;
        }
        #endregion

    }
}
