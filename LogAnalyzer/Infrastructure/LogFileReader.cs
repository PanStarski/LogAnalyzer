using LogAnalyzer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogAnalyzer.Infrastructure;

public class LogFileReader : ILogFileReader
{
    public IEnumerable<string> ReadLogFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Log file not found: {filePath}");
        }

        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"Proccessing file ({fileInfo.Length / 1024:N0} KB)");

        Encoding encoding = DetectEncoding(filePath);
        Console.WriteLine($"Detected encoding: {encoding.WebName}");

        using var reader = new StreamReader(filePath, encoding);
        string line;
        int lineCount = 0;

        StringBuilder logEntryBuffer = new();
        bool inMultiLineEntry = false;

        while ((line = reader.ReadLine()) != null)
        {
            lineCount++;

            if (IsStartOfLogEntry(line))
            {
                if (logEntryBuffer.Length > 0)
                {
                    yield return logEntryBuffer.ToString();
                    logEntryBuffer.Clear();
                }

                logEntryBuffer.AppendLine(line);
                inMultiLineEntry = true;
            }
            else if (inMultiLineEntry)
            {
                logEntryBuffer.AppendLine(line);
            }
            else
            {
                yield return line;
            }
            if (lineCount % 10000 == 0)
                Console.WriteLine($"Processed {lineCount:N0} lines...");
        }

        if (logEntryBuffer.Length > 0)
            yield return logEntryBuffer.ToString();

        Console.WriteLine($"Finished processing {lineCount:N0} lines.");
    }

    private bool IsStartOfLogEntry(string line)
    {
        return !string.IsNullOrWhiteSpace(line) &&
               (line.Length > 10 && (
                   char.IsDigit(line[0]) && line[1] == '-' && char.IsDigit(line[2]) ||
                   char.IsDigit(line[0]) && line[2] == '/' && char.IsDigit(line[3]) ||
                   char.IsDigit(line[0]) && line[2] == '.' && char.IsDigit(line[3])
               ));
    }

    private Encoding DetectEncoding(string filePath)
    {
        using var reader = new StreamReader(filePath, Encoding.ASCII, true);
        reader.Peek();
        return reader.CurrentEncoding;
    }
}
