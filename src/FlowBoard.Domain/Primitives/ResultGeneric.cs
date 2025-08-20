namespace FlowBoard.Domain.Primitives;

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
