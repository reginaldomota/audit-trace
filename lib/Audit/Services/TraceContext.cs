namespace Audit.Services;

public interface ITraceContext
{
    string TraceId { get; }
}

public class TraceContext : ITraceContext
{
    public string TraceId { get; set; } = string.Empty;
}
