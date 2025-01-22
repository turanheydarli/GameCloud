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
        // This one used to generate CAST(...).ILIKE. Now we do a case-insensitive .Contains(...) approach.
        { "jsonsearch", "JsonSearch" }
    };

    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicRequest dynamic)
    {
        if (dynamic.Filter is not null)
            query = Filter(query, dynamic.Filter);

        if (dynamic.Sort is not null && dynamic.Sort.Any())
            query = Sort(query, dynamic.Sort);

        return query;
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

    private static IQueryable<T> Filter<T>(
        IQueryable<T> queryable, Filter filter)
    {
        var filters = GetAllFilters(filter);

        var values = new object[filters.Count];
        for (int i = 0; i < filters.Count; i++)
        {
            var f = filters[i];
            if (f.Operator == "jsonsearch")
            {
                // We store the search string for case-insensitive .Contains(...) check
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

        var where = Transform(filter, filters);
        if (!string.IsNullOrWhiteSpace(where))
        {
            queryable = queryable.Where(where, values);
        }
        return queryable;
    }

    private static string Transform(Filter filter, IList<Filter> allFilters)
    {
        int index = allFilters.IndexOf(filter);
        string comparison = Operators[filter.Operator];
        var where = new StringBuilder();

        if (!string.IsNullOrEmpty(filter.Value))
        {
            if (filter.Operator == "jsonsearch")
            {
                // Use a dynamic-linq-friendly expression:
                // "np(Field) != null && np(Field).ToLower().Contains(@index.ToLower())"
                // np(...) is from System.Linq.Dynamic.Core to handle null-propagation.
                // This ensures EF tries to translate a normal string .Contains(...) if the column is a string.
                // If your column is JsonDocument or another type, it may do client eval. Adjust as needed.
                where.Append($"(np({filter.Field}) != null && np({filter.Field}).ToLower().Contains(@{index}.ToLower()))");
            }
            else if (filter.Operator == "doesnotcontain")
            {
                // e.g. "!np(Field).Contains(@index)"
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
                // eq, neq, lt, lte, gt, gte
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
                var joinedFilters = string.Join($" {filter.Logic} ", validFilters.Select(f => Transform(f, allFilters)));
                return where.Length > 0
                    ? $"({where} {filter.Logic} ({joinedFilters}))"
                    : $"({joinedFilters})";
            }
        }

        Console.WriteLine($"Generated Query: {where}");
        foreach (var param in allFilters.Select((flt, i) => new { Index = i, Value = flt.Value }))
            Console.WriteLine($"@{param.Index} = {param.Value}");

        return where.ToString();
    }

    public static IList<Filter> GetAllFilters(Filter filter)
    {
        var list = new List<Filter>();
        GetFiltersRecursive(filter, list);
        return list;
    }

    private static void GetFiltersRecursive(Filter filter, IList<Filter> filters)
    {
        filters.Add(filter);
        if (filter.Filters is not null && filter.Filters.Any())
        {
            foreach (var item in filter.Filters)
                GetFiltersRecursive(item, filters);
        }
    }

    public static IQueryable<T> ApplyDynamicRequest<T>(
        this IQueryable<T> query, DynamicRequest request)
    {
        return query.ToDynamic(request);
    }
}
