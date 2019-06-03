using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Web.Controllers;

namespace Web
{
    public class CustomControllerFactory : IControllerFactory
    {
        public IController CreateController(RequestContext requestContext, string controllerName)
        {
             return new HomeController(Clients.Get(requestContext.HttpContext.Request.Cookies["SessionId"]?.Value));
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
