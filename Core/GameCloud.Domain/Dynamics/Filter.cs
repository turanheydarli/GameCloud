namespace GameCloud.Domain.Dynamics;

public record Filter(string Field, string Operator, string? Value, string? Logic, IEnumerable<Filter>? Filters);