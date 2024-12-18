namespace GameCloud.Application.Common.Requests;

public record PageableRequest(
    int PageIndex = 0,
    int PageSize = 10);