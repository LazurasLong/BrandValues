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

            //var entries = Context.Entries.FindAll().SetLimit(6);

            //return last 6 entries
            SortByBuilder sbb = new SortByBuilder();
            sbb.Descending("CreatedOn");
            var allDocs = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb).SetLimit(6);

            //get menu
            ViewBag.Menu = GetMenu();

            ViewBag.Voting = GetVotingStatus();

            if (version == "Version2")
            {
                return View("HomePageV2", allDocs);
            }

            return View("HomePageV1", allDocs);

        }

        public List<SelectListItem> GetMenu()
        {
            //dropdown menu
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "Home", Value = "0" });

            items.Add(new SelectListItem { Text = "How the competition works", Value = "1" });

            items.Add(new SelectListItem { Text = "What prizes are up for grabs?", Value = "2" });

            items.Add(new SelectListItem { Text = "A reminder of the AIB brand values", Value = "3" });

            items.Add(new SelectListItem { Text = "What does a winning entry look like?", Value = "4" });

            items.Add(new SelectListItem { Text = "All your questions answered…", Value = "5" });

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

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_Search", entries);
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
            var allDocs = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb).SetLimit(6);

            ViewBag.Voting = GetVotingStatus();

            switch (selection)
            {
                case 0:
                    ViewBag.Voting = GetVotingStatus();
                    return PartialView("_Intro", allDocs);
                case 1:
                    return PartialView("_HowItWorks");
                case 2:
                    return PartialView("_Prizes");
                case 3:
                    return PartialView("_BrandValues");
                case 4:
                    if(IpAddress.CheckIp()) {
                        ViewBag.NetworkPC = true;
                    }
                    else
                    {
                        ViewBag.NetworkPC = false;
                    }
                    
                    return PartialView("_WinningEntry");
                case 5:
                    return PartialView("_FAQ");
            }

            return PartialView("_Intro", allDocs);
        }

        public PartialViewResult Intro()
        {
            SortByBuilder sbb = new SortByBuilder();
            sbb.Descending("CreatedOn");
            var allDocs = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb).SetLimit(6);
            ViewBag.Voting = GetVotingStatus();
            return PartialView("_Intro", allDocs);
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
        public PartialViewResult All()
        {
            SortByBuilder sbb = new SortByBuilder();
            sbb.Descending("CreatedOn");
            var entries = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb);

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", entries);
        }

        public PartialViewResult Customers()
        {
            var searchTerm = "customers";
            var search = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.Values, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var searchQuery =
                MongoDB.Driver.Builders.Query.Or(
                    search
                );

            var entries = Context.Entries
                .FindAs<Entry>(searchQuery)
                .SetSortOrder(SortBy<Entry>.Descending(g => g.CreatedOn));

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", entries);
        }

        public PartialViewResult Empowering()
        {
            var searchTerm = "empowering";
            var search = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.Values, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var searchQuery =
                MongoDB.Driver.Builders.Query.Or(
                    search
                );

            var entries = Context.Entries
                .FindAs<Entry>(searchQuery)
                .SetSortOrder(SortBy<Entry>.Descending(g => g.CreatedOn));

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", entries);
        }

        public PartialViewResult Trust()
        {
            var searchTerm = "trust";
            var search = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.Values, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var searchQuery =
                MongoDB.Driver.Builders.Query.Or(
                    search
                );

            var entries = Context.Entries
                .FindAs<Entry>(searchQuery)
                .SetSortOrder(SortBy<Entry>.Descending(g => g.CreatedOn));

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", entries);
        }

        public PartialViewResult Simple()
        {
            var searchTerm = "simple";
            var search = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.Values, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var searchQuery =
                MongoDB.Driver.Builders.Query.Or(
                    search
                );

            var entries = Context.Entries
                .FindAs<Entry>(searchQuery)
                .SetSortOrder(SortBy<Entry>.Descending(g => g.CreatedOn));

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", entries);
        }


        public PartialViewResult Together()
        {
            var searchTerm = "together";
            var search = MongoDB.Driver.Builders.Query<Entry>.Matches(g => g.Values, BsonRegularExpression.Create(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            var searchQuery =
                MongoDB.Driver.Builders.Query.Or(
                    search
                );

            var entries = Context.Entries
                .FindAs<Entry>(searchQuery)
                .SetSortOrder(SortBy<Entry>.Descending(g => g.CreatedOn));

            ViewBag.Voting = GetVotingStatus();

            return PartialView("_DisplayEntries", entries);
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
            var allDocs = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb).SetLimit(4);

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
            ViewBag.Voting = false;
            //check for vote
            ViewBag.Voted = false;
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

            //check for like
            ViewBag.Liked = false;
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

        public ActionResult Browse()
        {
            SortByBuilder sbb = new SortByBuilder();
            sbb.Descending("CreatedOn");
            var model = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb);
            ViewBag.Voting = GetVotingStatus();

            return View(model);
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
            var entry = Context.Entries.FindOneById(new ObjectId(id));
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
