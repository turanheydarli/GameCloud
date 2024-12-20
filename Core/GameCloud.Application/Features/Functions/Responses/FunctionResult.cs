namespace GameCloud.Application.Features.Functions.Responses;

public class FunctionResult
{
    public Guid Id { get; set; }
    public string Status { get; set; }
    public Dictionary<string, Dictionary<string, object>> Changes { get; set; }
    public List<NotificationMessage> Messages { get; set; }
    public FunctionError Error { get; set; }
}