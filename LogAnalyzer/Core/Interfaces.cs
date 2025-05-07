using System.Collections.Generic;

namespace LogAnalyzer.Core;

public interface ILogParser
{
    LogEntry ParseLine(string line);
    bool CanParse(string line);
    string ParserName { get; }
}

public interface ILogFileReader
{
    IEnumerable<string> ReadLogFile(string filePath);
}

public interface ILogAnalyzer
{
    AnalysisResult Analyze(IList<LogEntry> logEntries);
}