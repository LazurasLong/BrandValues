using System.Web.Mvc;

namespace BrandValues
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute());

            //#if !DEBUG
            //filters.Add(new RequireHttpsAttribute());
            //#endif
        }
    }
}
