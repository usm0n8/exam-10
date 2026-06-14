namespace Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public ErrorType? ErrorType { get; set; }

    private Result(T? data)
    {
        IsSuccess = true;
        Data = data;
    }

    private Result(string error, ErrorType errorType)
    {
        IsSuccess = false;
        Data = default;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Ok(T? data) => new(data);

    public static Result<T> Fail(string error, ErrorType errorType = Common.ErrorType.Unknown) =>
        new(error, errorType);
}
