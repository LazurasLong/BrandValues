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
using BrandValues.Cloudfront;
using BrandValues.App_Start;
using BrandValues.Entries;
using Microsoft.Ajax.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using Amazon.CloudFront;


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


            return View(entries);
        }



        public ActionResult Play(string id)
        {
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
            } else {
                ViewBag.CloudFrontUrl = GetCloudFrontUrl(entry);
            }

            ViewBag.Thumbnail = GetCloudFrontUrl(entry);
            

            return View(entry);
        }

        private string GetRTMPCloudfrontUrl(Entry entry)
        {
            string rtmpUrl = appConfig["RTMPCloudfront"];
            string videoUrl = entry.Url + ".mp4";
            string signedVideoUrl = GetSignedUrl.GetCloudfrontUrl(videoUrl);
            return rtmpUrl + signedVideoUrl;
        }

        private string GetAppleCloudFrontUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["TranscoderCloudfront"];
            string relativeUrl = entry.Url;
            string url = cloudfrontUrl + relativeUrl + ".m3u8";
            return GetSignedUrl.GetCloudfrontUrl(url);
        }

        private string GetFallbackMP4CloudFrontUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["TranscoderCloudfront"];
            string relativeUrl = entry.Url;
            string url = cloudfrontUrl + relativeUrl + ".mp4";
            return GetSignedUrl.GetCloudfrontUrl(url);
        }

        private string GetCloudFrontUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["UploadCloudfront"];
            string relativeUrl = entry.Url;
            string url = cloudfrontUrl + relativeUrl;
            return GetSignedUrl.GetCloudfrontUrl(url);
        }

        private string GetThumbnailUrl(Entry entry)
        {
            string cloudfrontUrl = appConfig["TranscoderCloudfront"];
            string relativeUrl = entry.ThumbnailUrl;
            string url = cloudfrontUrl + relativeUrl;
            return GetSignedUrl.GetCloudfrontUrl(url);
        }


        public ActionResult Upload()
        {
            return View("Upload");
        }

        [HttpPost]
        public ActionResult Upload(PostEntry postEntry, HttpPostedFileBase[] files)
        {
            var myResponse = "";
            
            if (!ModelState.IsValid)
            {
                return View();
            }

            var entry = new Entry(postEntry);
            Context.Entries.Insert(entry);

            //return RedirectToAction("Index");

            

            foreach (HttpPostedFileBase file in files)
            {

                if (file == null)
                {
                    ViewBag.Message = "Please select a file to upload";
                    return View();
                }

                string accessKey = appConfig["S3AWSAccessKey"];
                    string secretKey = appConfig["S3AWSSecretKey"];


                    IAmazonS3 client;

                    var filePath = "video/" + file.FileName;

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
                            myResponse = response.HttpStatusCode.ToString();
                        }
                    }
                    catch (AmazonS3Exception s3Exception)
                    {
                        //s3Exception.InnerException
                        ViewBag.Message = s3Exception.Message;
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
