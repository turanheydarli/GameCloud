using GameCloud.Domain.Paging;

namespace GameCloud.Domain.Dynamics;

public record DynamicRequest(
    string Search,
    IEnumerable<Sort>? Sort,
    Filter? Filter,
    int PageIndex = 0,
    int PageSize = 10) : PageableRequest(PageIndex, PageSize);
