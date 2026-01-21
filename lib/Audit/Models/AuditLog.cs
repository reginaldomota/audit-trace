namespace Audit.Models;

public class AuditLog
{
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long Duration { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
}
