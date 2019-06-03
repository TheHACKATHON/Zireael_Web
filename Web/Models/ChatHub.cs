using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using Microsoft.AspNet.SignalR;
using Web.ServiceReference1;

namespace Web.Models
{
    public class ChatHub : Hub, ICeadChatServiceCallback
    {
        public ChatHub():base()
        {

        }

        public void Send(string name, string message)
        {}
 
        public void Connect(int userId)
        {
            new CeadChatServiceClient(new InstanceContext(this)).Connect(Context.RequestCookies["SessionId"].Value, Context.ConnectionId);
        }

        public void CreateChatCallback(GroupWCF group, int creatorId, string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                context.Clients.Client(connectionId).addChat(group, creatorId);
            }
        }
        public void CreateMessageCallback(MessageWCF message, long hash, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void DeleteMessageCallback(MessageWCF message, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void NewLastMessageCallback(MessageWCF message, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void AddFriendToGroupCallback(UserBaseWCF user, GroupWCF group, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void RemoveGroupCallback(GroupWCF group, string sessionId)
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

        public void ReadedMessagesCallback(GroupWCF group, UserBaseWCF sender, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void SendedPackageCallback(int msgId, int numberPackage)
        {
            //throw new NotImplementedException();
        }

        public void ChangeOnlineStatusCallback(UserBaseWCF user, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void IsOnlineCallback(string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void ChangeTextInMessageCallback(MessageWCF message, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void RemoveOrExitUserFromGroupCallback(int groupId, UserBaseWCF user, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void LogOutCallback(string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void AddUserToBlackListCallback(UserBaseWCF user, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void RemoveUserFromBlackListCallback(UserBaseWCF user, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void AddContactCallback(UserBaseWCF user, string sessionId)
        {
            //throw new NotImplementedException();
        }

        public void RemoveContactCallback(UserBaseWCF user, string sessionId)
        {
            //throw new NotImplementedException();
        }
    }
}