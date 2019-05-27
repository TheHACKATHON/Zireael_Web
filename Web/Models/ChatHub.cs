using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Web.ServiceReference1;

namespace Web.Models
{
    public class ChatHub : Hub
    {
        static readonly List<ChatUser> Users = new List<ChatUser>();
         
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }
 
        public void Connect(string userName)
        {
            var id = Context.ConnectionId;
            if (Users.All(x => x.ConnectionId != id))
            {
                Users.Add(new ChatUser { ConnectionId = id, DisplayName = userName });
                Clients.Caller.onConnected(id, userName, Users);
                Clients.AllExcept(id).onNewUserConnected(id, userName);
            }
        }
        
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = Users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                Users.Remove(item);
                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.DisplayName);
            }
 
            return base.OnDisconnected(stopCalled);
        }
    }
}