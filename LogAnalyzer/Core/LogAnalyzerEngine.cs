using System;
using System.Collections.Generic;
using System.Linq;

namespace LogAnalyzer.Core;

public class LogAnalyzerEngine : ILogAnalyzer
{
    public AnalysisResult Analyze(IList<LogEntry> logEntries)
    {
        if (logEntries == null || !logEntries.Any())
            return new AnalysisResult { TotalEntries = 0 };

        var result = new AnalysisResult
        {
            TotalEntries = logEntries.Count,
            ErrorCount = logEntries.Count(e => e.Level == LogLevel.ERROR || e.Level == LogLevel.CRITICAL || e.Level == LogLevel.FATAL),
            WarningCount = logEntries.Count(e => e.Level == LogLevel.WARNING),
            InfoCount = logEntries.Count(e => e.Level == LogLevel.INFO)
        };

        result.TopErrors = AnalyzeTopErrors(logEntries);

        result.ErrorsOverTime = AnalyzeErrorsOverTime(logEntries);

        result.ErrorsBySource = AnalyzeErrorsBySource(logEntries);

        return result;
    }

    private List<ErrorSummary> AnalyzeTopErrors(IList<LogEntry> logEntries)
    {
        return logEntries
            .Where(e => e.Level == LogLevel.ERROR || e.Level == LogLevel.CRITICAL || e.Level == LogLevel.FATAL)
            .GroupBy(e => NormalizeErrorMessage(e.Message))
            .Select(g => new ErrorSummary
            {
                Message = g.Key,
                Count = g.Count(),
                FirstOccurrence = g.Min(e => e.Timestamp),
                LastOccurrence = g.Max(e => e.Timestamp)
            })
            .OrderByDescending(e => e.Count)
            .ToList();
    }

    private Dictionary<DateTime, int> AnalyzeErrorsOverTime(IList<LogEntry> logEntries)
    {
        return logEntries
            .Where(e => e.Level >= LogLevel.ERROR)
            .GroupBy(e => TruncateToHour(e.Timestamp))
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<DateTime, int> AnalyzeErrorsBySource(IList<LogEntry> logEntries)
    {
        return logEntries
            .Where(e => e.Level >= LogLevel.ERROR && !string.IsNullOrEmpty(e.Source))
            .GroupBy(e => TruncateToHour(e.Timestamp))
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private string NormalizeErrorMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        message = System.Text.RegularExpressions.Regex.Replace(
            message,
            @"line \d+",
            "line [NUMBER]"
        );

        message = System.Text.RegularExpressions.Regex.Replace(
            message,
            @"\b\d+\b",
            "[NUMBER]"
        );

        message = System.Text.RegularExpressions.Regex.Replace(
            message,
            @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
            "[GUID]"
        );

        message = System.Text.RegularExpressions.Regex.Replace(
            message,
            @"[A-Za-z]:\\[^\s,;:""'<>]*",
            "[FILE_PATH]"
        );

        if (message.Length > 200)
            message = message.Substring(0, 197) + "...";

        return message;
    }

    private DateTime TruncateToHour(DateTime dateTime)
    {
        return new DateTime(
            dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            dateTime.Hour,
            0,
            0,
            dateTime.Kind
        );
    }
}