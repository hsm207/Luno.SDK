// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

namespace Luno.SDK;

/// <summary>
/// The authoritative entry point for the Luno Universe. 🌍✨
/// Use this to create specialized clients that share implementation details. 🏛️💎
/// </summary>
public interface ILunoClient
{
    ILunoMarketClient Market { get; }

    ILunoMarketClient GetMarketClient();
}
