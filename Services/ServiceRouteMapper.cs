using DotNetNuke.Web.Api;
using System.Web.Http;

namespace Lemorange.Modules.FinHubAddOns.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute(
                moduleFolderName: "FinHubAddOns",
                routeName: "rpc",
                url: "{controller}/{action}/{itemId}",
                defaults: new { itemId = RouteParameter.Optional },
                namespaces: new[] { "Lemorange.Modules.FinHubAddOns.Services" });
        }
    }
}