using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace APISignalR
{
    public class NotificationConsumer : BackgroundService
    {
        private readonly IAmazonSQS _sqs;
        private readonly IHubContext<ReportHub> _hubContext;
        private readonly AWSSettings _awsSettings;

        public NotificationConsumer(IAmazonSQS sqs, IHubContext<ReportHub> hubContext, IOptions<AWSSettings> awsOptions)
        {
            _sqs = sqs;
            _hubContext = hubContext;
            _awsSettings = awsOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = _awsSettings.ResponseQueueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 10
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                var receiveMessageResponse = await _sqs.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                foreach (var message in receiveMessageResponse.Messages)
                {
                    // Notify clients via SignalR
                    var reportUrl = $"https://{_awsSettings.S3BucketName}.s3.amazonaws.com/{message.Body}";
                    await _hubContext.Clients.All.SendAsync("ReportReady", reportUrl);

                    // Remove message from the response queue
                    var deleteMessageRequest = new DeleteMessageRequest
                    {
                        QueueUrl = _awsSettings.ResponseQueueUrl,
                        ReceiptHandle = message.ReceiptHandle
                    };

                    await _sqs.DeleteMessageAsync(deleteMessageRequest, stoppingToken);
                }
            }
        }
    }
}
