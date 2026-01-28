using Audit.Enums;

namespace Audit.Models;

/// <summary>
/// Modelo para ingerir informações de trace em jobs e outros contextos.
/// Contém os dados necessários para rastrear operações através do sistema.
/// </summary>
public class TraceInfo
{
    /// <summary>
    /// Identificador único do trace (GUID v7)
    /// </summary>
    public string TraceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Categoria da operação (HTTP, JOB, etc)
    /// </summary>
    public AuditCategory Category { get; set; }
    
    /// <summary>
    /// Método ou operação sendo executada
    /// </summary>
    public string Method { get; set; } = string.Empty;
}
