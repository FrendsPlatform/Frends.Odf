using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Frends.Odf.ReadSpreadsheet.Tests.Helpers
{
    internal class TestHelper : IDisposable
    {
        private readonly List<string> filesCreated = [];

        public void Dispose()
        {
            foreach (var file in filesCreated)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Creates a standard .ods file with the provided XML content.
        /// </summary>
        /// <param name="contentXml">XML content to inject.</param>
        /// <returns>.ods file with XML content injected.</returns>
        internal string CreateTestFile(string contentXml)
        {
            var path = GeneratePath();
            using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
            var entry = archive.CreateEntry("content.xml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(contentXml);
            return path;
        }

        /// <summary>
        /// Creates a temporary .ods zip file without injecting content.xml.
        /// </summary>
        /// <returns>Empty .ods file.</returns>
        internal string CreateMissingContentXmlFile()
        {
            var path = GeneratePath();
            ZipFile.Open(path, ZipArchiveMode.Create).Dispose();
            return path;
        }

        /// <summary>
        /// Creates a temporary .ods file with standard text instead of a valid Zip archive.
        /// </summary>
        /// <returns>Corrupt .ods file.</returns>
        internal string CreateCorruptFile()
        {
            var path = GeneratePath();
            File.WriteAllText(path, "This is corrupt file input.");
            return path;
        }

        /// <summary>
        /// Creates a unique path in the temp folder.
        /// </summary>
        /// <returns>Unique temp path.</returns>
        private string GeneratePath()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ods");
            filesCreated.Add(path);
            return path;
        }
    }
}