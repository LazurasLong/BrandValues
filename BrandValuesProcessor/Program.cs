using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.ElasticTranscoder;
using Amazon.ElasticTranscoder.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using BrandValues.Entries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BrandValuesProcessor
{
    class Program
    {
        private static string AccessKeyID = "AKIAINIQRUHQ6IHTDS3A";
        private static string SecretAccessKeyID = "VtkJmp8Lgi9kCKGbICCaUIGzdl5nqsN0CIeuLCm6";

        static void Main(string[] args)
        {
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

                    CreateJobRequest();

                    //Delete all messages
                    //client.DeleteMessage(m.ReceiptHandle);

                    client.DeleteMessage(new DeleteMessageRequest() { QueueUrl = request.QueueUrl, ReceiptHandle = m.ReceiptHandle });
                }


            }

        }

        private static void CreateJobRequest(string videoPath, string bucketName)
        {

            string accsessKey = AccessKeyID;
            string secretKey = SecretAccessKeyID;
            var etsClient = new AmazonElasticTranscoderClient(AccessKeyID, SecretAccessKeyID, RegionEndpoint.EUWest1);
            var notifications = new Notifications()
            {
                Completed = "arn:aws:sns:us-east-1:XXXXXXXXXXXX:Transcode",
                Error = "arn:aws:sns:us-east-1:XXXXXXXXXXXX:Transcode",
                Progressing = "arn:aws:sns:us-east-1:XXXXXXXXXXXX:Transcode",
                Warning = "arn:aws:sns:us-east-1:XXXXXXXXXXXX:Transcode"
            };

            //var pipeline = etsClient.CreatePipeline(new CreatePipelineRequest()
            //{
            //    Name = "MyFolder",
            //    InputBucket = bucketName,
            //    OutputBucket = bucketName,
            //    Notifications = notifications,
            //    Role = "arn:aws:iam::XXXXXXXXXXXX:role/Elastic_Transcoder_Default_Role"

            //}).CreatePipelineResult.Pipeline;
            etsClient.CreateJob(new CreateJobRequest()
            {
                PipelineId = pipeline.Id,
                Input = new JobInput()
                {
                    AspectRatio = "auto",
                    Container = "mp4",
                    FrameRate = "auto",
                    Interlaced = "auto",
                    Resolution = "auto",
                    Key = videoPath

                },
                Output = new CreateJobOutput()
                {
                    ThumbnailPattern = videoPath + "videoName{resolution}_{count}",
                    Rotate = "0",
                    PresetId = "1351620000000-000020",
                    Key = videoPath + "newFileName.mp4"
                }
            });

        }
    }
}
