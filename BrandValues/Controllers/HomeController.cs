using System.Web.Mvc;

namespace BrandValues.Controllers {
    public class HomeController : Controller {

        [Authorize]
        public ActionResult Index()
        {
            var user = User.Identity.Name;

            return View("Index", "", user);
        }
    }
}
