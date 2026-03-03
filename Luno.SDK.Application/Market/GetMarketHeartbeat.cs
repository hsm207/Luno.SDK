// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using System.Runtime.CompilerServices;
using Luno.SDK.Core.Market;

namespace Luno.SDK.Application.Market;

public record GetMarketHeartbeatQuery;

public record MarketHeartbeatResponse(
    string Pair,
    decimal Price,
    decimal Spread,
    bool IsActive,
    DateTimeOffset Timestamp
);

public class GetMarketHeartbeatHandler(ILunoClient lunoClient)
{
    public async IAsyncEnumerable<MarketHeartbeatResponse> HandleAsync(
        GetMarketHeartbeatQuery query, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var marketClient = lunoClient.GetMarketClient();
        
        await foreach (var ticker in marketClient.GetTickersAsync(ct))
        {
            yield return new MarketHeartbeatResponse(
                ticker.Pair,
                ticker.LastTrade,
                ticker.Spread,
                ticker.IsActive,
                ticker.Timestamp
            );
        }
    }
}
