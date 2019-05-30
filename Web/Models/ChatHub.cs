using System;
using System.Collections.Generic;
using System.Linq;
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
        public ChatUser User { get; set; }

        public void Send(string name, string message)
        {}
 
        public void Connect(int userId)
        {
            
            User = new ChatUser();
            User.ConnectionId = Context.ConnectionId;
            User.Id = userId;
        }

        public void CreateChatCallback(GroupWCF group, int creatorId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            context.Clients.All.addChat(group, creatorId);
        }

        public void CreateMessageCallback(MessageWCF message, long hash)
        {
            //throw new NotImplementedException();
        }

        public void DeleteMessageCallback(MessageWCF message)
        {
            //throw new NotImplementedException();
        }

        public void NewLastMessageCallback(MessageWCF message)
        {
            //throw new NotImplementedException();
        }

        public void AddFriendToGroupCallback(UserBaseWCF user, GroupWCF group)
        {
            //throw new NotImplementedException();
        }

        public void RemoveGroupCallback(GroupWCF group)
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

        public void ReadedMessagesCallback(GroupWCF group, UserBaseWCF sender)
        {
            //throw new NotImplementedException();
        }

        public void SendedPackageCallback(int msgId, int numberPackage)
        {
            //throw new NotImplementedException();
        }

        public void ChangeOnlineStatusCallback(UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void IsOnlineCallback()
        {
            //throw new NotImplementedException();
        }

        public void ChangeTextInMessageCallback(MessageWCF message)
        {
            //throw new NotImplementedException();
        }

        public void RemoveOrExitUserFromGroupCallback(int groupId, UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void LogOutCallback()
        {
            //throw new NotImplementedException();
        }

        public void AddUserToBlackListCallback(UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void RemoveUserFromBlackListCallback(UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void AddContactCallback(UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }

        public void RemoveContactCallback(UserBaseWCF user)
        {
            //throw new NotImplementedException();
        }
    }
}