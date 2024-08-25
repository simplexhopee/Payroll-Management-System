using System.Web;
using System.Web.Mvc;

namespace Checod_Africa
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new UserSessionFilterAttribute()); // Add your custom filter here
           
        }
    }
}
