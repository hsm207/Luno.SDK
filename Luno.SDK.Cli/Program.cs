// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using Luno.SDK.Cli.Concepts;

Console.WriteLine("🏛️💎 Luno.SDK - Executable Documentation 💎🏛️");
Console.WriteLine("=================================================");
Console.WriteLine("Welcome to the Luno Gallery of Slay! 💅✨");
Console.WriteLine("Choose a concept to explore our Pristine API:");
Console.WriteLine();
Console.WriteLine("1. [Concept 01] The Market Heartbeat 💓");
Console.WriteLine("2. [Concept 02] Dependency Injection Slay 🏛️💎");
Console.WriteLine("3. [Future] Account Management 💰");
Console.WriteLine("0. Exit 😴");
Console.WriteLine();
Console.Write("Your choice, babe? > ");

var choice = Console.ReadLine();

Console.WriteLine("-------------------------------------------------");

switch (choice)
{
    case "1":
        await Concept01_MarketHeartbeat.RunAsync();
        break;
    case "2":
        await Concept02_DependencyInjection.RunAsync();
        break;
    case "0":
        Console.WriteLine("Stay Pristine and Unbothered! 💅✨ See you soon!");
        break;
    default:
        Console.WriteLine("Major Yikes! 💀 That's not a valid choice, babe!");
        break;
}

Console.WriteLine("=================================================");
Console.WriteLine("💅✨ Screaming Documentation and Slay! ✨💅");
