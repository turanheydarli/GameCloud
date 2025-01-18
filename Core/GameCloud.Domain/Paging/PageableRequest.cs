namespace GameCloud.Domain.Paging;

public record PageableRequest(
    int PageIndex = 0,
    int PageSize = 10);