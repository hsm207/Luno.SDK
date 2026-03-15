using Microsoft.Kiota.Abstractions;
using Luno.SDK.Account;

namespace Luno.SDK.Infrastructure.Account;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoAccountClient"/> interface
/// using the generated Kiota client.
/// </summary>
/// <param name="requestAdapter">The decorated request adapter pipeline.</param>
internal class LunoAccountClient(IRequestAdapter requestAdapter) : ILunoAccountClient
{
    private readonly Luno.SDK.Infrastructure.Generated.LunoApiClient _apiClient = new(requestAdapter);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Balance>> GetBalancesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.Api.One.Balance.GetAsync(requestConfiguration =>
        {
            requestConfiguration.Options.Add(new Luno.SDK.Infrastructure.Telemetry.LunoTelemetryOptions("GetBalances"));
        }, cancellationToken).ConfigureAwait(false);

        var results = new List<Balance>();
        foreach (var balanceDto in response!.Balance!)
        {
            if (balanceDto != null)
            {
                results.Add(balanceDto.ToDomain());
            }
        }

        return results;
    }
}
