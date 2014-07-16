using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Amazon.S3;
using Amazon.S3.Model;

namespace BrandValues.Cloudfront
{
    public class GetSignedUrl
    {
        

        //test url feed
        //http://d1k5ny0m6d4zlj.cloudfront.net/diag/CFStreamingDiag.html
        //http://www.jwplayer.com/wizard/

        //http://docs.aws.amazon.com/sdkfornet/latest/apidocs/Index.html

        //http://improve.dk/how-to-set-up-and-serve-private-content-using-s3/
        //http://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/private-content-trusted-signers.html


        public static string GetCloudfrontUrl(string key)
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;
            string keyPairID = appConfig["keyPairId"];

            //get Private Key from server path
            StreamReader secretKeyStream = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(@"~/Cloudfront/pk-APKAJWFKSJRPHR2V45EA.pem"));

            //string domain = "localhost";

            string file = key;

            return AmazonCloudFrontUrlSigner.SignUrlCanned(key,
                keyPairID, secretKeyStream, DateTime.Now.AddDays(1));

            //return AmazonCloudFrontUrlSigner.GetCannedSignedURL(AmazonCloudFrontUrlSigner.Protocol.https, domain, secretKeyStream, file, keyPairID, DateTime.Now.AddDays(1));
        }

        public static string GetS3Object(string bucket, string key)
        {
            string accessKeyId = ConfigurationManager.AppSettings["AWSAccessKey"];
            string secretAccessKeyId = ConfigurationManager.AppSettings["AWSSecretKey"];
            using (IAmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKeyId, secretAccessKeyId))
            {
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest();
                request.BucketName = bucket;
                request.Key = key;
                request.Expires = DateTime.Now.Add(new TimeSpan(7, 0, 0, 0));
                return client.GetPreSignedURL(request);
            }
        }

        protected static string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

    }
}