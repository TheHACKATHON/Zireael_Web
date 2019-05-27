using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Web.Controllers;
using Web.ServiceReference1;

namespace Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }

    public static class Clients
    {
        private static Dictionary<string, CeadChatServiceClient> _instanse;
        //public static Dictionary<string, CeadChatServiceClient> Get { get => _instanse ?? (_instanse = new Dictionary<string, CeadChatServiceClient>()); }

        static Clients()
        {
            _instanse = new Dictionary<string, CeadChatServiceClient>();
        }

        public static CeadChatServiceClient Add(string sessionId)
        {
            var client = new CeadChatServiceClient(new InstanceContext(new HomeController(null)));
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

    public class CustomControllerFactory : IControllerFactory
    {
        public IController CreateController(RequestContext requestContext, string controllerName)
        {
             return new HomeController(Clients.Get(requestContext.HttpContext.Session.SessionID));
        }

        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            return SessionStateBehavior.Required;
        }

        public void ReleaseController(IController controller)
        {
            if (controller is IDisposable disposable)
                disposable.Dispose();
            else
                controller = null;
        }
    }
}
