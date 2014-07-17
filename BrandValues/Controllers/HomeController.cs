using System.Web;
using System.Web.Mvc;
using System.Linq;
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


namespace BrandValues.Controllers {

    [Authorize]
    public class HomeController : Controller {

        public readonly BrandValuesContext Context = new BrandValuesContext();

        NameValueCollection appConfig = ConfigurationManager.AppSettings;
        //private static string adminAccessKey = appConfig["AWSAccessKey"];
        //private static string adminAccessSecret = appConfig["AWSAccessKey"];
        //AmazonS3Config config = new AmazonS3Config()
        //{
        //    ServiceURL = "https://s3.amazonaws.com"
        //};

        public ActionResult Index()
        {
            //var user = User.Identity.Name;

            //Test Mongo Access
            //Context.Database.GetStats();
            //return Json(Context.Database.Server.BuildInfo, JsonRequestBehavior.AllowGet);

            var entries = Context.Entries.FindAll();

            var model = from r in entries
                orderby r.Likes.Count() descending
                select r;


            return View(model);
        }


        public ActionResult Play(string id)
        {
            if (id.IsNullOrWhiteSpace())
            {
                return RedirectToAction("Index");
            }


            //get Entry first
            var entry = GetEntry(id);

            //get video for entry
            //RTMP resources do not take the form of a URL, 
            //and instead the resource path is nothing but the stream's name. e.g. "video1.mp4"
            if (entry.Format == "video")
            {
                ViewBag.RTMPUrl = GetRTMPCloudfrontUrl(entry);
                ViewBag.AppleUrl = GetAppleCloudFrontUrl(entry);
                ViewBag.FallbackUrl = GetFallbackMP4CloudFrontUrl(entry);
                ViewBag.VideoThumbnailUrl = GetVideoThumbnailUrl(entry);
            } else {
                ViewBag.CloudFrontUrl = GetCloudFrontUrl(entry);
            }       

            return View(entry);
        }

        private string GetRTMPCloudfrontUrl(Entry entry)
        {
            string rtmpUrl = appConfig["RTMPCloudfront"];
            string videoUrl = entry.Url;
            string signedVideoUrl = GetSignedUrl.GetCloudfrontUrl(videoUrl);
            return rtmpUrl + signedVideoUrl;
        }

        private string GetAppleCloudFrontUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["TranscoderCloudfront"];
            string relativeUrl = Path.GetFileNameWithoutExtension(entry.Url);
            string url = cloudfrontUrl + relativeUrl + ".m3u8";
            return GetSignedUrl.GetCloudfrontUrl(url);
        }

        private string GetFallbackMP4CloudFrontUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["TranscoderCloudfront"];
            string relativeUrl = entry.Url;
            string url = cloudfrontUrl + relativeUrl;
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


        public ActionResult Upload()
        {
            //ViewBag.Message = "Test";
            //PostEntry postEntry = new PostEntry();

            //GetValuesList(postEntry);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(PostEntry postEntry, HttpPostedFileBase[] files)
        {
            var myResponse = "";

            postEntry.UserName = User.Identity.Name;
            postEntry.UserArea = "Coo";
            
            if (!ModelState.IsValid)
            {
                return View();
            }

            //var user = UserManager.FindByName(User.Identity.Name);

            var entry = new Entry(postEntry);

            entry.UserName = User.Identity.Name;
            entry.CreatedOn = DateTime.UtcNow;
//            entry.UserArea = User.Identity.

            //return RedirectToAction("Index");

            foreach (var file in files)
            {
                if (file == null)
                {
                    ViewBag.Message = "Please select a file to upload";
                    return View();
                }

                string accessKey = appConfig["S3AWSAccessKey"];
                string secretKey = appConfig["S3AWSSecretKey"];


                IAmazonS3 client;
                var filePath = "";

                //remove spaces
                var newFileName = file.FileName.Replace(" ", String.Empty);

                var foldername = Path.GetFileNameWithoutExtension(newFileName);

                if (file.ContentType.Contains("video/"))
                {
                    filePath = "video/" + foldername + "/" + newFileName;
                    entry.VideoThumbnailUrl = "video/" + foldername + "/" + foldername + "-00001.png";
                    entry.ThumbnailUrl = "images/entries/video.png";
                    entry.Url = filePath;
                }

                if (file.ContentType.Contains("text/") || 
                    file.ContentType.Contains("application/pdf") || 
                    file.ContentType.Contains("application/msword") || 
                    file.ContentType.Contains("application/vnd.ms-powerpoint") ||
                    file.ContentType.Contains("application/vnd.openxmlformats-officedocument")
                    )
                {
                    filePath = "text/" + foldername + "/" + newFileName;
                    entry.ThumbnailUrl = "/images/entries/document.png";
                    entry.Url = filePath;
                }

                if (file.ContentType.Contains("image/"))
                {
                    filePath = "image/" + foldername + "/" + newFileName;
                    entry.ThumbnailUrl = "/images/entries/photo.png";
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
            }

            
            
           

            ViewBag.Message = myResponse;
            ViewBag.Uploaded = true;
            return View();
        }

        public ActionResult Entry()
        {
            var entries = Context.Entries.FindAll();
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
