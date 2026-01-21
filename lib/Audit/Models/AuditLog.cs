namespace Audit.Models;

using Audit.Enums;

public class AuditLog
{
    public int Id { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public DateTime LoggedAt { get; set; }
    public AuditCategory Category { get; set; } // HTTP, Job, Background, etc
    public string Operation { get; set; } = string.Empty; // Path da API ou nome do Job
    public string? Method { get; set; } // HTTP Method ou tipo de operação
    public int? StatusCode { get; set; } // HTTP Status ou Job Exit Code
    public string? StatusCodeDescription { get; set; } // Descrição do status: "OK", "Failed", etc
    public long DurationMs { get; set; }
    public string? InputData { get; set; } // Request body ou Job input
    public string? OutputData { get; set; } // Response body ou Job output
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? Metadata { get; set; } // JSON com dados adicionais
}
