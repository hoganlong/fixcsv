using System;
using System.IO;
using System.Linq;

const string NewHeader = "Filename,DESCRIPTION,SKETCHBOOK_NUMBER,PAGE_NUMBER,SKETCH_DT,,";

void PrintUsage()
{
    Console.WriteLine("Usage: FixCsv <directory>");
    Console.WriteLine();
    Console.WriteLine("  <directory>   folder containing *.csv files to normalize in-place");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -h, --help, -?, /?, ?   show this help and exit");
    Console.WriteLine();
    Console.WriteLine("Replaces the header on every CSV in the folder and strips a trailing");
    Console.WriteLine(".tif extension from the first (Filename) field on each row.");
}

if (args.Any(a => a is "-h" or "--help" or "-?" or "/?" or "?"))
{
    PrintUsage();
    return 0;
}
foreach (var a in args)
{
    if (a.StartsWith("-") || a.StartsWith("/"))
    {
        Console.WriteLine($"Unknown option: {a}");
        Console.WriteLine();
        PrintUsage();
        return 1;
    }
}

if (args.Length < 1)
{
    PrintUsage();
    return 1;
}

string directory = args[0];

if (!Directory.Exists(directory))
{
    Console.WriteLine($"Directory not found: {directory}");
    return 1;
}

string[] csvFiles = Directory.GetFiles(directory, "*.csv", SearchOption.TopDirectoryOnly);

if (csvFiles.Length == 0)
{
    Console.WriteLine("No CSV files found.");
    return 0;
}

foreach (string filePath in csvFiles)
{
    Console.Write($"Processing: {Path.GetFileName(filePath)} ... ");

    string[] lines = File.ReadAllLines(filePath);

    string[] output = new string[lines.Length];

    // Replace header
    output[0] = NewHeader;

    // Process remaining lines
    for (int i = 1; i < lines.Length; i++)
    {
        string line = lines[i];

        if (string.IsNullOrEmpty(line))
        {
            output[i] = line;
            continue;
        }

        // Find the end of the first field (up to the first comma)
        int commaIndex = line.IndexOf(',');
        string firstField = commaIndex >= 0 ? line[..commaIndex] : line;
        string remainder = commaIndex >= 0 ? line[commaIndex..] : string.Empty;

        // Strip .tif from first field if present (case-insensitive)
        if (firstField.EndsWith(".tif", StringComparison.OrdinalIgnoreCase))
            firstField = firstField[..^4];

        output[i] = firstField + remainder;
    }

    File.WriteAllLines(filePath, output);
    Console.WriteLine("done");
}

Console.WriteLine($"\nProcessed {csvFiles.Length} file(s).");
return 0;
