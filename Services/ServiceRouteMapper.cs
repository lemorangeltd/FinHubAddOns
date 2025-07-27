using DotNetNuke.Web.Api;

namespace Lemorange.Modules.FinHubAddOns.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute(
                "FinHubAddOns",
                "default",
                "{controller}/{action}",
                new[] { "Lemorange.Modules.FinHubAddOns.Services" }
            );
        }
    }
}