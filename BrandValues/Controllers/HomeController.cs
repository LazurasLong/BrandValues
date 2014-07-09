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
                   NameValueCollection appConfig = ConfigurationManager.AppSettings;
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
