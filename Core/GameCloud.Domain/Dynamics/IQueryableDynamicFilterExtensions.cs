using System.Linq.Dynamic.Core;
using System.Text;

namespace GameCloud.Domain.Dynamics;

public static class IQueryableDynamicFilterExtensions
{
    private static readonly IDictionary<string, string> Operators = new Dictionary<string, string>
{
    { "eq", "=" },
    { "neq", "!=" },
    { "lt", "<" },
    { "lte", "<=" },
    { "gt", ">" },
    { "gte", ">=" },
    { "isnull", "== null" },
    { "isnotnull", "!= null" },
    { "startswith", "StartsWith" },
    { "endswith", "EndsWith" },
    { "contains", "Contains" },
    { "doesnotcontain", "Contains" },
    { "jsonsearch", "JsonSearch" }
};


    public static IQueryable<T> ToDynamic<T>(
        this IQueryable<T> query, DynamicRequest dynamic)
    {
        if (dynamic.Filter is not null) query = Filter(query, dynamic.Filter);
        if (dynamic.Sort is not null && dynamic.Sort.Any()) query = Sort(query, dynamic.Sort);
        return query;
    }

    private static IQueryable<T> Filter_Old<T>(
        IQueryable<T> queryable, Filter filter)
    {
        IList<Filter> filters = GetAllFilters(filter);
    
        object[] values = new object[filters.Count];
        for (int i = 0; i < filters.Count; i++)
        {
            Filter f = filters[i];
            if (f.Operator == "jsonsearch")
            {
                values[i] = $"{{\"search\": \"{f.Value}\"}}";
            }
            else if (DateTime.TryParse(f.Value, out DateTime dateTime))
            {
                values[i] = dateTime.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                    : dateTime.ToUniversalTime();
            }
            else
            {
                values[i] = f.Value;
            }
        }

        string where = Transform(filter, filters);
        queryable = queryable.Where(where, values);

        return queryable;
    }

    private static IQueryable<T> Sort<T>(
        IQueryable<T> queryable, IEnumerable<Sort> sort)
    {
        if (sort.Any())
        {
            string ordering = string.Join(",", sort.Select(s => $"{s.Field} {s.Dir}"));
            return queryable.OrderBy(ordering);
        }

        return queryable;
    }

    public static IList<Filter> GetAllFilters(Filter filter)
    {
        List<Filter> filters = new();
        GetFilters(filter, filters);
        return filters;
    }

    private static void GetFilters(Filter filter, IList<Filter> filters)
    {
        filters.Add(filter);
        if (filter.Filters is not null && filter.Filters.Any())
            foreach (Filter item in filter.Filters)
                GetFilters(item, filters);
    }
    private static string Transform(Filter filter, IList<Filter> filters)
    {
        int index = filters.IndexOf(filter);
        string comparison = Operators[filter.Operator];
        StringBuilder where = new();

        if (!string.IsNullOrEmpty(filter.Value))
        {
            if (filter.Operator == "jsonsearch")
            {
                where.Append($"(CAST({filter.Field} AS text) ILIKE '%' || @{index} || '%')");
            }
            else if (filter.Operator == "doesnotcontain")
            {
                where.Append($"(!np({filter.Field}).{comparison}(@{index}))");
            }
            else if (comparison == "StartsWith" ||
                     comparison == "EndsWith" ||
                     comparison == "Contains")
            {
                where.Append($"(np({filter.Field}).{comparison}(@{index}))");
            }
            else
            {
                where.Append($"np({filter.Field}) {comparison} @{index}");
            }
        }
        else if (filter.Operator == "isnull" || filter.Operator == "isnotnull")
        {
            where.Append($"np({filter.Field}) {comparison}");
        }

        if (filter.Logic is not null && filter.Filters is not null && filter.Filters.Any())
        {
            var validFilters = filter.Filters.Where(f => f != null).ToList();
            if (validFilters.Any())
            {
                string joinedFilters = string.Join($" {filter.Logic} ", validFilters.Select(f => Transform(f, filters)));
                return where.Length > 0 
                    ? $"({where} {filter.Logic} ({joinedFilters}))"
                    : $"({joinedFilters})";
            }
        }
        
        Console.WriteLine($"Generated Query: {where.ToString()}");
        foreach (var param in filters.Select((filter, i) => new { Index = i, Value = filter.Value }))
        {
            Console.WriteLine($"@{param.Index} = {param.Value}");
        }


        return where.ToString();
    }


private static IQueryable<T> Filter<T>(
    IQueryable<T> queryable, Filter filter)
{
    IList<Filter> filters = GetAllFilters(filter);
    
    object[] values = new object[filters.Count];
    for (int i = 0; i < filters.Count; i++)
    {
        Filter f = filters[i];
        if (f.Operator == "jsonsearch")
        {
            values[i] = f.Value;
        }
        else if (DateTime.TryParse(f.Value, out DateTime dateTime))
        {
            values[i] = dateTime.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();
        }
        else
        {
            values[i] = f.Value;
        }
    }

    string where = Transform(filter, filters);
    if (!string.IsNullOrWhiteSpace(where))
    {
        queryable = queryable.Where(where, values);
    }

    return queryable;
}

    public static IQueryable<T> ApplyDynamicRequest<T>(
        this IQueryable<T> query,
        DynamicRequest request)
    {
        return query.ToDynamic(request);
    }
    
    private static object NormalizeValue(string value)
    {
        if (DateTime.TryParse(value, out DateTime dateTime))
        {
            return dateTime.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();
        }
        return value;
    }
}