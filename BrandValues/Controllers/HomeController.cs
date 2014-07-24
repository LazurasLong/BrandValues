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

            //var model = from r in entries
            //            orderby r.CreatedOn descending
            //            select r;


            if (version == "Version2")
            {
                return View("V2", allDocs);
            }

            return View(allDocs);

        }

        //cache
        [OutputCache(Duration = 600)]
        public ActionResult Play(string id)
        {
            if (id.IsNullOrWhiteSpace())
            {
                return RedirectToAction("Index");
            }

            //return last 6 entries
            SortByBuilder sbb = new SortByBuilder();
            sbb.Descending("CreatedOn");
            var allDocs = Context.Entries.FindAllAs<Entry>().SetSortOrder(sbb).SetLimit(2);

            //get Entry first
            var entry = GetEntry(id);

            //get video for entry
            //RTMP resources do not take the form of a URL, 
            //and instead the resource path is nothing but the stream's name. e.g. "video1.mp4"
            if (entry.Format == "video")
            {
                ViewBag.RTMPUrl = GetRTMPCloudfrontUrl(entry);
                //ViewBag.AppleUrl = GetAppleCloudFrontUrl(entry);
                ViewBag.FallbackUrl = GetFallbackMP4CloudFrontUrl(entry);
                ViewBag.VideoThumbnailUrl = GetVideoThumbnailUrl(entry);
            } else {
                ViewBag.CloudFrontUrl = GetCloudFrontUrl(entry);
            }       

            PlayViewModel playViewModel = new PlayViewModel();
            playViewModel.Entry = entry;
            playViewModel.Entries = allDocs;

            return View(playViewModel);
        }

        private string GetRTMPCloudfrontUrl(Entry entry)
        {
            string rtmpUrl = appConfig["RTMPCloudfront"];
            //string videoUrl = entry.Url;
            string fileName = Path.GetFileNameWithoutExtension(entry.Url);
            string videoUrl = fileName + ".mp4";
            string signedVideoUrl = GetSignedUrl.GetCloudfrontUrl(videoUrl);
            return rtmpUrl + signedVideoUrl;
        }

        private string GetAppleCloudFrontUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["TranscoderCloudfront"];
            string fileName = Path.GetFileNameWithoutExtension(entry.Url);
            string relativeUrl = entry.Url.Substring(0, entry.Url.LastIndexOf('/'));
            string url = cloudfrontUrl + relativeUrl + "/" + fileName + ".m3u8";
            return GetSignedUrl.GetCloudfrontUrl(url);
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

        private string GetVideoThumbnailUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["TranscoderCloudfront"];
            string relativeUrl = entry.VideoThumbnailUrl;
            string url = cloudfrontUrl + relativeUrl;
            return GetSignedUrl.GetCloudfrontUrl(url);
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
        public async Task<ActionResult> Upload(PostEntry postEntry, HttpPostedFileBase[] files)
        {
            //positive outcome response
            var myResponse = "";
            
            //check that model is valid
            if (!ModelState.IsValid)
            {
                return View();
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
            if (postEntry.Type == "team" && postEntry.TeamName.IsEmpty())
            {
                ViewBag.Message = "Please enter your team name";
                return View();
            }

            //set date
            entry.CreatedOn = DateTime.UtcNow;

            foreach (var file in files)
            {
                if (file == null)
                {
                    ViewBag.Message = "Please select a file to upload";
                    return View();
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

                var entryName = entry.Name.Replace(" ", String.Empty);
                var newFileName = entryName + Path.GetExtension(file.FileName);

                var foldername = username;
                    //Path.GetFileNameWithoutExtension(newFileName);


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
                    entry.Format = "text";
                    filePath = "text/" + foldername + "/" + newFileName;
                    entry.ThumbnailUrl = siteFilesCloudfront + "/entries/document.png";
                    entry.Url = filePath;
                }

                if (file.ContentType.Contains("image/"))
                {
                    //set the submission format to match filetype
                    entry.Format = "image";
                    filePath = "image/" + foldername + "/" + newFileName;
                    entry.ThumbnailUrl = siteFilesCloudfront + "/entries/photo.png";
                    entry.Url = filePath;
                }

                if (filePath == null)
                {
                   ViewBag.Message = "Sorry but we currently don't support the type of file you're trying to upload. Please contact us at <a href='mailto:aib@valuescompetition.com?Subject=Issue%20uploading'>aib@valuescompetition.com</a> for support";
                  return View(); 
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

                            myResponse = "File uploaded & entry submitted. Awesome. <br/> To view it, please <a href=\"";
                            var callbackUrl = Url.Action("Play", "Home", new { Id = entry.Id });
                            myResponse = myResponse + callbackUrl + "\">click here</a>";                          
                        }

                    }
                }
                catch (AmazonS3Exception s3Exception)
                {
                    //s3Exception.InnerException
#if(DEBUG)
                    ViewBag.Message = s3Exception.Message;
#endif

#if(!DEBUG)
                    if (s3Exception.Message.Contains("x-amz-server-side-encryption header is not supported"))
                    {
                        ViewBag.Message =
                            "Sorry but we currently don't support the type of file you're trying to upload. Please contact us at <a href='mailto:aib@valuescompetition.com?Subject=Issue%20uploading'>aib@valuescompetition.com</a> for support";
                    }
                    else
                    {
                        ViewBag.Message =
                            "There was a problem uploading your file. Please try again, if this continues to happen please contact us at <a href='mailto:aib@valuescompetition.com?Subject=Issue%20uploading'>aib@valuescompetition.com</a> for support";
                    }
#endif


                    return View("Upload");
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
                            ViewBag.Message = sqsException.Message;
#endif

#if(!DEBUG)
                        ViewBag.Message =
                            "There was a problem editing your video. Please try again, if this continues to happen please contact us at <a href='mailto:aib@valuescompetition.com?Subject=Issue%20re-encoding%20video'>aib@valuescompetition.com</a> for support";
#endif
                        }

                    }
                }


            }

            
            
           

            ViewBag.Message = myResponse;
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

        public ActionResult Browse(EntryFilter filters)
        {
            var entries = FilterRentals(filters);
            var model = new EntryList
            {
                Entries = entries,
                Filters = filters
            };
            return View(model);
        }

        private MongoCursor<Entry> FilterRentals(EntryFilter filters)
        {
            if (filters.TypeFilter.IsNullOrWhiteSpace())
            {
                return Context.Entries.FindAll();
            }
            var query = MongoDB.Driver.Builders.Query<Entry>.EQ(r => r.Type, filters.TypeFilter);
            return Context.Entries.Find(query);
        }

        public ActionResult Edit(string id)
        {
            var entry = GetEntry(id);


            return View(entry);
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
            entry.Edit(editEntry);
            Context.Entries.Save(entry);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(string id)
        {
            Context.Entries.Remove(MongoDB.Driver.Builders.Query.EQ("_id", new ObjectId(id)));
            return RedirectToAction("Entry");
        }
    }
}
