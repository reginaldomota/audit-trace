namespace Audit.Constants;

public static class AuditCategories
{
    public const string HTTP = "HTTP";
    public const string Job = "Job";
    public const string Background = "Background";
    public const string Queue = "Queue";
    public const string Database = "Database";
    public const string External = "External";
    public const string System = "System";
}

public static class HttpMethods
{
    public const string GET = "GET";
    public const string POST = "POST";
    public const string PUT = "PUT";
    public const string PATCH = "PATCH";
    public const string DELETE = "DELETE";
    public const string HEAD = "HEAD";
    public const string OPTIONS = "OPTIONS";
}

public static class JobMethods
{
    public const string Scheduled = "Scheduled";
    public const string Triggered = "Triggered";
    public const string Manual = "Manual";
    public const string Retry = "Retry";
}

public static class HttpStatusCodes
{
    // Success
    public const int OK = 200;
    public const int Created = 201;
    public const int Accepted = 202;
    public const int NoContent = 204;
    
    // Client Errors
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int Conflict = 409;
    public const int UnprocessableEntity = 422;
    
    // Server Errors
    public const int InternalServerError = 500;
    public const int BadGateway = 502;
    public const int ServiceUnavailable = 503;
    public const int GatewayTimeout = 504;
}

public static class JobStatusCodes
{
    public const int Success = 0;
    public const int Failed = 1;
    public const int Cancelled = 2;
    public const int Timeout = 3;
    public const int PartialSuccess = 4;
}
