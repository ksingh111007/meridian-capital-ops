namespace Meridian.Domain;

/// <summary>
/// Single exception family for business-rule failures. The API maps
/// <see cref="Kind"/> to an HTTP status (400/404/403/409) via middleware.
/// </summary>
public sealed class DomainException : Exception
{
    public ErrorKind Kind { get; }

    private DomainException(ErrorKind kind, string message) : base(message) => Kind = kind;

    public static DomainException Validation(string message) => new(ErrorKind.Validation, message);
    public static DomainException NotFound(string message) => new(ErrorKind.NotFound, message);
    public static DomainException Forbidden(string message) => new(ErrorKind.Forbidden, message);
    public static DomainException Conflict(string message) => new(ErrorKind.Conflict, message);
}
