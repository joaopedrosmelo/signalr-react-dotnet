using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace APISignalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IAmazonSQS _sqs;
        private readonly AWSSettings _awsSettings;

        public ReportController(IAmazonSQS sqs, IOptions<AWSSettings> awsOptions)
        {
            _sqs = sqs;
            _awsSettings = awsOptions.Value;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReport()
        {
            var reportFileName = $"report-{Guid.NewGuid()}.txt";
            var reportUrl = $"https://{_awsSettings.S3BucketName}.s3.amazonaws.com/{reportFileName}";

            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = _awsSettings.RequestQueueUrl,
                MessageBody = reportFileName
            };

            await _sqs.SendMessageAsync(sendMessageRequest);

            return Ok(new { reportUrl });
        }
    }
}