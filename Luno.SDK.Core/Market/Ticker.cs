// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

namespace Luno.SDK.Core.Market;

/// <summary>
/// Represents a high-fidelity market heartbeat from Luno. 💓
/// Now with human-readable timestamps! 🤌✨
/// </summary>
public record Ticker(
    string Pair,
    decimal Ask,
    decimal Bid,
    decimal LastTrade,
    decimal Rolling24HourVolume,
    MarketStatus Status,
    DateTimeOffset Timestamp
)
{
    public decimal Spread => Ask - Bid;
    
    public bool IsActive => Status is MarketStatus.Active;
}
