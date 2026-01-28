namespace Audit.Constants;

public static class JobStatusCodes
{
    public const int Success = 0;
    public const int Failed = 1;
    public const int Cancelled = 2;
    public const int Timeout = 3;
    public const int PartialSuccess = 4;
    
    public static string GetDescription(int statusCode)
    {
        return statusCode switch
        {
            Success => nameof(Success),
            Failed => nameof(Failed),
            Cancelled => nameof(Cancelled),
            Timeout => nameof(Timeout),
            PartialSuccess => nameof(PartialSuccess),
            _ => statusCode.ToString()
        };
    }

    public static bool IsError(int statusCode)
    {
        return statusCode switch
        {
            Success => false,
            PartialSuccess => false,
            Failed => true,
            Cancelled => true,
            Timeout => true,
            _ => true
        };
    }
}
