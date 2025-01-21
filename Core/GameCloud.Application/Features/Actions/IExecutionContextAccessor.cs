namespace GameCloud.Application.Features.Actions;

public interface IExecutionContextAccessor 
{
    ActionExecutionContext Context { get; }
    void SetContext(ActionExecutionContext context);
}