namespace GameCloud.Application.Features.Actions;

public class ExecutionContextAccessor : IExecutionContextAccessor
{
    private AsyncLocal<ActionExecutionContext> _context = new();
    public ActionExecutionContext Context => _context.Value;

    public ExecutionContextAccessor()
    {
        SetContext(ActionExecutionContext.Api);
    }

    public void SetContext(ActionExecutionContext context)
    {
        _context.Value = context;
    }
}

public enum ActionExecutionContext
{
    Function,
    Api
}