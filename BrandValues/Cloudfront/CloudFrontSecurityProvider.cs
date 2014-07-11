using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using System.Security.Cryptography;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace BrandValues.Cloudfront
{
    public class CloudFrontSecurityProvider
    {

        public static string ToUrlSafeBase64String(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('=', '_')
                .Replace('/', '~');
        }

        public static string CreateCannedPrivateURL(string urlString,
            string durationUnits, string durationNumber, string pathToPolicyStmnt,
            string pathToPrivateKey, string privateKeyId)
        {
            // args[] 0-thisMethod, 1-resourceUrl, 2-seconds-minutes-hours-days 
            // to expiration, 3-numberOfPreviousUnits, 4-pathToPolicyStmnt, 
            // 5-pathToPrivateKey, 6-PrivateKeyId

            TimeSpan timeSpanInterval = GetDuration(durationUnits, durationNumber);

            // Create the policy statement.
            string strPolicy = CreatePolicyStatement(pathToPolicyStmnt,
                urlString,
                DateTime.Now,
                DateTime.Now.Add(timeSpanInterval),
                "0.0.0.0/0");
            if ("Error!" == strPolicy) return "Invalid time frame." +
                "Start time cannot be greater than end time.";

            // Copy the expiration time defined by policy statement.
            string strExpiration = CopyExpirationTimeFromPolicy(strPolicy);

            // Read the policy into a byte buffer.
            byte[] bufferPolicy = Encoding.ASCII.GetBytes(strPolicy);

            // Initialize the SHA1CryptoServiceProvider object and hash the policy data.
            using (SHA1CryptoServiceProvider
                cryptoSHA1 = new SHA1CryptoServiceProvider())
            {
                bufferPolicy = cryptoSHA1.ComputeHash(bufferPolicy);

                // Initialize the RSACryptoServiceProvider object.
                RSACryptoServiceProvider providerRSA = new RSACryptoServiceProvider();
                XmlDocument xmlPrivateKey = new XmlDocument();

                // Load PrivateKey.xml, which you created by converting your 
                // .pem file to the XML format that the .NET framework uses.  
                // Several tools are available. We used  
                // .NET 2.0 OpenSSL Public and Private Key Parser, 
                // http://www.jensign.com/opensslkey/opensslkey.cs.
                xmlPrivateKey.Load(pathToPrivateKey);

                // Format the RSACryptoServiceProvider providerRSA and 
                // create the signature.
                providerRSA.FromXmlString(xmlPrivateKey.InnerXml);
                RSAPKCS1SignatureFormatter rsaFormatter =
                    new RSAPKCS1SignatureFormatter(providerRSA);
                rsaFormatter.SetHashAlgorithm("SHA1");
                byte[] signedPolicyHash = rsaFormatter.CreateSignature(bufferPolicy);

                // Convert the signed policy to URL-safe Base64 encoding and 
                // replace unsafe characters + = / with the safe characters - _ ~
                string strSignedPolicy = ToUrlSafeBase64String(signedPolicyHash);

                // Concatenate the URL, the timestamp, the signature, 
                // and the key pair ID to form the signed URL.
                return urlString +
                    "?Expires=" +
                    strExpiration +
                    "&Signature=" +
                    strSignedPolicy +
                    "&Key-Pair-Id=" +
                    privateKeyId;
            }
        }


        public static string CreatePolicyStatement(string policyStmnt, string resourceUrl,
                               DateTime startTime, DateTime endTime, string ipAddress)
        {
            // Create the policy statement.
            FileStream streamPolicy = new FileStream(policyStmnt, FileMode.Open, FileAccess.Read);
            using (StreamReader reader = new StreamReader(streamPolicy))
            {
                string strPolicy = reader.ReadToEnd();

                TimeSpan startTimeSpanFromNow = (startTime - DateTime.Now);
                TimeSpan endTimeSpanFromNow = (endTime - DateTime.Now);
                TimeSpan intervalStart =
                    (DateTime.UtcNow.Add(startTimeSpanFromNow)) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan intervalEnd =
                    (DateTime.UtcNow.Add(endTimeSpanFromNow)) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                int startTimestamp = (int)intervalStart.TotalSeconds; // START_TIME
                int endTimestamp = (int)intervalEnd.TotalSeconds;  // END_TIME

                if (startTimestamp > endTimestamp)
                    return "Error!";

                // Replace variables in the policy statement.
                strPolicy = strPolicy.Replace("RESOURCE", resourceUrl);
                strPolicy = strPolicy.Replace("START_TIME", startTimestamp.ToString());
                strPolicy = strPolicy.Replace("END_TIME", endTimestamp.ToString());
                strPolicy = strPolicy.Replace("IP_ADDRESS", ipAddress);
                strPolicy = strPolicy.Replace("EXPIRES", endTimestamp.ToString());
                return strPolicy;
            }
        }

        public static TimeSpan GetDuration(string units, string numUnits)
        {
            TimeSpan timeSpanInterval = new TimeSpan();
            switch (units)
            {
                case "seconds":
                    timeSpanInterval = new TimeSpan(0, 0, 0, int.Parse(numUnits));
                    break;
                case "minutes":
                    timeSpanInterval = new TimeSpan(0, 0, int.Parse(numUnits), 0);
                    break;
                case "hours":
                    timeSpanInterval = new TimeSpan(0, int.Parse(numUnits), 0, 0);
                    break;
                case "days":
                    timeSpanInterval = new TimeSpan(int.Parse(numUnits), 0, 0, 0);
                    break;
                default:
                    Console.WriteLine("Invalid time units; use seconds, minutes, hours, or days");
                    break;
            }
            return timeSpanInterval;
        }

        private static TimeSpan GetDurationByUnits(string durationUnits, string startIntervalFromNow)
        {
            TimeSpan timeSpanInterval = new TimeSpan();
            switch (durationUnits)
            {
                case "seconds":
                    timeSpanInterval = new TimeSpan(0, 0, int.Parse(startIntervalFromNow));
                    break;
                case "minutes":
                    timeSpanInterval = new TimeSpan(0, int.Parse(startIntervalFromNow), 0);
                    break;
                case "hours":
                    timeSpanInterval = new TimeSpan(int.Parse(startIntervalFromNow), 0, 0);
                    break;
                case "days":
                    timeSpanInterval = new TimeSpan(int.Parse(startIntervalFromNow), 0, 0, 0);
                    break;
                default:
                    timeSpanInterval = new TimeSpan(0, 0, 0, 0);
                    break;
            }
            return timeSpanInterval;
        }

        public static string CopyExpirationTimeFromPolicy(string policyStatement)
        {
            int startExpiration = policyStatement.IndexOf("EpochTime");
            string strExpirationRough = policyStatement.Substring(startExpiration + "EpochTime".Length);
            char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            List<char> listDigits = new List<char>(digits);
            StringBuilder buildExpiration = new StringBuilder(20);
            foreach (char c in strExpirationRough)
            {
                if (listDigits.Contains(c))
                    buildExpiration.Append(c);
            }
            return buildExpiration.ToString();
        }

    }
}