using Microsoft.Kiota.Abstractions;
using Luno.SDK.Account;
using Luno.SDK.Infrastructure.Authentication;
using Luno.SDK.Infrastructure.Generated.Api;

namespace Luno.SDK.Infrastructure.Account;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoAccountClient"/> interface
/// using the generated Kiota client.
/// </summary>
/// <param name="requestAdapter">The decorated request adapter pipeline.</param>
internal class LunoAccountClient(IRequestAdapter requestAdapter) : ILunoAccountClient
{
    private readonly Luno.SDK.Infrastructure.Generated.LunoApiClient _apiClient = new(requestAdapter ?? throw new ArgumentNullException(nameof(requestAdapter)));

    /// <inheritdoc />
    public async Task<IReadOnlyList<Balance>> GetBalancesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.Api.One.Balance.GetAsync(requestConfiguration =>
        {
            requestConfiguration.Options.Add(new LunoAuthenticationOption { RequiresAuthentication = true });
            requestConfiguration.Options.Add(new Luno.SDK.Infrastructure.Telemetry.LunoTelemetryOptions("GetBalances"));
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
