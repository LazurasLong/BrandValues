using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BrandValuesEmailProcessor
{
    class Program
    {
        private static string AccessKeyID = "AKIAINIQRUHQ6IHTDS3A";
        private static string SecretAccessKeyID = "VtkJmp8Lgi9kCKGbICCaUIGzdl5nqsN0CIeuLCm6";
       

        static void Main(string[] args)
        {
            //get HTML template
            string dir = System.IO.Path.GetDirectoryName(
      System.Reflection.Assembly.GetExecutingAssembly().Location);
            StreamReader reader = new StreamReader(dir + "/EmailTemplate.html");
            string HTMLTemplate = reader.ReadToEnd();


            Console.WriteLine("Checking for SQS messages");

            ConsoleKeyInfo cki = new ConsoleKeyInfo();

            //create client
            AmazonSQSClient client = new AmazonSQSClient(AccessKeyID, SecretAccessKeyID, Amazon.RegionEndpoint.EUWest1);


            //loop
            do
            {
                while (Console.KeyAvailable == false)
                {
                    //call polling
                    PollQueue(client, HTMLTemplate);

                    Thread.Sleep(1000); //loop until input
                }
                cki = Console.ReadKey(true);

            } while (cki.Key != ConsoleKey.X); //stop if X pressed

            Console.WriteLine("Shutting down..");
        }

        private static void PollQueue(AmazonSQSClient client, string htmlTemplate)
        {
            Console.WriteLine("Polling queue..");

            ReceiveMessageRequest request = new ReceiveMessageRequest();
            request.AttributeNames = new List<string>() { "All" };
            request.MaxNumberOfMessages = 10;
            request.QueueUrl = "https://sqs.eu-west-1.amazonaws.com/122985306253/valuescomp-email";

            //create and hold results
            List<Message> queueMessages = new List<Message>();

            //receive messages if any
            ReceiveMessageResponse response = client.ReceiveMessage(request);

            if (response.ReceiveMessageResult.Messages.Count != 0)
            {
                //get list of queue messages
                queueMessages = response.ReceiveMessageResult.Messages;
                Console.WriteLine("Queue messages received; count is " + queueMessages.Count.ToString());

                //loop through each message that comes back
                foreach (Message m in queueMessages)
                {

                    JToken token = JObject.Parse(m.Body);
                    string email = (string)token.SelectToken("email");
                    string name = (string)token.SelectToken("name");
                    string url = (string)token.SelectToken("url");
                    string subject = (string)token.SelectToken("subject");

                    if (subject == "Welcome")
                    {
                        StringBuilder htmlBuilder = new StringBuilder();
                        htmlBuilder.Append("<p>");
                        htmlBuilder.Append("To get started, please confirm your account by clicking this <a href='");
                        htmlBuilder.Append(url);
                        htmlBuilder.Append("' target='_blank'>link</a>");

                        var bodyHtml = htmlTemplate;

                        bodyHtml = bodyHtml.Replace("{Name}", name);
                        bodyHtml = bodyHtml.Replace("{Url}", htmlBuilder.ToString());

                        subject = "Welcome to the Competition";

                        Send(bodyHtml, email, subject, client, request, m);
                    }


                    //Delete all messages
                    //client.DeleteMessage(m.ReceiptHandle);

                    //client.DeleteMessage(new DeleteMessageRequest() { QueueUrl = request.QueueUrl, ReceiptHandle = m.ReceiptHandle });
                }


            }

        }

        private static async void Send(string message, string userEmail, string subject, AmazonSQSClient client, ReceiveMessageRequest request, Message m)
        {

            MailMessage email = new MailMessage();

            email.From = new MailAddress("aib@valuescompetition.com", "AIB Brand Values Competition");
            email.Subject = subject;

            email.IsBodyHtml = true;

            email.Body = message;

            email.To.Add(new MailAddress(userEmail));
            
            bool result = await SendMailInSeperateThread(email);

            if (result == false)
            {
                Console.WriteLine("Error sending email to - " + userEmail);
            }
            else
            {
                //delete from queue
                client.DeleteMessage(new DeleteMessageRequest() { QueueUrl = request.QueueUrl, ReceiptHandle = m.ReceiptHandle });
            }

        }

        private static async Task<bool> SendMailInSeperateThread(MailMessage message)
        {

            try
            {
                SmtpClient client = new SmtpClient();
                client.Timeout = 20000; // 20 second timeout... why more?

                // Connecting to the server and configuring it

                client.Host = "email-smtp.eu-west-1.amazonaws.com";
                client.Port = 587;
                client.EnableSsl = true;

                    // The server requires user's credentials
                    // not the default credentials
                    client.UseDefaultCredentials = false;
                    // Provide your credentials
                    client.Credentials = new System.Net.NetworkCredential("AKIAJGCED5UQT5EBPDYQ", "AkUK2pScW+v4ewGFLMe/P07btl3/UeFOybO72R5jUL3x");

                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                await client.SendMailAsync(message);
                client.Dispose();
                message.Dispose();

            }
            catch (Exception ex)
            {
                // This is very necessary to catch errors since we are in
                // a different context & thread
                //Elmah.ErrorLog.GetDefault(null).Log(new Error(ex));

                Console.WriteLine("Error Sending emails: " + ex);
                return false;
            }

            return true;


        }



    }
}
