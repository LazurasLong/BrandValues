using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Web.WebPages;
using Amazon.DataPipeline.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System.Drawing.Imaging;
using Amazon;
using System.IO;
using System.Drawing;
using System.Collections.Specialized;
using System.Configuration;
using Antlr.Runtime.Misc;
using AspNet.Identity.MongoDB;
using BrandValues.Cloudfront;
using BrandValues.App_Start;
using BrandValues.Entries;
using Microsoft.Ajax.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using Amazon.CloudFront;
using BrandValues.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Globalization;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using MongoDB.Driver.Builders;
using BrandValues.Utils;
using System.Text.RegularExpressions;


namespace BrandValues.Controllers {

    [Authorize]
    public class HomeController : Controller {

        public readonly BrandValuesContext Context = new BrandValuesContext();

        public HomeController() { }

        public HomeController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        NameValueCollection appConfig = ConfigurationManager.AppSettings;

        private SiteVersion _siteVersion;
        public SiteVersion SiteVersion
        {
            get
            {
                return _siteVersion ?? Context.SiteVersions.FindOne();
            }
            private set
            {
                _siteVersion = value;
            }
        }


        //cache
        //[OutputCache(Duration = 60)]
        public ActionResult Index()
        {
            //var user = User.Identity.Name;

            //Test Mongo Access
            //Context.Database.GetStats();
            //return Json(Context.Database.Server.BuildInfo, JsonRequestBehavior.AllowGet);

            var version = SiteVersion.Homepage;
            ViewBag.SiteVersion = version;

            //get menu
            ViewBag.Menu = GetMenu();

            ViewBag.Voting = GetVotingStatus();

            

            //surveys
            var username = User.Identity.Name;

            Random rand = new Random();
            var pollsNotCompleted = Context.Polls.FindAll().Where(i => !i.Completed.Contains(username))
                .Select(r => new Poll
                {
                    Name = r.Name
                }).OrderBy(x => rand.Next()).Take(1);

            if (pollsNotCompleted != null && pollsNotCompleted.Any())
            {
                var e = pollsNotCompleted.FirstOrDefault();
                
                switch (e.Name) 
                {
                    case "enjoy":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=R2SRlMeswwbZf8eSY0_2bEuA_3d_3d";
                        ViewBag.SurveyName = "enjoy";
                        break;
                    case "describe":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=fups23Yw9gVFiGPvAMro8Q_3d_3d";
                        ViewBag.SurveyName = "describe";
                        break;
                    case "focus":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=wwFVFlxc6h6r_2faCTsCfByw_3d_3d";
                        ViewBag.SurveyName = "focus";
                        break;
                    case "role":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=CaPLVGUo3frY7trnJFTw4A_3d_3d";
                        ViewBag.SurveyName = "role";
                        break;
                    case "energised":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=qB8YNl_2feMg3eYD8EDzZDtA_3d_3d";
                        ViewBag.SurveyName = "energised";
                        break;
                    case "culture":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=upntmzov3Gbrjzrag_2bPPwA_3d_3d";
                        ViewBag.SurveyName = "culture";
                        break;
                    case "pride":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=_2b5zV_2bvy9xjH6M1RJ5B4yHA_3d_3d";
                        ViewBag.SurveyName = "pride";
                        break;
                }
   
            }

            if (version == "Version4")
            {

                return View("HomePageV4");
            }


            if (version == "Version3")
            {
                
                var teamEntries = Context.ShortlistedEntries.FindAllAs<ShortlistedEntry>().Where(x => x.Type.Contains("team"));
                var individualEntries = Context.ShortlistedEntries.FindAllAs<ShortlistedEntry>().Where(x => x.Type.Contains("individual"));

                ShortlistViewModel shortlistViewModel = new ShortlistViewModel();
                shortlistViewModel.IndividualEntries = individualEntries;
                shortlistViewModel.TeamEntries = teamEntries;

                return View("HomePageV3", shortlistViewModel);
            }

            if (version == "Version2")
            {
                var allDocs = Context.Entries.FindAll();
                return View("HomePageV2", allDocs);
                //return View("HomePageV2");
            }

            //return last 6 entries
            SortByBuilder sbb = new SortByBuilder();
            sbb.Descending("CreatedOn");
            var latest = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb).SetLimit(6);

            return View("HomePageV1", latest);
            //return View("HomePageV1");

        }

        [HttpPost]
        public async Task<JsonResult> UpdatePoll(string pollName)
        {
            //update user polls
            if (!string.IsNullOrEmpty(pollName))
            {
                //http://localhost/BrandValues/Home/updatepoll?pollName=enjoy
                //https://valuescompetition.com/home/updatepoll?pollname=enjoy

                var user = await UserManager.FindByNameAsync(User.Identity.Name);
                if (user != null)
                {
                    var poll = GetPoll(pollName);
                    if (poll == null)
                    {
                        switch (pollName)
                        {
                            case "enjoy":
                                    AddPoll(pollName);
                                break;
                            case "describe":
                                    AddPoll(pollName);
                                break;
                            case "focus":
                                AddPoll(pollName);
                                break;
                            case "role":
                                AddPoll(pollName);
                                break;
                            case "energised":
                                AddPoll(pollName);
                                break;
                            case "culture":
                                AddPoll(pollName);
                                break;
                            case "pride":
                                AddPoll(pollName);
                                break;
                            default:
                                return Json("No poll name selected", JsonRequestBehavior.AllowGet);
                        }

                        poll = GetPoll(pollName);

                    }

                    poll.Completed.Add(user.UserName);

                    Context.Polls.Save(poll);
                }
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }

            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        private void AddPoll(string name)
        {
            Poll newPoll = new Poll();
            newPoll.Name = name;
            Context.Polls.Save(newPoll);
        }

        private Poll GetPoll(string name)
        {
            //http://theprogrammersnotebook.wordpress.com/2014/03/19/mongodb-raw-query-with-c-driver/
            var jsonQuery = "{ 'Name' : '" + name + "' }";

            BsonDocument doc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jsonQuery);
            var query = new QueryDocument(doc);

            var poll = Context.Polls.FindOne(query);
            return poll;
        }

        public List<SelectListItem> GetMenu()
        {
            //dropdown menu
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "Home", Value = "0" });

            items.Add(new SelectListItem { Text = "How the competition works", Value = "1" });

            items.Add(new SelectListItem { Text = "What prizes are up for grabs?", Value = "2" });

            items.Add(new SelectListItem { Text = "A reminder of the AIB brand values", Value = "3" });

            items.Add(new SelectListItem { Text = "All your questions answered…", Value = "4" });

            return items;
        }

        [HttpGet]
        public ActionResult Search(string term)
        {

            if (string.IsNullOrEmpty(term))
                return PartialView("_Search");

            var searchTerm = term.Trim().ToLower();
            searchTerm = searchTerm.ToLower();

            var userName = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.UserName, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var teamName = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.TeamName, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var desc = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.Description, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var name = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.Name, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));

            var searchQuery =
                MongoDB.Driver.Builders.Query.Or(
                    userName, teamName, desc, name
                );

            var entries = Context.Entries.Find(searchQuery)
                .SetSortOrder(SortBy<Entry>.Descending(g => g.CreatedOn));

            //var teamEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("team")).OrderByDescending(x => x.CreatedOn);
            //var individualEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("individual")).OrderByDescending(x => x.CreatedOn);

            BrowseViewModel browseViewModel = new BrowseViewModel();
            browseViewModel.IndividualEntries = entries.Where(x => x.Type.Contains("individual"));
            browseViewModel.TeamEntries = entries.Where(x => x.Type.Contains("team")); ;

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_Search", browseViewModel);
        }

        public ActionResult Autocomplete(string term)
        {
            //test directly using http://localhost/BrandValues/Home/Autocomplete?term=tes

            if (string.IsNullOrEmpty(term))
                return Json("Nothing found", JsonRequestBehavior.AllowGet);

            var searchTerm = term.Trim().ToLower();
            searchTerm = searchTerm.ToLower();

            var userName = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.UserName, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var teamName = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.TeamName, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var desc = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.Description, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var name = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.Name, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));

            var searchQuery =
                MongoDB.Driver.Builders.Query.Or(
                    userName, teamName, desc, name
                );

            var entries = Context.Entries
                .FindAs<Entry>(searchQuery)
                .SetSortOrder(SortBy<Entry>.Descending(g => g.CreatedOn))
                .SetLimit(5).Select(r => new
                {
                    entryName = r.Name,
                    userFirstName = r.UserFirstName,
                    userSurname = r.UserSurname
                });


            //var entries = Context.Entries.FindAll().Where(
            //    x => x.UserName.ToLower().Contains(searchTerm)
            //    ).Take(5).Select(r => new
            //    {
            //        entryName = r.Name,
            //        userFirstName = r.UserFirstName,
            //        userSurname = r.UserSurname
            //    });

            return Json(entries, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CategoryChosen(string Menu)
        {

            var selection = Convert.ToInt32(Menu);

            SortByBuilder sbb = new SortByBuilder();
            sbb.Descending("CreatedOn");
            var latest = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb).SetLimit(6);

            ViewBag.Voting = GetVotingStatus();

            //surveys
            var username = User.Identity.Name;

            Random rand = new Random();
            var pollsNotCompleted = Context.Polls.FindAll().Where(i => !i.Completed.Contains(username))
                .Select(r => new Poll
                {
                    Name = r.Name
                }).OrderBy(x => rand.Next()).Take(1);

            if (pollsNotCompleted != null && pollsNotCompleted.Any())
            {
                var e = pollsNotCompleted.FirstOrDefault();

                switch (e.Name)
                {
                    case "enjoy":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=R2SRlMeswwbZf8eSY0_2bEuA_3d_3d";
                        ViewBag.SurveyName = "enjoy";
                        break;
                    case "describe":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=fups23Yw9gVFiGPvAMro8Q_3d_3d";
                        ViewBag.SurveyName = "describe";
                        break;
                    case "focus":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=wwFVFlxc6h6r_2faCTsCfByw_3d_3d";
                        ViewBag.SurveyName = "focus";
                        break;
                    case "role":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=CaPLVGUo3frY7trnJFTw4A_3d_3d";
                        ViewBag.SurveyName = "role";
                        break;
                    case "energised":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=qB8YNl_2feMg3eYD8EDzZDtA_3d_3d";
                        ViewBag.SurveyName = "energised";
                        break;
                    case "culture":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=upntmzov3Gbrjzrag_2bPPwA_3d_3d";
                        ViewBag.SurveyName = "culture";
                        break;
                    case "pride":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=_2b5zV_2bvy9xjH6M1RJ5B4yHA_3d_3d";
                        ViewBag.SurveyName = "pride";
                        break;
                }

            }

            switch (selection)
            {
                case 0:
                    var version = SiteVersion.Homepage;
                    if (version == "Version3")
                    {
                        var teamEntries = Context.ShortlistedEntries.FindAllAs<ShortlistedEntry>().Where(x => x.Type.Contains("team"));
                        var individualEntries = Context.ShortlistedEntries.FindAllAs<ShortlistedEntry>().Where(x => x.Type.Contains("individual"));

                        ShortlistViewModel shortlistViewModel = new ShortlistViewModel();
                        shortlistViewModel.IndividualEntries = individualEntries;
                        shortlistViewModel.TeamEntries = teamEntries;

                        return PartialView("_IntroV3", shortlistViewModel);
                    }
                    if (version == "Version2")
                    {
                        var allEntries = Context.Entries.FindAll();
                        return PartialView("_IntroV2", allEntries);
                    }
                    //return last 6 entries
                    return PartialView("_Intro", latest);
                case 1:
                    return PartialView("_HowItWorks");
                case 2:
                    return PartialView("_Prizes");
                case 3:
                    return PartialView("_BrandValues");
                case 4:
                    return PartialView("_FAQ");
            }

            return PartialView("_Intro", latest);
        }

        public PartialViewResult Intro()
        {
            SortByBuilder sbb = new SortByBuilder();
            sbb.Descending("CreatedOn");
            var allDocs = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb).SetLimit(6);
            ViewBag.Voting = GetVotingStatus();
            return PartialView("_Intro", allDocs);
        }

        public PartialViewResult IntroV2()
        {

            //surveys
            var username = User.Identity.Name;

            Random rand = new Random();
            var pollsNotCompleted = Context.Polls.FindAll().Where(i => !i.Completed.Contains(username))
                .Select(r => new Poll
                {
                    Name = r.Name
                }).OrderBy(x => rand.Next()).Take(1);

            if (pollsNotCompleted != null && pollsNotCompleted.Any())
            {
                var e = pollsNotCompleted.FirstOrDefault();

                switch (e.Name)
                {
                    case "enjoy":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=R2SRlMeswwbZf8eSY0_2bEuA_3d_3d";
                        ViewBag.SurveyName = "enjoy";
                        break;
                    case "describe":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=fups23Yw9gVFiGPvAMro8Q_3d_3d";
                        ViewBag.SurveyName = "describe";
                        break;
                    case "focus":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=wwFVFlxc6h6r_2faCTsCfByw_3d_3d";
                        ViewBag.SurveyName = "focus";
                        break;
                    case "role":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=CaPLVGUo3frY7trnJFTw4A_3d_3d";
                        ViewBag.SurveyName = "role";
                        break;
                    case "energised":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=qB8YNl_2feMg3eYD8EDzZDtA_3d_3d";
                        ViewBag.SurveyName = "energised";
                        break;
                    case "culture":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=upntmzov3Gbrjzrag_2bPPwA_3d_3d";
                        ViewBag.SurveyName = "culture";
                        break;
                    case "pride":
                        ViewBag.SurveySrc = "https://www.surveymonkey.com/jsEmbed.aspx?sm=_2b5zV_2bvy9xjH6M1RJ5B4yHA_3d_3d";
                        ViewBag.SurveyName = "pride";
                        break;
                }

            }


            var allDocs = Context.Entries.FindAll();
            ViewBag.Voting = GetVotingStatus();
            return PartialView("_IntroV2", allDocs);
        }

        public PartialViewResult IntroV3()
        {
            ViewBag.Voting = GetVotingStatus();
            var teamEntries = Context.ShortlistedEntries.FindAllAs<ShortlistedEntry>().Where(x => x.Type.Contains("team"));
            var individualEntries = Context.ShortlistedEntries.FindAllAs<ShortlistedEntry>().Where(x => x.Type.Contains("individual"));

            ShortlistViewModel shortlistViewModel = new ShortlistViewModel();
            shortlistViewModel.IndividualEntries = individualEntries;
            shortlistViewModel.TeamEntries = teamEntries;

            return PartialView("_IntroV3", shortlistViewModel);
        }

        public PartialViewResult HowItWorks()
        {
            return PartialView("_HowItWorks");
        }

        public PartialViewResult Prizes()
        {
            return PartialView("_Prizes");
        }

        public PartialViewResult BrandFilm()
        {
            return PartialView("_BrandFilm");
        }

        public PartialViewResult BrandValues()
        {
            return PartialView("_BrandValues");
        }

        public PartialViewResult WinningEntry()
        {
            if (IpAddress.CheckIp())
            {
                ViewBag.NetworkPC = true;
            }
            else
            {
                ViewBag.NetworkPC = false;
            }
            return PartialView("_WinningEntry");
        }

        public PartialViewResult FAQs()
        {
            return PartialView("_FAQ");
        }

        public PartialViewResult Trending()
        {
            return PartialView("_Trending");
        }

        private bool GetVotingStatus()
        {
            var votingEnabled = SiteVersion.Voting;

            if (votingEnabled == true)
            {
                ViewBag.Voting = true;
                return true;
            }

            return false;
        }

        //Browse
        public ActionResult Browse()
        {
            var teamEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("team")).OrderByDescending(x => x.CreatedOn);
            var individualEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("individual")).OrderByDescending(x => x.CreatedOn);

            BrowseViewModel browseViewModel = new BrowseViewModel();
            browseViewModel.IndividualEntries = individualEntries;
            browseViewModel.TeamEntries = teamEntries;

            ViewBag.Voting = GetVotingStatus();

            return View(browseViewModel);
        }

        public PartialViewResult All()
        {
            var teamEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("team")).OrderByDescending(x => x.CreatedOn);
            var individualEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("individual")).OrderByDescending(x => x.CreatedOn);

            BrowseViewModel browseViewModel = new BrowseViewModel();
            browseViewModel.IndividualEntries = individualEntries;
            browseViewModel.TeamEntries = teamEntries;

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", browseViewModel);
        }

        public PartialViewResult AllValues()
        {
            var searchTerm = "all";
            var teamEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("team") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);
            var individualEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("individual") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);

            BrowseViewModel browseViewModel = new BrowseViewModel();
            browseViewModel.IndividualEntries = individualEntries;
            browseViewModel.TeamEntries = teamEntries;

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", browseViewModel);
        }

        public PartialViewResult Customers()
        {
            var searchTerm = "customers";
            var teamEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("team") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);
            var individualEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("individual") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);

            BrowseViewModel browseViewModel = new BrowseViewModel();
            browseViewModel.IndividualEntries = individualEntries;
            browseViewModel.TeamEntries = teamEntries;

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", browseViewModel);
        }

        public PartialViewResult Empowering()
        {
            var searchTerm = "empowering";
            var teamEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("team") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);
            var individualEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("individual") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);

            BrowseViewModel browseViewModel = new BrowseViewModel();
            browseViewModel.IndividualEntries = individualEntries;
            browseViewModel.TeamEntries = teamEntries;

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", browseViewModel);
        }

        public PartialViewResult Trust()
        {
            var searchTerm = "trust";
            var teamEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("team") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);
            var individualEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("individual") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);

            BrowseViewModel browseViewModel = new BrowseViewModel();
            browseViewModel.IndividualEntries = individualEntries;
            browseViewModel.TeamEntries = teamEntries;

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", browseViewModel);
        }

        public PartialViewResult Simple()
        {
            var searchTerm = "simple";
            var teamEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("team") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);
            var individualEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("individual") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);

            BrowseViewModel browseViewModel = new BrowseViewModel();
            browseViewModel.IndividualEntries = individualEntries;
            browseViewModel.TeamEntries = teamEntries;

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", browseViewModel);
        }


        public PartialViewResult Together()
        {
            var searchTerm = "together";
            var teamEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("team") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);
            var individualEntries = Context.Entries.FindAllAs<Entry>().Where(x => x.Type.Contains("individual") && x.Values.Contains(searchTerm)).OrderByDescending(x => x.CreatedOn);

            BrowseViewModel browseViewModel = new BrowseViewModel();
            browseViewModel.IndividualEntries = individualEntries;
            browseViewModel.TeamEntries = teamEntries;

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", browseViewModel);
        }

        //cache
        //[OutputCache(Duration = 600)]
        public ActionResult Play(string id)
        {
            if (id.IsNullOrWhiteSpace())
            {
                return RedirectToAction("Index");
            }

            //return last 6 entries
            SortByBuilder sbb = new SortByBuilder();
            sbb.Descending("CreatedOn");
            //var allDocs = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb).SetLimit(4);
            Random r = new Random();
            var allDocs = Context.ShortlistedEntries.FindAllAs<ShortlistedEntry>().OrderBy(x => r.Next()).Take(4);

            //get Entry first
            var entry = GetEntry(id);

            //get video for entry
            //RTMP resources do not take the form of a URL, 
            //and instead the resource path is nothing but the stream's name. e.g. "video1.mp4"
            if (entry.Format == "video")
            {
                //check for AIB network PC and disable
                

                if (IpAddress.CheckIp())
                {
                    ViewBag.NetworkCheck = true;
                }
                else
                {
                    ViewBag.NetworkCheck = false;
                    //ViewBag.RTMPUrl = GetRTMPCloudfrontUrl(entry);
                    ViewBag.AppleUrl = GetAppleCloudFrontUrl(entry);
                    ViewBag.FallbackUrl = GetFallbackMP4CloudFrontUrl(entry);
                    ViewBag.VideoThumbnailUrl = GetVideoThumbnailUrl(entry);
                } 

            } else {
                ViewBag.CloudFrontUrl = GetCloudFrontUrl(entry);
            }       

            PlayViewModel playViewModel = new PlayViewModel();
            playViewModel.Entry = entry;
            playViewModel.Entries = allDocs;

            //check site version for like/vote
            var votingEnabled = SiteVersion.Voting;

            //check for vote
            ViewBag.Voted = false;
            ViewBag.Liked = false;

            var allShortlistedEntries = Context.ShortlistedEntries.FindAllAs<ShortlistedEntry>().Where(x => x.Id.Contains(entry.Id));

            if (allShortlistedEntries.Any())
            {
                if (votingEnabled == true)
                {
                    ViewBag.Voting = true;

                    foreach (string x in entry.Votes)
                    {
                        if (x.Contains(User.Identity.Name))
                        {
                            ViewBag.Voted = true;
                        }
                    }
                    return View(playViewModel);
                }

            }

            ViewBag.Voting = false;


            //check for like

            foreach (string x in entry.Likes)
            {
                if (x.Contains(User.Identity.Name))
                {
                    ViewBag.Liked = true;
                }
            }


            return View(playViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PostComment(FormCollection form, PostComment postComment)
        {
            //get entry
            var id = form["Entry.Id"];
            var entry = GetEntry(id);
            
            postComment.UserName = User.Identity.Name;
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            postComment.UserArea = user.Area;
            postComment.UserFirstName = user.FirstName;
            postComment.UserSurname = user.Surname;
            postComment.CreatedOn = DateTime.Now;
            postComment.Censored = false;
            postComment.Comment = form["comment"];

            //Check if your Comments collection exists
            if (entry.Comments == null)
            {
                //It's null - create it
                entry.Comments = new List<PostComment>();
            }

            entry.Comments.Add(postComment);

            Context.Entries.Save(entry);

            //return Json("Ok", JsonRequestBehavior.AllowGet);
            return PartialView("_Comments", entry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostLike(FormCollection form)
        {
            //get entry
            var id = form["Entry.Id"];
            var entry = GetEntry(id);

            //var user = await UserManager.FindByNameAsync(User.Identity.Name);

            entry.Likes.Add(User.Identity.Name);

            Context.Entries.Save(entry);

            return Json("Ok", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostVote(FormCollection form)
        {
            //get entry
            var id = form["Entry.Id"];
            var entry = GetEntry(id);

            //var user = await UserManager.FindByNameAsync(User.Identity.Name);

            entry.Votes.Add(User.Identity.Name);

            Context.Entries.Save(entry);

            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        private string GetRTMPCloudfrontUrl(Entry entry)
        {
            string rtmpUrl = appConfig["RTMPCloudfront"];
            string pathUrl = entry.Url;
            string fileName = Path.GetFileNameWithoutExtension(pathUrl);
            string videoUrl = fileName + ".mp4";

            //remove file name
            var result = pathUrl.Replace(videoUrl, "");
            
            string signedVideoUrl = GetSignedUrl.GetCloudfrontUrl(videoUrl);
            return rtmpUrl + result + "mp4:" + signedVideoUrl;
        }

        private string GetAppleCloudFrontUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["SimpleTranscoderCloudfront"];
            string fileName = Path.GetFileNameWithoutExtension(entry.Url);
            string relativeUrl = entry.Url.Substring(0, entry.Url.LastIndexOf('/'));
            string url = cloudfrontUrl + relativeUrl + "/" + fileName + ".m3u8";
            return url;
        }

        private string GetFallbackMP4CloudFrontUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["TranscoderCloudfront"];
            string fileName = Path.GetFileNameWithoutExtension(entry.Url);
            string relativeUrl = entry.Url.Substring(0, entry.Url.LastIndexOf('/'));
            string url = cloudfrontUrl + relativeUrl + "/" + fileName + ".mp4";
            return GetSignedUrl.GetCloudfrontUrl(url);
        }

        private string GetCloudFrontUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["UploadCloudfront"];
            string relativeUrl = entry.Url;
            string url = cloudfrontUrl + relativeUrl;
            return GetSignedUrl.GetCloudfrontUrl(url);
        }

        private string GetSignedVideoThumbnailUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["TranscoderCloudfront"];
            string relativeUrl = entry.VideoThumbnailUrl;
            string url = cloudfrontUrl + relativeUrl;
            return GetSignedUrl.GetCloudfrontUrl(url);
        }

        private string GetVideoThumbnailUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["SimpleTranscoderCloudfront"];
            string relativeUrl = entry.VideoThumbnailUrl;
            return cloudfrontUrl + relativeUrl;
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult Upload()
        {
            //ViewBag.Message = "Test";
            //PostEntry postEntry = new PostEntry();

            //GetValuesList(postEntry);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upload(PostEntry postEntry, IEnumerable<HttpPostedFileBase> files)
        {
            //positive outcome response
            var myResponse = "";
            
            //check that model is valid
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please complete the form";
                return View(postEntry);
            }

            //start new entry from posted fields
            var entry = new Entry(postEntry);

            //get users details
            entry.UserName = User.Identity.Name;
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            entry.UserArea = user.Area;
            entry.UserFirstName = user.FirstName;
            entry.UserSurname = user.Surname;

            //check that user has completed team name
            if (postEntry.Type == "team") {
                if (postEntry.TeamName.IsEmpty() || postEntry.TeamNumber.IsEmpty() || postEntry.TeamMemberNames.IsEmpty())
                {
                    ViewBag.ErrorMessage = "Please complete your team details";
                    return View(postEntry);
                }
            }

            //set date
            entry.CreatedOn = DateTime.Now;



            foreach (var file in files)
            {

                if (file == null)
                {
                    ViewBag.ErrorMessage = "Please select a file to upload";
                    return View(postEntry);
                }


                //network check for 10MB limit
                if (IpAddress.CheckIp())
                {
                    if (file.ContentLength > 10485760) //bytes
                    {
                        ViewBag.ErrorMessage = "Sorry but the file you're trying to upload is too big for the AIB network. Please contact us at <a href='mailto:aib@valuescompetition.com?Subject=Issue%20uploading'>aib@valuescompetition.com</a> for support uploading your file.";
                        return View(postEntry);
                    }
                }



                string accessKey = appConfig["S3AWSAccessKey"];
                string secretKey = appConfig["S3AWSSecretKey"];

                string siteFilesCloudfront = appConfig["SiteFilesCloudfront"];


                IAmazonS3 client;
                var filePath = "";

                //remove spaces
                var username = User.Identity.Name;
                if (username.Contains('@'))
                    username = username.Substring(0, username.LastIndexOf('@'));

                //remove odd characters
                username = Regex.Replace(username, "[^0-9a-zA-Z.]+", "");

                var entryName = entry.Name.Replace(" ", String.Empty);
                entryName = entryName + DateTime.Now;
                entryName = Regex.Replace(entryName, "[^0-9a-zA-Z]+", "");
                var newFileName = entryName + Path.GetExtension(file.FileName);

                var foldername = username;
                    //Path.GetFileNameWithoutExtension(newFileName);

                if (entry.Format == "story")
                {
                    entry.ThumbnailUrl = siteFilesCloudfront + "/entries/document.png";
                }
                if (entry.Format == "poem")
                {
                    entry.ThumbnailUrl = siteFilesCloudfront + "/entries/poem.png";
                }
                if (entry.Format == "lyric")
                {
                    entry.ThumbnailUrl = siteFilesCloudfront + "/entries/lyrics.png";
                }
                if (entry.Format == "image")
                {
                    if (!file.ContentType.Contains("image/"))
                    {
                        entry.Format = "text";
                    }
                    entry.ThumbnailUrl = siteFilesCloudfront + "/entries/photo.png";
                }
                if (entry.Format == "video")
                {
                    entry.ThumbnailUrl = siteFilesCloudfront + "/entries/video.png";
                }
                if (entry.Format == "other")
                {
                    entry.ThumbnailUrl = siteFilesCloudfront + "/entries/other.png";
                }

                if (file.ContentType.Contains("video/"))
                {
                    //set the submission format to match filetype
                    entry.Format = "video";
                    filePath = "video/" + foldername + "/" + newFileName;
                    entry.VideoThumbnailUrl = "video/" + foldername + "/" + entryName + "_00001.png";
                    entry.ThumbnailUrl = siteFilesCloudfront + "/entries/video.png";
                    entry.Url = filePath;
                }

                if (file.ContentType.Contains("text/") || 
                    file.ContentType.Contains("application/pdf") || 
                    file.ContentType.Contains("application/msword") || 
                    file.ContentType.Contains("application/vnd.ms-powerpoint") ||
                    file.ContentType.Contains("application/vnd.openxmlformats-officedocument")
                    )
                {
                    //set the submission format to match filetype
                    //entry.Format = "text";
                    filePath = "text/" + foldername + "/" + newFileName;
                    entry.Url = filePath;
                }

                if (file.ContentType.Contains("image/"))
                {
                    //set the submission format to match filetype
                    //entry.Format = "image";
                    filePath = "image/" + foldername + "/" + newFileName;                    
                    entry.Url = filePath;
                }

                if (filePath == null)
                {
                    ViewBag.ErrorMessage = "Sorry but we currently don't support the type of file you're trying to upload. Please try closing the file if it is still open and uploading it again. If this doesn't work, please contact us at <a href='mailto:aib@valuescompetition.com?Subject=Issue%20uploading'>aib@valuescompetition.com</a> for support " + file.ContentType;
                    return View(postEntry);
                }

                try
                {
                    using (client = AWSClientFactory.CreateAmazonS3Client(accessKey, secretKey))
                    {
                        PutObjectRequest request = new PutObjectRequest();

                        request.BucketName = "valuescompetition-useruploads";
                        request.CannedACL = S3CannedACL.Private;
                        request.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;
                        request.Key = filePath;
                        request.InputStream = file.InputStream;

                        PutObjectResponse response = client.PutObject(request);
                        //myResponse = response.HttpStatusCode.ToString();
                        if (response.HttpStatusCode.ToString() == "OK")
                        {
                            Context.Entries.Insert(entry);

                            myResponse = "File uploaded & entry submitted.<br/>To view it, please <a href=\"";
                            var callbackUrl = Url.Action("Play", "Home", new { Id = entry.Id });
                            var editUrl = Url.Action("Edit", "Home", new { Id = entry.Id });
                            myResponse = myResponse + callbackUrl + "\">click here</a><hr/>Alternatively, to edit your entry please <a href=\"";
                            myResponse = myResponse + editUrl + "\">click here</a>";
                        }

                    }
                }
                catch (AmazonS3Exception s3Exception)
                {
                    //s3Exception.InnerException
#if(DEBUG)
                    ViewBag.ErrorMessage = s3Exception.Message;
#endif

#if(!DEBUG)
                    if (s3Exception.Message.Contains("x-amz-server-side-encryption header is not supported"))
                    {
                        ViewBag.ErrorMessage =
                            "Sorry but we currently don't support the type of file you're trying to upload. Please contact us at <a href='mailto:aib@valuescompetition.com?Subject=Issue%20uploading'>aib@valuescompetition.com</a> for support";
                    }
                    else
                    {
                        ViewBag.ErrorMessage =
                            "There was a problem uploading your file. Please try again, if this continues to happen please contact us at <a href='mailto:aib@valuescompetition.com?Subject=Issue%20uploading'>aib@valuescompetition.com</a> for support";
                    }
#endif


                    return View(postEntry);
                }

                //Send to SQS for re-encoding
                if (file.ContentType.Contains("video/"))
                {
                    using (AmazonSQSClient sqsClient = new AmazonSQSClient())
                    {
                        try
                        {
                            var json = Newtonsoft.Json.JsonConvert.SerializeObject(entry);

                            SendMessageRequest request = new SendMessageRequest();
                            request.QueueUrl = appConfig["SQSEncodingQueue"];
                            request.MessageBody = json;

                            SendMessageResponse response = sqsClient.SendMessage(request);
                        }
                        catch (AmazonSQSException sqsException)
                        {
#if(DEBUG)
                            ViewBag.ErrorMessage = sqsException.Message;
#endif

#if(!DEBUG)
                        ViewBag.ErrorMessage =
                            "There was a problem editing your video. Please try again, if this continues to happen please contact us at <a href='mailto:aib@valuescompetition.com?Subject=Issue%20re-encoding%20video'>aib@valuescompetition.com</a> for support";
#endif
                        }

                    }
                }


            }

            
            
           

            ViewBag.SuccessMessage = myResponse;
            ViewBag.Uploaded = true;
            return View();
        }

        public ActionResult Entry()
        {
            var entries = Context.Entries.FindAll().Where(x => x.UserName.Contains(User.Identity.Name));

            if (Request.IsAuthenticated && User.IsInRole("Admin"))
            {
                entries = Context.Entries.FindAll();
            }

            if (entries.Count() == 0)
            {
                var myResponse = "You have not entered the competition, please <a href=\"";
                var callbackUrl = Url.Action("Upload", "Home");
                myResponse = myResponse + callbackUrl + "\">click here</a> to upload your entry";
                ViewBag.Message = myResponse;
            }

            

            return View(entries);
        }

        public ActionResult Edit(string id)
        {

            //only give edit access to admin or user who created
            var entry = GetEntry(id);

            if (User.IsInRole("Admin") || entry.UserName == User.Identity.Name)
            {
                return View(entry);
            }

            return RedirectToAction("Entry");
            
        }

        private Entry GetEntry(string id)
        {
            //bugfix Could not find any recognizable digits.
            var max24chars = Utils.Substring.TruncateLongString(id, 24);

            var entry = Context.Entries.FindOneById(new ObjectId(max24chars));
            return entry;
        }

        [HttpPost]
        public ActionResult Edit(string id, Edit editEntry)
        {
            var entry = GetEntry(id);
            if (User.IsInRole("Admin") || entry.UserName == User.Identity.Name)
            {
                entry.Edit(editEntry);
                Context.Entries.Save(entry);
                return RedirectToAction("Index");
            }

            return RedirectToAction("Entry");


        }

        public ActionResult Delete(string id)
        {
            var entry = GetEntry(id);
            if (User.IsInRole("Admin") || entry.UserName == User.Identity.Name)
            {
                Context.Entries.Remove(MongoDB.Driver.Builders.Query.EQ("_id", new ObjectId(id)));
                return RedirectToAction("Entry");
            }

            return RedirectToAction("Entry");
        }
    }
}
