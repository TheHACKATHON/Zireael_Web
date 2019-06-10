using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Web.ServiceReference1;
using Newtonsoft.Json;

namespace Web.Models
{
    public class ChatHub : Hub, ICeadChatServiceCallback
    {
        public ChatHub():base()
        {

        }

        public void Send(string name, string message)
        {}
 
        public string Connect()
        {
            var client = new CeadChatServiceClient(new InstanceContext(this));
            var currentUser = client.Connect(Context.RequestCookies["SessionId"].Value, Context.ConnectionId);
            return JsonConvert.SerializeObject(new { Id = currentUser.Id, Email = currentUser.Email, DisplayName = currentUser.DisplayName, Login = currentUser.Login, Avatar = $"user/{currentUser.Id}/{AdditionsLibrary.HashCode.GetMD5(currentUser.DisplayName)}" });
        }

        public void GiveIdToMessageCallback(KeyValuePair<long, int>[] messageHashId, string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                context.Clients.Client(connectionId).giveMessageId(JsonConvert.SerializeObject(messageHashId));
            }
        }

        public void CreateChatCallback(GroupWCF group, int creatorId, string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                context.Clients.Client(connectionId).addChat(group, creatorId);
            }
        }
        public void CreateMessageCallback(MessageWCF message, long hash, string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                context.Clients.Client(connectionId).addMessage(message, hash);
            }
        }

        public void DeleteMessageCallback(MessageWCF message, string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                context.Clients.Client(connectionId).deleteMessage(message.Id);
            }
        }

        public void NewLastMessageCallback(MessageWCF message, string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                context.Clients.Client(connectionId).newLastMessage(message);
            }
        }

        public void AddFriendToGroupCallback(UserBaseWCF user, GroupWCF group, string connectionId)
        {
            //throw new NotImplementedException();
        }

        public void RemoveGroupCallback(GroupWCF group, string connectionId)
        {
            //throw new NotImplementedException();
        }

        public void SetAvatarCallback(AvatarWCF avatar, UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void SetAvatarForGroupCallback(AvatarWCF avatar, GroupWCF group)
        {
            //throw new NotImplementedException();
        }

        public void ReadedMessagesCallback(GroupWCF group, UserBaseWCF sender, string connectionId)
        {
            //throw new NotImplementedException();
        }

        public void SendedPackageCallback(int msgId, int numberPackage)
        {
            //throw new NotImplementedException();
        }

        public void ChangeOnlineStatusCallback(UserBaseWCF user, string connectionId)
        {
            //throw new NotImplementedException();
        }

        public void IsOnlineCallback(string connectionId)
        {
            //throw new NotImplementedException();
        }

        public void ChangeTextInMessageCallback(MessageWCF message, string connectionId)
        {
            //throw new NotImplementedException();
        }

        public void RemoveOrExitUserFromGroupCallback(int groupId, UserBaseWCF user, string connectionId)
        {
            //throw new NotImplementedException();
        }

        public void LogOutCallback(string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                context.Clients.Client(connectionId).logout();
            }
        }



        public void AddUserToBlackListCallback(UserBaseWCF user, string connectionId)
        {
            //throw new NotImplementedException();
        }

        public void RemoveUserFromBlackListCallback(UserBaseWCF user, string connectionId)
        {
            //throw new NotImplementedException();
        }

        public void AddContactCallback(UserBaseWCF user, string connectionId)
        {
            if (user != null)
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                var md5 = AdditionsLibrary.HashCode.GetMD5(user.DisplayName);
                context.Clients.Client(connectionId).addContact(user, md5);
            }
        }

        public void RemoveContactCallback(UserBaseWCF user, string connectionId)
        {
            if (user != null)
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                context.Clients.Client(connectionId).removeContact(user.Id);
            }
        }
    }
}