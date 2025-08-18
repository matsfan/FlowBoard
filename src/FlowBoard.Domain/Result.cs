namespace FlowBoard.Domain;

public readonly record struct Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
}

public enum ErrorType
{
    Validation,
    Conflict,
    NotFound,
    Unexpected
}

public class Result
{
    private static readonly Result _success = new(true, Array.Empty<Error>());
    protected Result(bool isSuccess, IReadOnlyCollection<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyCollection<Error> Errors { get; }

    public static Result Success() => _success;
    public static Result Failure(params Error[] errors) => new(false, errors);
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors.ToArray());

    public static implicit operator Result(Error error) => Failure(error);
    public static implicit operator Result(Error[] errors) => Failure(errors);
    public static implicit operator Result(List<Error> errors) => Failure(errors);
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, IReadOnlyCollection<Error> errors) : base(isSuccess, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<Error>());
    public static new Result<T> Failure(params Error[] errors) => new(false, default, errors);
    public static new Result<T> Failure(IEnumerable<Error> errors) => new(false, default, errors.ToArray());

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
    public static implicit operator Result<T>(Error[] errors) => Failure(errors);
    public static implicit operator Result<T>(List<Error> errors) => Failure(errors);
}
