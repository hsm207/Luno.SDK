// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Luno.SDK;

/// <summary>
/// Configuration options for the Luno SDK. 🏛️💎
/// </summary>
public class LunoClientOptions
{
    public string BaseUrl { get; set; } = "https://api.luno.com";
    
    public string UserAgent { get; set; } = "Luno.SDK/1.0.0 (.NET 10; Pristine)";

    /// <summary>
    /// The API version to target. Default is "1". 💅✨
    /// Change this to "2" next year without breaking your code! 🚀
    /// </summary>
    public string ApiVersion { get; set; } = "1";
    
    public ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;
}
