namespace PlaylistApi.Common;

// Equivale a NotFoundException, ConflictException, UnauthorizedException... de NestJS.
// Los servicios la lanzan y ErrorHandlingMiddleware la convierte en la respuesta HTTP.
public class HttpException : Exception
{
    public int StatusCode { get; }
    public string ErrorName { get; }

    public HttpException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorName = statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            _ => "Error"
        };
    }
}
