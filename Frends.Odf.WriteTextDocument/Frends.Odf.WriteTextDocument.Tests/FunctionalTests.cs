using System;
using System.IO;
using System.Threading;
using Frends.Odf.WriteTextDocument.Definitions;
using Frends.Odf.WriteTextDocument.Tests.Helpers;
using NUnit.Framework;

namespace Frends.Odf.WriteTextDocument.Tests;

[TestFixture]
internal class FunctionalTests : TestBase
{
    [Test]
    public void Should_Write_Input_Content()
    {
        var input = DefaultInput();
        var options = DefaultOptions();

        var result = Odf.WriteTextDocument(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True, "Task failed to execute successfully.");
        Assert.That(File.Exists(result.FilePath), Is.True, "The output .odt file was not created.");

        var xmlString = TestHelper.ReadOdtContent(result.FilePath);

        Assert.That(xmlString, Contains.Substring("Name: John"));
        Assert.That(xmlString, Contains.Substring("Test: Test 1"));
        Assert.That(xmlString, Contains.Substring("Name: Doe"));
        Assert.That(xmlString, Contains.Substring("Test: Test 2"));
    }

    [Test]
    public void Should_Throw_When_Input_FilePath_Is_Incorrect()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "fake_path.odt");

        var input = DefaultInput();
        input.FilePath = path;

        var exception = Assert.Throws<Exception>(() => Odf.WriteTextDocument(input, DefaultOptions(), CancellationToken.None));

        Assert.That(exception.Message, Contains.Substring("Destination directory not found"));
    }

    [Test]
    public void Should_Throw_When_Input_Content_Is_Incorrect()
    {
        var invalidPayload = @"{ ""Name"": ""John"" }";

        var input = DefaultInput();
        input.Payload = invalidPayload;

        var exception = Assert.Throws<Exception>(() => Odf.WriteTextDocument(input, DefaultOptions(), CancellationToken.None));

        Assert.That(exception.Message, Contains.Substring("must be a valid array of objects"));
    }

    [Test]
    public void Should_Throw_When_ActionOnExistingFile_Is_Throw()
    {
        File.WriteAllText(ValidTestFilePath, "This is an existing file.");

        var input = DefaultInput();
        input.ActionOnExistingFile = ActionOnExistingFile.Throw;

        var exception = Assert.Throws<Exception>(() => Odf.WriteTextDocument(input, DefaultOptions(), CancellationToken.None));

        Assert.That(exception.Message, Contains.Substring("File already exists"));
    }

    [Test]
    public void Should_Overwrite_When_ActionOnExistingFile_Is_Overwrite()
    {
        File.WriteAllText(ValidTestFilePath, "This is an existing file.");

        var input = DefaultInput();
        input.ActionOnExistingFile = ActionOnExistingFile.Overwrite;

        var result = Odf.WriteTextDocument(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(result.FilePath), Is.True);

        var xmlString = TestHelper.ReadOdtContent(result.FilePath);

        Assert.That(xmlString, Contains.Substring("Name: John"));
        Assert.That(xmlString, Does.Not.Contain("This is an existing file."));
    }

    [Test]
    public void Should_Write_Empty_Document_With_Empty_Payload()
    {
        var emptyPayload = "[]";

        var input = DefaultInput();
        input.Payload = emptyPayload;

        var result = Odf.WriteTextDocument(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(result.FilePath), Is.True);

        var xmlString = TestHelper.ReadOdtContent(result.FilePath);

        Assert.That(xmlString, Does.Not.Contain("Name: John"));
        Assert.That(xmlString, Does.Not.Contain("text:p>"));
    }

    [Test]
    public void Should_Handle_Unicode_Content()
    {
        var unicodePayload = @"[
            { ""Text1"": ""AäÄaOöÖo."" },
            { ""Text2"": ""ÖöÄä."" }
        ]";

        var input = DefaultInput();
        input.Payload = unicodePayload;

        var result = Odf.WriteTextDocument(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(result.FilePath), Is.True);

        var xmlString = TestHelper.ReadOdtContent(result.FilePath);

        Assert.That(xmlString, Contains.Substring("Text1: AäÄaOöÖo."));
        Assert.That(xmlString, Contains.Substring("Text2: ÖöÄä."));
    }

    [Test]
    public void Should_Throw_When_File_Extension_Is_Not_Odt()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");

        var input = DefaultInput();
        input.FilePath = path;

        var exception = Assert.Throws<Exception>(() => Odf.WriteTextDocument(input, DefaultOptions(), CancellationToken.None));

        Assert.That(exception.Message, Contains.Substring("The destination file must have a .odt extension."));
    }

    [Test]
    public void Should_Throw_When_Array_Contains_Non_Objects()
    {
        var invalidPayload = @"[ ""This is a string."" ]";

        var input = DefaultInput();
        input.Payload = invalidPayload;

        var exception = Assert.Throws<Exception>(() => Odf.WriteTextDocument(input, DefaultOptions(), CancellationToken.None));

        Assert.That(exception.Message, Contains.Substring("The JSON payload must contain valid JSON objects."));
    }

    [Test]
    public void Should_Handle_Null_JSON_Values()
    {
        var nullPayload = @"[
            { ""Name"": ""John"", ""Role"": null }
        ]";

        var input = DefaultInput();
        input.Payload = nullPayload;

        var result = Odf.WriteTextDocument(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(File.Exists(result.FilePath), Is.True);

        var xmlString = TestHelper.ReadOdtContent(result.FilePath);

        Assert.That(xmlString, Contains.Substring("Name: John"));
        Assert.That(xmlString, Contains.Substring("Role: "));
    }
}