LogAnalyzer
A console tool for log file analysis, written in C# 9.0.
Features

Reading and parsing various log file formats (.NET, Apache, general formats)
Multi-line entry analysis (e.g., stack traces)
Filtering logs by:

Level (ERROR, WARNING, INFO, etc.)
Regular expressions
Time ranges


Log entry analysis:

Most frequent errors
Error distribution over time
Error sources


Report generation in formats:

Text (TXT)
CSV



Requirements

.NET 9.0 or newer

Installation

Clone the repository
Compile the project: dotnet build
Run the application: dotnet run -- [options]

Usage
LogAnalyzer [options] [file]

Options:
  -f, --file <path>          Path to the log file
  -p, --pattern <regex>      Regular expression pattern for searching
  -l, --level <level>        Filter by minimum logging level (ERROR, WARNING, INFO, etc.)
  -s, --start-date <date>    Filter entries after this date/time
  -e, --end-date <date>      Filter entries before this date/time
  -o, --output <path>        Output file path for reports
  -r, --report-format <fmt>  Report format: text or csv (default: text)
  -h, --help                 Display this help message

Examples
Analyze all entries in a file:
LogAnalyzer app.log
Analyze only errors:
LogAnalyzer -f app.log -l ERROR -o error_report.txt
Search for exceptions and export to CSV:
LogAnalyzer -f app.log -p "Exception" -r csv
Output Format
The text report includes:

Summary statistics (number of all entries, errors, warnings, information)
Top 10 most frequent errors
Distribution of errors by source

The CSV report contains the same information in a format suitable for further analysis in spreadsheets.
Project Development
Future development plans:

Adding language model-based analysis
Data visualization (charts)
Anomaly detection
Web interface
Support for more log formats
RetryClaude does not have the ability to run the code it generates yet.Claude can make mistakes. Please double-check responses.