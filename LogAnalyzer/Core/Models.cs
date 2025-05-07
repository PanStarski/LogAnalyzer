using System;
using System.Collections.Generic;

namespace LogAnalyzer.Core;

public enum LogLevel
{
    TRACE,
    DEBUG,
    INFO,
    WARNING,
    ERROR,
    CRITICAL,
    FATAL
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; }
    public string Source { get; set; }
    public string Exception { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
    public override string ToString()
    {
        return $"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Source}: {Message}";
    }
}

public class ErrorSummary
{
    public string Message { get; set; }
    public int Count { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
}

public class AnalysisResult
{
    public int TotalEntries { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public List<ErrorSummary> TopErrors { get; set; } = new();
    public Dictionary<DateTime, int> ErrorsOverTime { get; set; } = new();
    public Dictionary<DateTime, int> ErrorsBySource { get; set; } = new();
}