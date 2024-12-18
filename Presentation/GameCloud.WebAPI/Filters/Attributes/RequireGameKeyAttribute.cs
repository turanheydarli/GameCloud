namespace GameCloud.WebAPI.Filters.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireGameKeyAttribute : Attribute
{
}
