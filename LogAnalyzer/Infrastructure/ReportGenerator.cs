using LogAnalyzer.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Infrastructure;

public class ReportGenerator
{
    public void GenerateCsvReport(AnalysisResult result, string outputPath)
    {
        using var writer = new StreamWriter(outputPath, false, Encoding.UTF8);

        writer.WriteLine("Log Analysis Report");
        writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine();
        writer.WriteLine("Summary Statistics");
        writer.WriteLine($"Total entries,{result.TotalEntries}");
        writer.WriteLine($"Error count,{result.ErrorCount}");
        writer.WriteLine($"Warning count,{result.WarningCount}");
        writer.WriteLine($"Info count,{result.InfoCount}");
        writer.WriteLine();

        writer.WriteLine("Top Errors");
        writer.WriteLine("Count,First Occurrence,Last Occurrence,Message");
        foreach (var error in result.TopErrors)
        {
            writer.WriteLine($"{error.Count},{error.FirstOccurrence:yyyy-MM-dd HH:mm:ss},{error.LastOccurrence:yyyy-MM-dd HH:mm:ss},\"{EscapeCsvField(error.Message)}\"");
        }
        writer.WriteLine();

        writer.WriteLine("Errors Over Time");
        writer.WriteLine("Timestamp,Count");
        foreach (var item in result.ErrorsOverTime.OrderBy(x => x.Key))
        {
            writer.WriteLine($"{item.Key:yyyy-MM-dd HH:mm:ss},{item.Value}");
        }
        writer.WriteLine();

        writer.WriteLine("Errors By Source");
        writer.WriteLine("Source,Count");
        foreach (var item in result.ErrorsBySource.OrderByDescending(x => x.Value))
        {
            writer.WriteLine($"\"{EscapeCsvField(item.Key.ToString())}\",{item.Value}");
        }

        Console.WriteLine($"Report saved to: {outputPath}");
    }

    public void GenerateTextReport(AnalysisResult result, string outputPath)
    {
        using var writer = new StreamWriter(outputPath, false, Encoding.UTF8);

        writer.WriteLine("==============================================");
        writer.WriteLine("             LOG ANALYSIS REPORT              ");
        writer.WriteLine("==============================================");
        writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine();

        writer.WriteLine("SUMMARY STATISTICS");
        writer.WriteLine("------------------");
        writer.WriteLine($"Total log entries: {result.TotalEntries}");
        writer.WriteLine($"Error count:       {result.ErrorCount}");
        writer.WriteLine($"Warning count:     {result.WarningCount}");
        writer.WriteLine($"Info count:        {result.InfoCount}");
        writer.WriteLine();

        writer.WriteLine("TOP ERRORS");
        writer.WriteLine("----------");
        if (result.TopErrors.Any())
        {
            foreach (var error in result.TopErrors.Take(10))
            {
                writer.WriteLine($"[{error.Count,4}] {error.Message}");
                writer.WriteLine($"       First: {error.FirstOccurrence:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"       Last:  {error.LastOccurrence:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();
            }
        }
        else
        {
            writer.WriteLine("No errors found in the log file.");
        }
        writer.WriteLine();

        writer.WriteLine("ERRORS BY SOURCE");
        writer.WriteLine("---------------");
        if (result.ErrorsBySource.Any())
        {
            foreach (var item in result.ErrorsBySource.OrderByDescending(x => x.Value))
            {
                writer.WriteLine($"{item.Key}: {item.Value}");
            }
        }
        else
        {
            writer.WriteLine("No source information available in the log file.");
        }

        Console.WriteLine($"Text report saved to: {outputPath}");
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return field.Replace("\"", "\"\"");
        }

        return field;
    }
}

