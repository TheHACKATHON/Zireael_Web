using System.IO;
using System.Web.Mvc;

namespace Web.Models
{
    public static class RazorViewToStringFormat
    {
        public static string RenderRazorViewToString(Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
            // checking the view inside the controller  
            if (viewResult.View != null)
            {
                using (var sw = new StringWriter())
                {
                    var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                    viewResult.View.Render(viewContext, sw);
                    viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                    return sw.GetStringBuilder().ToString();
                }
            }
            else
                return "View cannot be found.";
        }
    }
}