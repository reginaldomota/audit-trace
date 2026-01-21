namespace Audit.Constants;

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
    
    public static string GetDescription(int statusCode)
    {
        return statusCode switch
        {
            OK => nameof(OK),
            Created => nameof(Created),
            Accepted => nameof(Accepted),
            NoContent => nameof(NoContent),
            BadRequest => nameof(BadRequest),
            Unauthorized => nameof(Unauthorized),
            Forbidden => nameof(Forbidden),
            NotFound => nameof(NotFound),
            Conflict => nameof(Conflict),
            UnprocessableEntity => nameof(UnprocessableEntity),
            InternalServerError => nameof(InternalServerError),
            BadGateway => nameof(BadGateway),
            ServiceUnavailable => nameof(ServiceUnavailable),
            GatewayTimeout => nameof(GatewayTimeout),
            _ => statusCode.ToString()
        };
    }
}
