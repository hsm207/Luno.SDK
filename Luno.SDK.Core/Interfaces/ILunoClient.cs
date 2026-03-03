namespace Luno.SDK;

/// <summary>
/// Defines the main interface for interacting with the Luno API.
/// Provides access to specialized sub-clients for market data and account management.
/// </summary>
public interface ILunoClient
{
    /// <summary>
    /// Gets the specialized client for market data operations.
    /// </summary>
    ILunoMarketClient Market { get; }

    /// <summary>
    /// Factory method to retrieve a specialized market client instance.
    /// </summary>
    /// <returns>An instance of <see cref="ILunoMarketClient"/>.</returns>
    ILunoMarketClient GetMarketClient();
}
