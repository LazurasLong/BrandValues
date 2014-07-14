using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using Amazon;
using Amazon.CloudFront.Model;
using Amazon.S3;
using Amazon.S3.Model;

namespace BrandValues.Cloudfront
{
    public class GetSignedUrl
    {
        

        //test url feed
        //http://d1k5ny0m6d4zlj.cloudfront.net/diag/CFStreamingDiag.html

        //
        //http://improve.dk/how-to-set-up-and-serve-private-content-using-s3/

        //http://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/private-content-trusted-signers.html
        public static string GetUrl(string url, int time)
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;
            string keyPairID = appConfig["keyPairId"];
            string secretKey = @"<RSAKeyValue><Modulus>v/Lv61ioOS9pZ7HYdg16Qyott4TQ2gqxxmqe9v45ar8tnuHAQD0B7YqUPO
                                cX/n6zBRTDF/0mooTKnHsAaN+V2YOoieHLdwc6ZPlmmzOT4bITgmj8TjzTZCPqNfczA2oRUQT8AOijoi
                                CRQokrbSlzasQ3y+OkeivrhDcjYH8tc2HC1vzHHPJAZop5WkmyaEFijssLmQWxbmIKw4OR13gXsN+GEZ
                                ufZecupFO1hNh/NX/vTTBOr4s4N7oOSRwthZYOUNL+dPSEHzIk1abueSrZet7DGnr6q1XabDZtmO6wJs
                                wmUmp1hfhpC2rjZeL8gibhirT9jAsMPSFMF8cXJyN4dQ==</Modulus><Exponent>AQAB</Exponent
                                ><P>57alnj7BHrqVC0/avN/OQYhYIJRXcP0RC5TKD041vhTPAd4brJxIZlbcRnebdiSBPu4i0yQ29pLh
                                iiqI78lcYJUpeE8d34QZRzjHufqY15jseXjL1cVmfXvBZlgNKqyRQ9nxIHiiLJm7aOASHkcs87C7ceQl
                                NJ7AsUb5kEZYhHM=</P><Q>1BFTkNK8vYvwsg1qj8YPAfTdbVE8spo0GBOrBcqLVjxSX/8V/QsylImSU
                                9TGW+Zg4eGxeyMh3An+IiPj9lLwOPOR7ZKZui4U/+HXhzA2RzqqcBMmG4AJL0fuKAIh1SKGAKE2EYNXC
                                M49bOY+kKxJcobDtcIiybYD7yU6PuS3vXc=</Q><DP>YHc1tA/ELprW+gKgugkiwT2WEtovHSb+NagYB
                                w80aJIZLFnfg5j8uz5mLiAVZLYF6METVnu5NiYoJ3FV2R4niz4OKbwmX6uLKo+E9vRRNVDBAkE7+x9Xb
                                95ZwdXehWmagHj3XeFbAt5tm1fNszVGjZ2CLAQjSSjeZ+94bowaxek=</DP><DQ>VxBN+JUsO71qMg9k
                                AmbT3n/DMJA7lS/N11yryBLhpaPaReMaCetA9s6MLrKaRTyN5e6cyOshiY7osOd/Pa/LQ/ZroNehGVi1
                                8l+r9qqKfi/NrXmPGZc1Lh4f3nkRnskvrq6A3ivH3aueeEkGwqqY9NXzH3n2gk2hy9rC0PNho/0=</DQ
                                ><InverseQ>eTp35lT9wwCHRGRvXM8icqpLVH7hkxoGNNjVq0NtBc+oZGEQqjOQAAM8qXy2izb5sZfX7
                                oGznzhFZ6rTn4yBmsYhLtAc24uVpafMiEeNiuYBIRj810AuToXQnIZvqMh3I7LlJruxwoFQHbt4kWx1o
                                q5IJ+Of+38etVR2WxV2F3g=</InverseQ><D>e4KcsnkXtcQ2NM0RFdVSSImJZT8HDGY5Qg01QIUQ+Qk
                                PF973T95xsSGshQvGOrHYC3rylisEgyqjHNFg0BqeV5oKBr7Np0d1YafBSDiF/YISX6WiQ82L0DHz8Mm
                                hf57uiY2FgigvspD2JzQQR2uWoqqjpIRUP3CnWSoJj3wGCOlPYW/Uk/pOuvOQ6xu7RrwZ6byhZ6e3tyU
                                X6CcISceFWkQFE9zijaceH/z56PkkL/ne9ncOo/1Zt6yxwxaqAquLg3z7ZjKaPY9xS3mDIJqRnjXzXW3
                                8KB9WXcx7NesuUyFG+LaFbPRTi+ZFsdn4IcqZcE+B1RgPiOaoZ/ybIy5RuQ==</D></RSAKeyValue>";
            var provider = CloudFrontSecurityProvider.CreateCannedPrivateURL(
                url, 
                "minutes", 
                "5", 
                "F:/GitHub BrandValues/BrandValues/BrandValues/Cloudfront/CannedPolicy.txt",
                "F:/GitHub BrandValues/BrandValues/BrandValues/Cloudfront/PrivateKey.xml", 
                keyPairID);
            //var signedUrl = provider.GetCustomUrl(url, DateTime.Now.AddMinutes(time));

            return provider;
        }

        public static string GetCustomUrl(string url, int time)
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;
            string keyPairID = appConfig["keyPairId"];
            var provider = CloudFrontSecurityProvider.CreateCustomPrivateURL(
                url,
                "minutes",
                "5",
                "0",
                GetIPAddress(),
                "F:/GitHub BrandValues/BrandValues/BrandValues/Cloudfront/CustomPolicy.txt",
                "F:/GitHub BrandValues/BrandValues/BrandValues/Cloudfront/PrivateKey.xml",
                keyPairID);
            //var signedUrl = provider.GetCustomUrl(url, DateTime.Now.AddMinutes(time));

            return provider;
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