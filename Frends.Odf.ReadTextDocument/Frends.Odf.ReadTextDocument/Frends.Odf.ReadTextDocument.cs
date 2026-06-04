using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Frends.Odf.ReadTextDocument.Definitions;
using Frends.Odf.ReadTextDocument.Helpers;

namespace Frends.Odf.ReadTextDocument;

/// <summary>
/// Task Class for reading Odf text documents.
/// </summary>
public static class Odf
{
    private static readonly XNamespace TextNamespace = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";

    /// <summary>
    /// Extracts readable text from OpenDocument Text (.odt) files.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Odf-ReadTextDocument)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Content, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result ReadTextDocument(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, options);

            if (!File.Exists(input.FilePath))
                throw new FileNotFoundException($"Input file not found: {input.FilePath}");

            if (!Path.GetExtension(input.FilePath).Equals(".odt", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The input file is not in .odt format.");

            var normalizedPath = Path.GetFullPath(input.FilePath);

            using ZipArchive archive = ZipFile.OpenRead(normalizedPath);

            var contentXml = archive.GetEntry("content.xml") ?? throw new Exception("content.xml is missing from the .odt file.");

            // Check the unzipped file size is below 50MB to prevent zip bombing.
            if (contentXml.Length > 50 * 1024 * 1024)
                throw new Exception("content.xml is larger than the maximum allowed file size of 50MB.");

            cancellationToken.ThrowIfCancellationRequested();

            using var stream = contentXml.Open();

            // Configure XmlReader to disable DTDs and external entities.
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
            };

            using var xmlReader = XmlReader.Create(stream, settings);
            var xDocument = XDocument.Load(xmlReader);

            // Extract <text:p> and <text:h> elements in document order, ignoring nested duplicates.
            var textElements = xDocument.Descendants()
                .Where(x => (x.Name == TextNamespace + "p" || x.Name == TextNamespace + "h")
                         && !x.Ancestors().Any(a => a.Name == TextNamespace + "p" || a.Name == TextNamespace + "h"))
                .Select(x => Helpers.OdfTextParser.ParseOdfElements(x, TextNamespace, cancellationToken));

            var extractedContent = string.Join(Environment.NewLine, textElements);

            return new Result
            {
                Success = true,
                Content = extractedContent,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }
}