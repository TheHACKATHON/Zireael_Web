using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;
            routes.MapRoute(
                name: "Default",
                url: "{action}",
                defaults: new { controller = "Home", action = "Index" }
            );
            routes.MapRoute(
               name: "UserImage",
               url: "user/{userId}/{hash}/{id}",
                defaults: new { controller = "Home", action = "UserImage", userId = UrlParameter.Optional, hash = UrlParameter.Optional, id = UrlParameter.Optional }

           );
            routes.MapRoute(
              name: "GroupImage",
              url: "group/{groupId}/{hash}/{id}",
                defaults: new { controller = "Home", action = "GroupImage", groupId = UrlParameter.Optional, hash = UrlParameter.Optional, id = UrlParameter.Optional }

          );
        }
    }
}
