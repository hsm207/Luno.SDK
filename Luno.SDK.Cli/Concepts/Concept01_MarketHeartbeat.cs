// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using Luno.SDK;

namespace Luno.SDK.Cli.Concepts;

/// <summary>
/// CONCEPT 01: The Market Heartbeat 💓✨
/// Learn how to fetch real-time market data with just one line of code! 🤌
/// </summary>
public static class Concept01_MarketHeartbeat
{
    public static async Task RunAsync()
    {
        Console.WriteLine("--- Concept 01: Market Heartbeat ---");
        
        // 1. Initialize the client (Standalone mode! 💅)
        using var luno = new LunoClient();

        // 2. Use the fluent extension to stream heartbeats! 🌊📈
        await foreach (var heartbeat in luno.GetMarketHeartbeatAsync())
        {
            var statusEmoji = heartbeat.IsActive ? "✅" : "🛑";
            Console.WriteLine($"[{heartbeat.Timestamp:HH:mm:ss.fff}] [{statusEmoji}] {heartbeat.Pair,-10} | Price: {heartbeat.Price,12:N2}");
        }
        
        Console.WriteLine("--- Concept 01 Complete! 🥂 ---");
    }
}
