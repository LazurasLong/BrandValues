using System.Web.Mvc;

namespace BrandValues.Controllers {

    [Authorize]
    public class HomeController : Controller {

        public ActionResult Index()
        {
            var user = User.Identity.Name;

            return View("Index", "", user);
        }

        public ActionResult Upload()
        {
            return View("Upload");
        }
    }
}
