namespace Audit.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AuditLogAttribute : Attribute
{
    public string? Operation { get; set; }
    public string? Category { get; set; }
}
