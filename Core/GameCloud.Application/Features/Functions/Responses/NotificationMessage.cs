namespace GameCloud.Application.Features.Functions.Responses;

public class NotificationMessage
{
    public string To { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }
    public Dictionary<string, object> Data { get; set; }
}