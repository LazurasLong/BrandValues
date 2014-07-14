using System.Web;
using System.Web.Mvc;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using System.Drawing.Imaging;
using Amazon;
using System.IO;
using System.Drawing;
using System.Collections.Specialized;
using System.Configuration;
using BrandValues.Cloudfront;


namespace BrandValues.Controllers {

    [Authorize]
    public class HomeController : Controller {

        NameValueCollection appConfig = ConfigurationManager.AppSettings;
        //private static string adminAccessKey = appConfig["AWSAccessKey"];
        //private static string adminAccessSecret = appConfig["AWSAccessKey"];
        //AmazonS3Config config = new AmazonS3Config()
        //{
        //    ServiceURL = "https://s3.amazonaws.com"
        //};

        public ActionResult Index()
        {
            var user = User.Identity.Name;

            return View("Index", "", user);
        }

        public ActionResult Play()
        {
            string testUrl = GetSignedUrl.GetUrl("https://d3o104q4nbxlx6.cloudfront.net/video/image.jpg", 5 * 60);

            //string url = GetSignedUrl.GetUrl("rtmp://sbw4t54bzxsgi.cloudfront.net/cfx/st/mp4:test1.mp4", 5 * 60);
            string url = GetSignedUrl.GetUrl("rtmp://sbw4t54bzxsgi.cloudfront.net/cfx/test1/test1.mp4", 5 * 60);

            string customURL = GetSignedUrl.GetCustomUrl("rtmp://sbw4t54bzxsgi.cloudfront.net/cfx/st/mp4:test1.mp4", 5*60);

            ViewBag.Message = url;
            ViewBag.Test = testUrl;

            return View("Play");
        }


        public ActionResult Upload()
        {
            return View("Upload");
        }

        public class ViewDataUploadFilesResult
        {
            public string Name { get; set; }
            public int Length { get; set; }
        }

        [HttpPost]
        public ActionResult UploadFiles(HttpPostedFileBase[] files)
        {
            var myResponse = "";

            foreach (HttpPostedFileBase file in files)
            {

                if (file.ContentLength > 0)
                {
                   
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
            }

            ViewBag.Message = myResponse;
            return View("Upload");
        }

    }
}
