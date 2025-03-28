using System.Web.Mvc;
using System.Web.Routing;

namespace DataDashboardApi
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            // NOTE: I removed the defaults because I do not want any index page to load ///////////
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}"
                //defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
