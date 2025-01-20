using System.Text.Json;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using GameCloud.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameCloud.Messaging.Brokers;

public class YandexCloudOptions
{
    public string QueueUrl { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ServiceUrl { get; set; } = "https://message-queue.api.cloud.yandex.net";
}

public class YandexSqsPublisher : IEventPublisher, IDisposable
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;
    private readonly ILogger<YandexSqsPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public YandexSqsPublisher(
        IOptions<YandexCloudOptions> options,
        ILogger<YandexSqsPublisher> logger)
    {
        _logger = logger;
        _queueUrl = options.Value.QueueUrl;

        var sqsConfig = new AmazonSQSConfig
        {
            ServiceURL = options.Value.ServiceUrl
        };

        var credentials = new BasicAWSCredentials(
            options.Value.AccessKeyId,
            options.Value.SecretKey);

        _sqsClient = new AmazonSQSClient(credentials, sqsConfig);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task PublishAsync<T>(T eventMessage) where T : class
    {
        try
        {
            var message = JsonSerializer.Serialize(eventMessage, _jsonOptions);

            var request = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = message,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "EventType",
                        new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = typeof(T).Name
                        }
                    }
                }
            };

            var response = await _sqsClient.SendMessageAsync(request);

            _logger.LogInformation(
                "Published event {EventType} with message ID {MessageId}",
                typeof(T).Name,
                response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing event {EventType} to queue {QueueUrl}",
                typeof(T).Name,
                _queueUrl);
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _sqsClient?.Dispose();
        }

        _disposed = true;
    }
}