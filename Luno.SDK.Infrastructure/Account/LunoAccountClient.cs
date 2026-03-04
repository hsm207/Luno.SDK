using Microsoft.Kiota.Abstractions;
using Luno.SDK.Core.Account;
using Luno.SDK.Infrastructure.Authentication;
using Luno.SDK.Infrastructure.Generated.Api;

namespace Luno.SDK.Infrastructure.Account;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoAccountClient"/> interface
/// using the generated Kiota client.
/// </summary>
public class LunoAccountClient : ILunoAccountClient
{
    private readonly Luno.SDK.Infrastructure.Generated.LunoApiClient _apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAccountClient"/> class.
    /// </summary>
    /// <param name="requestAdapter">The decorated request adapter pipeline.</param>
    public LunoAccountClient(IRequestAdapter requestAdapter)
    {
        if (requestAdapter == null) throw new ArgumentNullException(nameof(requestAdapter));
        _apiClient = new Luno.SDK.Infrastructure.Generated.LunoApiClient(requestAdapter);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Balance>> GetBalancesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.Api.One.Balance.GetAsync(requestConfiguration =>
        {
            requestConfiguration.Options.Add(new LunoAuthenticationOption { RequiresAuthentication = true });
        }, cancellationToken).ConfigureAwait(false);

        if (response?.Balance == null)
        {
            throw new InvalidOperationException("The API response was successful but the balances array was missing or null.");
        }

        var results = new List<Balance>();
        foreach (var balanceDto in response.Balance)
        {
            if (balanceDto != null)
            {
                results.Add(balanceDto.ToDomain());
            }
        }

        return results;
    }
}
