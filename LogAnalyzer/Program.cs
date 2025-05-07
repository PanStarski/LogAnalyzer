using LogAnalyzer.Core;
using LogAnalyzer.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=======================================");
            Console.WriteLine("=         LOG ANALYZER v1.0          =");
            Console.WriteLine("=======================================");

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            try
            {
                ProcessArguments(args);
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                ShowHelp();
            }
        }

        static void ProcessArguments(string[] args)
        {
            string filePath = string.Empty;
            string pattern = string.Empty;
            LogLevel? filterLevel = null;
            DateTime? startDate = null;
            DateTime? endDate = null;
            string outputPath = string.Empty;
            string reportFormat = "text";

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-f":
                    case "--file":
                        if (i + 1 < args.Length) filePath = args[++i];
                        break;
                    case "-p":
                    case "--pattern":
                        if (i + 1 < args.Length) pattern = args[++i];
                        break;
                    case "-l":
                    case "--level":
                        if (i + 1 < args.Length && Enum.TryParse<LogLevel>(args[++i], true, out var level))
                            filterLevel = level;
                        break;
                    case "-s":
                    case "--start-date":
                        if (i + 1 < args.Length && DateTime.TryParse(args[++i], out var start))
                            startDate = start;
                        break;
                    case "-e":
                    case "--end-date":
                        if (i + 1 < args.Length && DateTime.TryParse(args[++i], out var end))
                            endDate = end;
                        break;
                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length) outputPath = args[++i];
                        break;
                    case "-r":
                    case "--report-format":
                        if (i + 1 < args.Length) reportFormat = args[++i].ToLower();
                        break;
                    case "-h":
                    case "--help":
                        ShowHelp();
                        return;
                    default:
                        if (string.IsNullOrEmpty(filePath) && File.Exists(args[i]))
                            filePath = args[i];
                        break;
                }
            }

            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("Please specify a log file to analyze.");
                return;
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                var extension = reportFormat == "csv" ? "csv" : "txt";
                outputPath = Path.Combine(
                    Path.GetDirectoryName(filePath) ?? ".",
                    $"{Path.GetFileNameWithoutExtension(filePath)}_analysis.{extension}"
                );
            }

            var result = AnalyzeFile(filePath, pattern, filterLevel, startDate, endDate);
            GenerateReport(result, outputPath, reportFormat);
        }

        static AnalysisResult AnalyzeFile(string filePath, string pattern, LogLevel? filterLevel, DateTime? startDate, DateTime? endDate)
        {
            Console.WriteLine($"Analyzing file: {filePath}");

            var fileReader = new LogFileReader();
            var parser = LogParserFactory.GetParser(filePath);
            var analyzer = new LogAnalyzerEngine();

            Console.WriteLine("Reading and parsing log entries...");
            var logEntries = fileReader.ReadLogFile(filePath)
                .Select(line => parser.ParseLine(line))
                .Where(entry => entry != null)
                .ToList();

            Console.WriteLine($"Found {logEntries.Count()} log entries.");

            if (filterLevel.HasValue)
            {
                Console.WriteLine($"Filtering by level: {filterLevel.Value} and above");
                logEntries = logEntries.Where(entry => entry.Level >= filterLevel.Value).ToList();
            }

            if (!string.IsNullOrEmpty(pattern))
            {
                Console.WriteLine($"Filtering by pattern: {pattern}");
                logEntries = logEntries.Where(entry => Regex.IsMatch(entry.Message, pattern)).ToList();
            }

            if (startDate.HasValue)
            {
                Console.WriteLine($"Filtering by start date: {startDate.Value:yyyy-MM-dd HH:mm:ss}");
                logEntries = logEntries.Where(entry => entry.Timestamp >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                Console.WriteLine($"Filtering by end date: {endDate.Value:yyyy-MM-dd HH:mm:ss}");
                logEntries = logEntries.Where(entry => entry.Timestamp <= endDate.Value).ToList();
            }

            Console.WriteLine($"After filtering: {logEntries.Count()} log entries remain.");
            Console.WriteLine("Analyzing log data...");

            return analyzer.Analyze(logEntries);
        }

        static void GenerateReport(AnalysisResult results, string outputPath, string format)
        {
            var reportGenerator = new ReportGenerator();

            switch (format.ToLower())
            {
                case "csv":
                    reportGenerator.GenerateCsvReport(results, outputPath);
                    break;
                case "text":
                default:
                    reportGenerator.GenerateTextReport(results, outputPath);
                    break;
            }

            DisplayResults(results);
        }

        static void DisplayResults(AnalysisResult results)
        {
            Console.WriteLine("\nAnalysis Results:");
            Console.WriteLine($"Total log entries: {results.TotalEntries}");
            Console.WriteLine($"Error count: {results.ErrorCount}");
            Console.WriteLine($"Warning count: {results.WarningCount}");
            Console.WriteLine($"Info count: {results.InfoCount}");

            if (results.TopErrors.Any())
            {
                Console.WriteLine("\nTop 5 Errors:");
                foreach (var error in results.TopErrors.Take(5))
                {
                    Console.WriteLine($"[{error.Count}] {error.Message}");
                }
            }

            Console.WriteLine("\nSee the generated report for more detailed analysis.");
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: LogAnalyzer [options] [file]");
            Console.WriteLine("Options:");
            Console.WriteLine("  -f, --file <path>          Path to log file");
            Console.WriteLine("  -p, --pattern <regex>      Regular expression pattern to search for");
            Console.WriteLine("  -l, --level <level>        Filter by minimum log level (ERROR, WARNING, INFO, etc.)");
            Console.WriteLine("  -s, --start-date <date>    Filter entries after this date/time");
            Console.WriteLine("  -e, --end-date <date>      Filter entries before this date/time");
            Console.WriteLine("  -o, --output <path>        Output file path for reports");
            Console.WriteLine("  -r, --report-format <fmt>  Report format: text or csv (default: text)");
            Console.WriteLine("  -h, --help                 Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  LogAnalyzer app.log");
            Console.WriteLine("  LogAnalyzer -f app.log -l ERROR -o error_report.txt");
            Console.WriteLine("  LogAnalyzer -f app.log -p \"Exception\" -r csv");
        }
    }
}