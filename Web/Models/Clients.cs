using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Web.Controllers;
using Web.Models;
using Web.ServiceReference1;

namespace Web
{
   
    public static class Clients
    {
        private static Dictionary<string, CeadChatServiceClient> _instanse;

        static Clients()
        {
            _instanse = new Dictionary<string, CeadChatServiceClient>();
        }

        public static CeadChatServiceClient Add(string sessionId)
        {
            var client = new CeadChatServiceClient(new InstanceContext(new ChatHub()));
            client.Endpoint.Binding.SendTimeout = new TimeSpan(1, 0, 0);
            client.Open();
            _instanse.Add(sessionId, client);
            return _instanse[sessionId];
        }

        public static void Remove(string sessionId)
        {
            _instanse.Remove(sessionId);
        }

        public static CeadChatServiceClient Get(string sessionId)
        {
            return _instanse.ContainsKey(sessionId) ? _instanse[sessionId] : Add(sessionId);
        }
    }
}
