// -----------------------------------------------------------------------
// Conduit CLI - Command-Line Tool for Querying Pipeline Output
//
// A read-only tool that works with the JSON files produced by the pipeline.
// Operates entirely on local data -- no network requests.
//
// COMMANDS:
//   conduit search <term> [--dir <path>]     Search items by keyword
//   conduit list [--dir <path>] [--limit <n>] List recent items
//   conduit stats [--dir <path>]              Show ingestion statistics
//
// RUN WITH: dotnet run --project src/Conduit.Cli -- list
//           dotnet run --project src/Conduit.Cli -- search "AI"
//           dotnet run --project src/Conduit.Cli -- stats
// -----------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using Conduit.Core.Models;

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

var dirOption = new Option<string>("--dir", () => "fetched", "Directory containing output JSON files");

var rootCommand = new RootCommand("Conduit CLI - search and filter pipeline output");

// ---- SEARCH COMMAND ----
var searchTerm = new Argument<string>("term", "Text to search for in titles and descriptions");
var searchCommand = new Command("search", "Search items by keyword") { searchTerm, dirOption };
searchCommand.SetHandler((term, dir) =>
{
    if (!Directory.Exists(dir))
    {
        Console.Error.WriteLine($"Directory not found: {dir}");
        return;
    }

    var files = Directory.GetFiles(dir, "*.json");
    var matchCount = 0;

    foreach (var file in files)
    {
        var json = File.ReadAllText(file);
        var items = JsonSerializer.Deserialize<List<FeedItem>>(json, jsonOptions) ?? [];

        var matches = items.Where(i =>
            i.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            i.Description.Contains(term, StringComparison.OrdinalIgnoreCase));

        foreach (var item in matches)
        {
            matchCount++;
            Console.WriteLine($"[{item.PublishedDate:yyyy-MM-dd}] {item.Title}");
            Console.WriteLine($"  {item.Link}");
            Console.WriteLine();
        }
    }

    Console.WriteLine($"Found {matchCount} result(s).");
}, searchTerm, dirOption);

// ---- LIST COMMAND ----
var limitOption = new Option<int>("--limit", () => 10, "Number of items to show");
var listCommand = new Command("list", "List recent items") { dirOption, limitOption };
listCommand.SetHandler((dir, limit) =>
{
    if (!Directory.Exists(dir))
    {
        Console.Error.WriteLine($"Directory not found: {dir}");
        return;
    }

    var latestFile = Directory.GetFiles(dir, "*.json")
        .OrderByDescending(f => f)
        .FirstOrDefault();

    if (latestFile is null)
    {
        Console.WriteLine("No data found.");
        return;
    }

    var json = File.ReadAllText(latestFile);
    var items = JsonSerializer.Deserialize<List<FeedItem>>(json, jsonOptions) ?? [];

    Console.WriteLine($"Latest output: {Path.GetFileName(latestFile)}");
    Console.WriteLine(new string('-', 60));

    foreach (var item in items.Take(limit))
    {
        Console.WriteLine($"[{item.PublishedDate:yyyy-MM-dd}] {item.Title}");
        Console.WriteLine($"  {item.Link}");
        Console.WriteLine();
    }

    Console.WriteLine($"Showing {Math.Min(limit, items.Count)} of {items.Count} items.");
}, dirOption, limitOption);

// ---- STATS COMMAND ----
var statsCommand = new Command("stats", "Show ingestion statistics") { dirOption };
statsCommand.SetHandler((dir) =>
{
    if (!Directory.Exists(dir))
    {
        Console.Error.WriteLine($"Directory not found: {dir}");
        return;
    }

    var files = Directory.GetFiles(dir, "*.json");
    var totalItems = 0;

    foreach (var file in files)
    {
        var json = File.ReadAllText(file);
        var items = JsonSerializer.Deserialize<List<FeedItem>>(json, jsonOptions) ?? [];
        totalItems += items.Count;
    }

    Console.WriteLine($"Output files:   {files.Length}");
    Console.WriteLine($"Total items:    {totalItems}");
    Console.WriteLine($"Directory:      {Path.GetFullPath(dir)}");
}, dirOption);

rootCommand.AddCommand(searchCommand);
rootCommand.AddCommand(listCommand);
rootCommand.AddCommand(statsCommand);

return await rootCommand.InvokeAsync(args);
