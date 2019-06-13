using System.Web;
using System.Web.Optimization;

namespace Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;

            #region layout
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/Scripts/NotifyType.js",
                "~/Scripts/notifications.js",
                "~/Scripts/jquery-3.4.1.min.js",
                "~/Scripts/jquery.scrollbar.min.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/css").Include(
                "~/Content/notifications.css",
                "~/Content/loading.css",
                "~/Content/temp-vlad.css",
                "~/Content/temp-nikita.css"
                ));
            #endregion
            
            #region auth
            bundles.Add(new StyleBundle("~/bundles/auth").Include(
               "~/Content/auth.css"
               ));
            
            bundles.Add(new ScriptBundle("~/bundles/authjs").Include(
               "~/Scripts/auth.js"
               ));
            #endregion

            #region index
            bundles.Add(new StyleBundle("~/bundles/indexcss").Include(
              "~/Content/modal-dialogs.css",
              "~/Content/dialogs.css",
              "~/Content/jquery.scrollbar.css",
              "~/Content/main_style.css"
              ));

            bundles.Add(new ScriptBundle("~/bundles/indexjs").Include(
               "~/Scripts/jquery.signalR-2.4.1.min.js",
               "~/Scripts/2.5.3-crypto-md5.js",
               "~/Scripts/generator.js",
               "~/Scripts/signal-callback.js",
               "~/Scripts/modal-dialogs.js",
               "~/Scripts/jquery.mousewheel.min.js",
               "~/Scripts/main.js"
               ));


            #endregion

            foreach (var item in bundles)
            {
                if(item is ScriptBundle)
                {
                    item.Transforms.Add(new JsMinify());
                }
                else if(item is StyleBundle)
                {
                    item.Transforms.Add(new CssMinify());
                }
            }
        }
    }
}
