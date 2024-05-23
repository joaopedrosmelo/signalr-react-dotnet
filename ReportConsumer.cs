using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using System.Text;

namespace APISignalR
{
    public class ReportConsumer : BackgroundService
    {
        private readonly IAmazonSQS _sqs;
        private readonly IAmazonS3 _s3Client;
        private readonly AWSSettings _awsSettings;

        public ReportConsumer(IAmazonSQS sqs, IAmazonS3 s3Client, IOptions<AWSSettings> awsOptions)
        {
            _sqs = sqs;
            _s3Client = s3Client;
            _awsSettings = awsOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = _awsSettings.RequestQueueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 10
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                var receiveMessageResponse = await _sqs.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                foreach (var message in receiveMessageResponse.Messages)
                {
                    // Simulate report generation
                    var reportContent = "This is the report content";
                    var reportFileName = message.Body;

                    // Upload the report to S3
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(reportContent)))
                    {
                        var putRequest = new Amazon.S3.Model.PutObjectRequest
                        {
                            BucketName = _awsSettings.S3BucketName,
                            Key = reportFileName,
                            InputStream = stream
                        };

                        await _s3Client.PutObjectAsync(putRequest, stoppingToken);
                    }
                    await Task.Delay(5000);
                    // Notify that the report is ready
                    var responseMessage = new SendMessageRequest
                    {
                        QueueUrl = _awsSettings.ResponseQueueUrl,
                        MessageBody = reportFileName
                    };

                    await _sqs.SendMessageAsync(responseMessage, stoppingToken);

                    // Remove message from the request queue
                    var deleteMessageRequest = new DeleteMessageRequest
                    {
                        QueueUrl = _awsSettings.RequestQueueUrl,
                        ReceiptHandle = message.ReceiptHandle
                    };

                    await _sqs.DeleteMessageAsync(deleteMessageRequest, stoppingToken);
                }
            }
        }
    }
}