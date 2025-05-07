using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogAnalyzer.Core;

public static class LogParserFactory
{
    public static ILogParser GetParser(string filePath)
    {
        var sampleLines = GetSampleLines(filePath, 5);

        var parsers = new ILogParser[]
        {
            new DotNetLogParser(),
            new ApacheLogParser(),
            new DefaultLogParser()
        };

        foreach (var parser in parsers)
        {
            foreach (var line in sampleLines)
            {
                if (parser.CanParse(line))
                {
                    Console.WriteLine($"Using parser: {parser.ParserName}");
                    return parser;
                }
            }
        }

        Console.WriteLine("No suitable parser found. Using default parser.");
        return new DefaultLogParser();
    }

    private static string[] GetSampleLines(string filePath, int count)
    {
        try
        {
            return File.ReadLines(filePath).Take(count).ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public class DotNetLogParser : ILogParser
    {
        private static readonly Regex LogPattern = new(
            @"^(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}.\d{3})\s+\[(\w+)\]\s+(\w+)\s+-\s+(.*?)(?:\r\n|\n|$)",
            RegexOptions.Compiled | RegexOptions.Multiline
        );

        public string ParserName => ".NET Logger";

        public bool CanParse(string sampleLine)
        {
            return LogPattern.IsMatch(sampleLine);
        }

        public LogEntry ParseLine(string line)
        {
            var match = LogPattern.Match(line);
            if (!match.Success) return null;
            var timestamp = DateTime.Parse(match.Groups[1].Value);
            var level = Enum.TryParse<LogLevel>(match.Groups[2].Value, true, out var logLevel) ? logLevel : LogLevel.INFO;
            var source = match.Groups[3].Value;
            var message = match.Groups[4].Value;
            return new LogEntry
            {
                Timestamp = timestamp,
                Level = level,
                Source = source,
                Message = message
            };
        }

        private LogLevel ParseLogLevel(string levelStr)
        {
            return levelStr.ToUpper() switch
            {
                "TRACE" => LogLevel.TRACE,
                "DEBUG" => LogLevel.DEBUG,
                "INFO" => LogLevel.INFO,
                "WARN" or "WARNING" => LogLevel.WARNING,
                "ERROR" => LogLevel.ERROR,
                "CRITICAL" => LogLevel.CRITICAL,
                "FATAL" => LogLevel.FATAL,
                _ => LogLevel.INFO
            };
        }
    }

    public class ApacheLogParser : ILogParser
    {
        private static readonly Regex LogPattern = new(
            @"^(\S+)\s+(\S+)\s+(\S+)\s+\[(.*?)\]\s+""(.*?)""\s+(\d{3})\s+(\d+|-)""(.*?)""",
            RegexOptions.Compiled | RegexOptions.Multiline
        );
        public string ParserName => "Apache Log";
        public bool CanParse(string sampleLine)
        {
            return LogPattern.IsMatch(sampleLine);
        }
        public LogEntry ParseLine(string line)
        {
            var match = LogPattern.Match(line);
            if (!match.Success) return null;
            var timestamp = DateTime.Parse(match.Groups[4].Value);
            var level = Enum.TryParse<LogLevel>(match.Groups[6].Value, true, out var logLevel) ? logLevel : LogLevel.INFO;
            var source = match.Groups[1].Value;
            var message = match.Groups[5].Value;
            return new LogEntry
            {
                Timestamp = timestamp,
                Level = level,
                Source = source,
                Message = message
            };
        }
    }

    public class DefaultLogParser : ILogParser
    {
        private static readonly Regex DateTimePattern = new(
            @"(\d{4}-\d{2}-\d{2}[T\s]\d{2}:\d{2}:\d{2}(?:\.\d+)?)",
            RegexOptions.Compiled
        );

        private static readonly Regex LevelPattern = new(
            @"\b(TRACE|DEBUG|INFO|WARN(?:ING)?|ERROR|CRITICAL|FATAL)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public string ParserName => "Default Log Format";

        public bool CanParse(string sampleLine)
        {
            return true;
        }

        public LogEntry ParseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            var entry = new LogEntry
            {
                Message = line,
                Level = LogLevel.INFO,
                Timestamp = DateTime.Now
            };

            var dateMatch = DateTimePattern.Match(line);
            if (dateMatch.Success)
            {
                entry.Timestamp = DateTime.Parse(dateMatch.Groups[1].Value);
                entry.Message = line;
            }

            var levelMatch = LevelPattern.Match(line);
            if (levelMatch.Success)
            {
                entry.Level = ParseLogLevel(levelMatch.Groups[1].Value);
            }

            return entry;
        }

        private LogLevel ParseLogLevel(string levelStr)
        {
            return levelStr.ToUpper() switch
            {
                "TRACE" => LogLevel.TRACE,
                "DEBUG" => LogLevel.DEBUG,
                "INFO" => LogLevel.INFO,
                "WARN" or "WARNING" => LogLevel.WARNING,
                "ERROR" => LogLevel.ERROR,
                "CRITICAL" => LogLevel.CRITICAL,
                "FATAL" => LogLevel.FATAL,
                _ => LogLevel.INFO
            };
        }
    }
}