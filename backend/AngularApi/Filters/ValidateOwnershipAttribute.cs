namespace AngularApi.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ValidateOwnershipAttribute : Attribute
{
    public ValidateOwnershipAttribute(ResourceType resourceType, string idParameterName = "id")
    {
        ResourceType = resourceType;
        IdParameterName = idParameterName;
    }

    public ResourceType ResourceType { get; }

    public string IdParameterName { get; }
}
