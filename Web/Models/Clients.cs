﻿using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Web.Controllers;
using Web.Models;
using Web.ServiceReference1;

namespace Web
{
   
    public static class Clients
    {
        private static Dictionary<string, CeadChatServiceClient> _instanse;
        private static ChatHub _chatHub;

        static Clients()
        {
            _instanse = new Dictionary<string, CeadChatServiceClient>();
            _chatHub = new ChatHub();
        }

        public static CeadChatServiceClient Add(string sessionId)
        {
            if(sessionId is null)
            {
                return new CeadChatServiceClient(new InstanceContext(_chatHub));
            }
            var client = new CeadChatServiceClient(new InstanceContext(_chatHub));
            client.Endpoint.Binding.SendTimeout = new TimeSpan(1, 0, 0);
            client.Open();
            if (_instanse.ContainsKey(sessionId))
            {
                _instanse.Remove(sessionId);
            }
            _instanse.Add(sessionId, client);
            return _instanse[sessionId];
        }

        public static CeadChatServiceClient Add(string sessionId, CeadChatServiceClient client)
        {
            _instanse.Add(sessionId, client);
            return _instanse[sessionId];
        }

        public static void Remove(string sessionId)
        {
            _instanse.Remove(sessionId);
        }
        public static void Remove(CeadChatServiceClient client)
        {
            var user = _instanse.FirstOrDefault(i => i.Value == client).Key;
            if (user != null)
            {
                _instanse.Remove(user);
            }
        }

        public static CeadChatServiceClient Get(string sessionId)
        {
            return _instanse.ContainsKey(sessionId ?? string.Empty) ? _instanse[sessionId] : Add(sessionId);
        }
    }
}
