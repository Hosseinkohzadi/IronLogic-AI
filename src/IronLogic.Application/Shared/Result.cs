namespace IronLogic.Application.Shared;

/// <summary>
/// Represents the outcome of an operation, indicating success or failure.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">A flag indicating the success of the operation.</param>
    /// <param name="error">The error message associated with a failure.</param>
    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a success result.
    /// </summary>
    /// <returns>A new instance of <see cref="Result"/> indicating success.</returns>
    public static Result Success() => new(true, string.Empty);

    /// <summary>
    /// Creates a failure result with the specified error.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A new instance of <see cref="Result"/> indicating failure.</returns>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a success result with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to be returned.</param>
    /// <returns>A new instance of <see cref="Result{T}"/> indicating success.</returns>
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

    /// <summary>
    /// Creates a failure result with the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="error">The error message.</param>
    /// <returns>A new instance of <see cref="Result{T}"/> indicating failure.</returns>
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value, indicating success or failure.
/// </summary>
/// <typeparam name="T">The type of the value returned by the operation.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value returned by the operation if successful.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">The value returned by the operation.</param>
    /// <param name="isSuccess">A flag indicating the success of the operation.</param>
    /// <param name="error">The error message associated with a failure.</param>
    protected internal Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}