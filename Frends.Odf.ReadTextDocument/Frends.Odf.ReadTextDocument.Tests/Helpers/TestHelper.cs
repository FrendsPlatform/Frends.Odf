using System;
using System.IO;
using System.IO.Compression;

namespace Frends.Odf.ReadTextDocument.Tests.Helpers
{
    internal class TestHelper
    {
        /// <summary>
        /// Creates a .odt file for testing.
        /// </summary>
        /// <param name="contentXml">The XML string to inject into content.xml.</param>
        /// <returns>The temporary file path to the generated .odt file.</returns>
        internal static string CreateTestFile(string contentXml)
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".odt");

            using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
            var entry = archive.CreateEntry("content.xml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(contentXml);

            return path;
        }
    }
}
