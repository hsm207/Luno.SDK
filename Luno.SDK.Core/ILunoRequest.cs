using Luno.SDK;

namespace Luno.SDK;

/// <summary>
/// Defines the base contract for all requests in the Luno SDK.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the request.</typeparam>
public interface ILunoRequest<out TResponse>
{
    /// <summary>
    /// Gets the per-request options for the operation.
    /// </summary>
    LunoRequestOptions Options { get; }
}

/// <summary>
/// Defines a command operation that mutates state and returns a response.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
public interface ILunoCommand<out TResponse> : ILunoRequest<TResponse> { }

/// <summary>
/// Defines a query operation that reads state and returns a response.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the query.</typeparam>
public interface ILunoQuery<out TResponse> : ILunoRequest<TResponse> { }

/// <summary>
/// Provides a base implementation for requests, ensuring a standardized Options property.
/// </summary>
/// <typeparam name="TResponse">The type of response result.</typeparam>
public abstract record LunoRequestBase<TResponse> : ILunoRequest<TResponse>
{
    /// <summary>
    /// Gets or initializes the request-scoped options (e.g., security intent, public auth).
    /// </summary>
    public LunoRequestOptions Options { get; init; } = new();
}

/// <summary>
/// Base record for all state-mutating commands.
/// </summary>
/// <typeparam name="TResponse">The type of response result.</typeparam>
public abstract record LunoCommandBase<TResponse> : LunoRequestBase<TResponse>, ILunoCommand<TResponse>;

/// <summary>
/// Base record for all state-reading queries.
/// </summary>
/// <typeparam name="TResponse">The type of response result.</typeparam>
public abstract record LunoQueryBase<TResponse> : LunoRequestBase<TResponse>, ILunoQuery<TResponse>;
