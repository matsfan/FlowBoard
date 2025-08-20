namespace FlowBoard.Domain;

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
