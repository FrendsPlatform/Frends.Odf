using System;
using System.IO;
using System.IO.Compression;
using Frends.Odf.ReadTextDocument.Definitions;
using NUnit.Framework;

namespace Frends.Odf.ReadTextDocument.Tests;

internal abstract class TestBase
{
    // Input file path variable available to all test classes.
    protected string ValidTestFilePath { get; private set; }

    [SetUp]
    public void SetupBase()
    {
        // Initialise a temporary path for the .odt file used in tests.
        ValidTestFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".odt");

        // Create a zip archive at the temporary path.
        using var archive = ZipFile.Open(ValidTestFilePath, ZipArchiveMode.Create);
        var entry = archive.CreateEntry("content.xml");
        using var writer = new StreamWriter(entry.Open());

        // Inject a basic ODF XML framework into the zip archive.
        writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?>
            <office:document-content xmlns:office=""urn:oasis:names:tc:opendocument:xmlns:office:1.0"" xmlns:text=""urn:oasis:names:tc:opendocument:xmlns:text:1.0"">
                <office:body>
                    <office:text>
                        <text:h>Test Heading</text:h>
                        <text:p>Test paragraph 1.</text:p>
                        <text:p>Test paragraph 2.</text:p>
                    </office:text>
                </office:body>
            </office:document-content>");
    }

    [TearDown]
    public void TearDownBase()
    {
        // Delete the temporary file after each test runs.
        if (File.Exists(ValidTestFilePath))
        {
            File.Delete(ValidTestFilePath);
        }
    }

    protected static Options DefaultOptions() => new();

    protected Input DefaultInput() => new()
    {
        // Assigns the input file path variable to the default Input used in tests.
        FilePath = ValidTestFilePath,
    };
}