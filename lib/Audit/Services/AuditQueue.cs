namespace Audit.Services;

using Audit.Interfaces;
using Audit.Models;
using System.Threading.Channels;

public class AuditQueue : IAuditQueue
{
    private readonly Channel<AuditLog> _channel;

    public AuditQueue()
    {
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.DropOldest // Descarta logs mais antigos se a fila estiver cheia
        };
        
        _channel = Channel.CreateBounded<AuditLog>(options);
    }

    public ValueTask EnqueueAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        // TryWrite não bloqueia - retorna false se a fila estiver cheia
        // Com DropOldest, sempre retorna true removendo o item mais antigo
        _channel.Writer.TryWrite(auditLog);
        return ValueTask.CompletedTask;
    }

    public async ValueTask<AuditLog?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }
        catch (ChannelClosedException)
        {
            return null;
        }
    }
}
