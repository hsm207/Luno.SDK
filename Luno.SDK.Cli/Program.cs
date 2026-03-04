using Luno.SDK.Cli.Concepts;

Console.WriteLine("Luno.SDK - Demonstration Gallery");
Console.WriteLine("=================================================");
Console.WriteLine("Welcome to the Luno SDK Demonstration Gallery.");
Console.WriteLine("Select a demonstration to explore the API capabilities:");
Console.WriteLine();
Console.WriteLine("1. Market Data (Heartbeat Demonstration)");
Console.WriteLine("2. Dependency Injection Integration");
Console.WriteLine("3. [Future] Account Management");
Console.WriteLine("0. Exit");
Console.WriteLine();
Console.Write("Selection > ");

var choice = Console.ReadLine();

Console.WriteLine("-------------------------------------------------");

switch (choice)
{
    case "1":
        await Concept01_MarketData.RunAsync();
        break;
    case "2":
        await Concept02_DependencyInjection.RunAsync();
        break;
    case "0":
        Console.WriteLine("Exiting application.");
        break;
    default:
        Console.WriteLine("Invalid selection. Please try again.");
        break;
}

Console.WriteLine("=================================================");
