namespace Frends.Odf.WriteTextDocument.Definitions;

/// <summary>
/// Action to take if the destination file already exists.
/// </summary>
public enum ActionOnExistingFile
{
    /// <summary>
    /// Replaces the existing file.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Throws an error if the file already exists.
    /// </summary>
    Throw,
}