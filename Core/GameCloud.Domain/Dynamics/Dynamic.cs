
namespace GameCloud.Domain.Dynamics;

public record DynamicRequest(
    IEnumerable<Sort>? Sort,
    Filter? Filter,
    int PageIndex = 0,
    int PageSize = 10);
