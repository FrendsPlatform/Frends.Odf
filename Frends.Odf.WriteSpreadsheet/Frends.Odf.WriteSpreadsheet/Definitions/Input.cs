using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Odf.WriteSpreadsheet.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// JSON data array to be written into the file.
    /// </summary>
    /// <example>[ { "Name": "John", "Age": 40 }, { "Name": "Jane", "Age": 30 } ]</example>
    [Required]
    [DefaultValue("")]
    public string Payload { get; set; }

    /// <summary>
    /// Full path of the destination for the new .ods file.
    /// </summary>
    /// <example>c:\temp\foo.ods</example>
    [Required]
    [DefaultValue("")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Defines how the file write should work if a file with the new name already exists.
    /// </summary>
    /// <example>ActionOnExistingFile.Throw</example>
    [DefaultValue(ActionOnExistingFile.Throw)]
    public ActionOnExistingFile ActionOnExistingFile { get; set; } = ActionOnExistingFile.Throw;
}