using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BrandValues.App_Start;
using BrandValues.Models;
using MongoDB.Bson;

namespace BrandValues.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SiteAdminController : Controller
    {

        public readonly BrandValuesContext Context = new BrandValuesContext();


        // GET: SiteAdmin
        public ActionResult Index()
        {
            var versions = Context.SiteVersions.FindAll();
            return View(versions);
        }

        // GET: SiteAdmin/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: SiteAdmin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SiteAdmin/Create
        [HttpPost]
        public ActionResult Create(SiteVersionViewModel siteVersion)        
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                //var user = UserManager.FindByName(User.Identity.Name);

                Context.SiteVersions.Insert(siteVersion);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: SiteAdmin/Edit/5
        public ActionResult Edit(string id)
        {
            var siteVersion = GetVersion(id);
            return View(siteVersion);
        }


        private SiteVersion GetVersion(string id)
        {
            var siteVersion = Context.SiteVersions.FindOneById(new ObjectId(id));
            return siteVersion;
        }

        [HttpPost]
        public ActionResult Edit(string id, SiteVersionViewModel editEntry)
        {
            var siteVersion = GetVersion(id);
            siteVersion.Edit(editEntry);
            Context.SiteVersions.Save(siteVersion);
            return RedirectToAction("Index");
        }

        //public ActionResult Delete(string id)
        //{
        //    Context.SiteVersions.Remove(MongoDB.Driver.Builders.Query.EQ("_id", new ObjectId(id)));
        //    return RedirectToAction("Index");
        //}
    }
}
