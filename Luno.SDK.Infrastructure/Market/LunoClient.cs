// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Luno.SDK.Infrastructure.Telemetry;
using Luno.SDK.Infrastructure.Market; // THE MISSING SLAY! 🤌

namespace Luno.SDK;

/// <summary>
/// A modular orchestrator for the Luno API following OpenAI design patterns! 🏛️💎
/// </summary>
public class LunoClient : ILunoClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly LunoTelemetry _telemetry;
    private readonly bool _disposeClient;
    private readonly ILogger<LunoClient> _logger;
    private readonly LunoClientOptions _options;

    public ILunoMarketClient Market => GetMarketClient();

    public LunoClient(LunoClientOptions? options = null)
    {
        _options = options ?? new LunoClientOptions();
        _logger = _options.LoggerFactory.CreateLogger<LunoClient>();
        
        _telemetry = new LunoTelemetry();
        
        var handler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) };
        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", _options.UserAgent);
        
        _disposeClient = true;
    }

    public LunoClient(HttpClient httpClient, ILogger<LunoClient>? logger = null)
    {
        _httpClient = httpClient;
        _options = new LunoClientOptions();
        _telemetry = new LunoTelemetry();
        _disposeClient = false;
        _logger = logger ?? NullLogger<LunoClient>.Instance;
    }

    public ILunoMarketClient GetMarketClient() => 
        new LunoMarketClient(_httpClient, _telemetry, _logger, _options.ApiVersion);

    public void Dispose()
    {
        if (_disposeClient) _httpClient.Dispose();
        _telemetry.Dispose();
        GC.SuppressFinalize(this);
    }
}
