using System.Security.Cryptography;

namespace Audit.Helpers;

public static class TraceIdGenerator
{
    public static string NewTraceId()
    {
        // Formato: UCTI-YYYYMMDDHHmmssff-HEX (32 caracteres)
        // UCTI- = 5 caracteres
        // YYYYMMDDHHmmssff = 16 caracteres (com centésimos de segundo)
        // - = 1 caractere
        // HEX = 10 caracteres hexadecimais
        // Total: 32 caracteres
        
        // Timestamp com centésimos de segundo para reduzir colisões
        var now = DateTime.UtcNow;
        var timestamp = now.ToString("yyyyMMddHHmmssff");
        
        // Gera 5 bytes aleatórios criptograficamente seguros (10 caracteres hex)
        var randomBytes = new byte[5];
        RandomNumberGenerator.Fill(randomBytes);
        var hexRandom = Convert.ToHexString(randomBytes);
        
        return $"UCTI-{timestamp}-{hexRandom}";
    }
}
