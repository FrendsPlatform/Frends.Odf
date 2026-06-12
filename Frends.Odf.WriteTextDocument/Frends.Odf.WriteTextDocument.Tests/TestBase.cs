using System;
using System.IO;
using Frends.Odf.WriteTextDocument.Definitions;
using NUnit.Framework;

namespace Frends.Odf.WriteTextDocument.Tests;

internal abstract class TestBase
{
    protected string ValidTestFilePath { get; private set; }

    [SetUp]
    public void SetupBase()
    {
        ValidTestFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".odt");
    }

    [TearDown]
    public void TearDownBase()
    {
        if (File.Exists(ValidTestFilePath))
        {
            File.Delete(ValidTestFilePath);
        }
    }

    protected static Options DefaultOptions() => new();

    protected Input DefaultInput()
    {
        var jsonPayload = @"[
            { ""Name"": ""John"", ""Test"": ""Test 1"" },
            { ""Name"": ""Doe"", ""Test"": ""Test 2"" }
        ]";

        return new Input
        {
            FilePath = ValidTestFilePath,
            Payload = jsonPayload,
            ActionOnExistingFile = ActionOnExistingFile.Overwrite,
        };
    }
}