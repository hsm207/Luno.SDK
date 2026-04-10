using Luno.SDK.Cli.Concepts;

// Check if a selection was passed via command-line arguments (useful for solo debugging!)
var choice = args.FirstOrDefault();

if (string.IsNullOrWhiteSpace(choice))
{
    Console.WriteLine("Luno.SDK - Demonstration Gallery");
    Console.WriteLine("=================================================");
    Console.WriteLine("Welcome to the Luno SDK Demonstration Gallery.");
    Console.WriteLine("Select a demonstration to explore the API capabilities:");
    Console.WriteLine();
    Console.WriteLine("1. Market Data (Heartbeat Demonstration)");
    Console.WriteLine("2. Dependency Injection Integration");
    Console.WriteLine("3. Account Balances (Authentication)");
    Console.WriteLine("4. Single Ticker (Targeted Retrieval)");
    Console.WriteLine("5. Ticker Wrapping (User Decorators)");
    Console.WriteLine("6. Limit Order Lifecycle (Private API)");
    Console.WriteLine("7. Real-Time Quote (Live Market Data)");
    Console.WriteLine("0. Exit");
    Console.WriteLine();
    Console.Write("Selection > ");

    choice = Console.ReadLine();
}

Console.WriteLine("-------------------------------------------------");

switch (choice)
{
    case "1":
        await Concept01_MarketData.RunAsync();
        break;
    case "2":
        await Concept02_DependencyInjection.RunAsync();
        break;
    case "3":
        await Concept03_AccountBalances.RunAsync();
        break;
    case "4":
        await Concept04_SingleTicker.RunAsync();
        break;
    case "5":
        await Concept05_TickerWrapping.RunAsync();
        break;
    case "6":
        await Concept06_Orders.RunAsync();
        break;
    case "7":
        await Concept07_RealTimeQuote.RunAsync();
        break;
    case "0":
        Console.WriteLine("Exiting application.");
        break;
    default:
        Console.WriteLine("Invalid selection. Please try again.");
        break;
}

Console.WriteLine("=================================================");
