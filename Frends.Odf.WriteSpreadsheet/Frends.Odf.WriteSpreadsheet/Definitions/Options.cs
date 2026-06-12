using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Odf.WriteSpreadsheet.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// If true, the Task will recognise the keys of the JSON payload as headers and write them into the .ods file as the first row of the spreadsheet.
    /// If false, the Task will only write the values of the JSON payload and not implement the keys as headers.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool IncludeHeaderRow { get; set; } = true;

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}