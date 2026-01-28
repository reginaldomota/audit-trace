namespace Audit.Models;

using Audit.Enums;

public class AuditLog
{
    public int Id { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public DateTime LoggedAt { get; set; }
    public AuditCategory Category { get; set; }
    public string? Method { get; set; }
    public string Operation { get; set; } = string.Empty;
    public bool HasError { get; set; }
    public int? StatusCode { get; set; }
    public string? StatusCodeDescription { get; set; }  
    public long DurationMs { get; set; }
    public string? InputData { get; set; }
    public string? OutputData { get; set; }
    public string? Metadata { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
}
