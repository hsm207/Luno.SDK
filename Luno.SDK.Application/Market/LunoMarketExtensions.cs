// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using Luno.SDK.Application.Market;

namespace Luno.SDK;

/// <summary>
/// High-energy fluent extensions to unlock the Luno Universe! 🌍✨
/// This is the "One Line to Slay" 💅 experience! 🏛️💎
/// </summary>
public static class LunoMarketExtensions
{
    /// <summary>
    /// Fetches the high-fidelity market heartbeats directly from the client! 💓✨
    /// </summary>
    public static IAsyncEnumerable<MarketHeartbeatResponse> GetMarketHeartbeatAsync(
        this ILunoClient client, 
        CancellationToken ct = default)
    {
        var handler = new GetMarketHeartbeatHandler(client);
        return handler.HandleAsync(new GetMarketHeartbeatQuery(), ct);
    }
}
