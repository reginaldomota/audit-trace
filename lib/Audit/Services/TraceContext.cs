using Audit.Enums;

namespace Audit.Services;

public interface ITraceContext
{
    string TraceId { get; }
    AuditCategory Category { get; }
    string Method { get; }
}

public class TraceContext : ITraceContext
{
    public string TraceId { get; set; } = string.Empty;
    public AuditCategory Category { get; set; }
    public string Method { get; set; } = string.Empty;
}
