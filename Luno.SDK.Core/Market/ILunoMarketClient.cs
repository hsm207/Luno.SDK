// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using Luno.SDK.Core.Market;

namespace Luno.SDK;

/// <summary>
/// Specialized client for Luno Market data. 📈✨
/// Can be instantiated directly or via the main LunoClient! 🤌✨
/// </summary>
public interface ILunoMarketClient
{
    IAsyncEnumerable<Ticker> GetTickersAsync(CancellationToken ct = default);
}
