using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using BrandValues.Entries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BrandValuesProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Checking for SQS messages");

            ConsoleKeyInfo cki = new ConsoleKeyInfo();

            string AccessKeyID = "AKIAINIQRUHQ6IHTDS3A";
            string SecretAccessKeyID = "VtkJmp8Lgi9kCKGbICCaUIGzdl5nqsN0CIeuLCm6";

            //create client
            AmazonSQSClient client = new AmazonSQSClient(AccessKeyID, SecretAccessKeyID, Amazon.RegionEndpoint.EUWest1);

            //loop
            do
            {
                while (Console.KeyAvailable == false)
                {
                    //call polling
                    PollQueue(client);

                    Thread.Sleep(10000); //loop until input
                }
                cki = Console.ReadKey(true);

            } while (cki.Key != ConsoleKey.X); //stop if X pressed

            Console.WriteLine("Shutting down..");

        }

        private static void PollQueue(AmazonSQSClient client)
        {
            Console.WriteLine("Polling queue..");

            ReceiveMessageRequest request = new ReceiveMessageRequest();
            request.AttributeNames = new List<string>() { "All" };
            request.MaxNumberOfMessages = 10;
            request.QueueUrl = "https://sqs.eu-west-1.amazonaws.com/122985306253/valuescomp-encoding";

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
                    Entry entry = JsonConvert.DeserializeObject<Entry>(m.Body);

                    Console.WriteLine(entry.VideoThumbnailUrl);

                    //Delete all messages
                    //client.DeleteMessage(m.ReceiptHandle);

                    client.DeleteMessage(new DeleteMessageRequest() { QueueUrl = request.QueueUrl, ReceiptHandle = m.ReceiptHandle });
                }


            }

        }
    }
}
