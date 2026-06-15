using System;

namespace Frends.Odf.WriteSpreadsheet.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>File already exists: c:\temp\foo.ods</example>
    public string Message { get; set; }

    /// <summary>
    /// Additional information about the error.
    /// </summary>
    /// <example>System.IO.IOException</example>
    public Exception AdditionalInfo { get; set; }
}