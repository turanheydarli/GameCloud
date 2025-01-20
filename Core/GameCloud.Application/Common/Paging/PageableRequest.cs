namespace GameCloud.Application.Common.Paging;

public record PageableRequest(
    bool IsAscending = true,
    string? Search = null,
    int PageIndex = 0,
    int PageSize = 10);