using System;
using System.Threading;
using Frends.Odf.ReadTextDocument.Tests.Helpers;
using NUnit.Framework;

namespace Frends.Odf.ReadTextDocument.Tests;

[TestFixture]
internal class FunctionalTests : TestBase
{
    [Test]
    public void Should_Extract_File_Content()
    {
        var result = Odf.ReadTextDocument(DefaultInput(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True, "Task failed to execute successfully.");

        var expectedOutput = "Test Heading" + Environment.NewLine + "Test paragraph 1." + Environment.NewLine + "Test paragraph 2.";
        Assert.That(result.Content, Is.EqualTo(expectedOutput));
    }

    [Test]
    public void Should_Throw_When_ContentXml_Is_Missing()
    {
        using var testHelper = new TestHelper();
        var path = testHelper.CreateMissingContentXmlFile();

        var input = DefaultInput();
        input.FilePath = path;

        var exception = Assert.Throws<Exception>(() => Odf.ReadTextDocument(input, DefaultOptions(), CancellationToken.None));

        Assert.That(exception.Message, Contains.Substring("content.xml is missing"));
    }

    [Test]
    public void Should_Return_Empty_String()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <office:document-content xmlns:office=""urn:oasis:names:tc:opendocument:xmlns:office:1.0"" xmlns:text=""urn:oasis:names:tc:opendocument:xmlns:text:1.0"">
                <office:body>
                    <office:text>
                    </office:text>
                </office:body>
            </office:document-content>";

        using var testHelper = new TestHelper();
        var path = testHelper.CreateTestFile(xml);

        var input = DefaultInput();
        input.FilePath = path;

        var result = Odf.ReadTextDocument(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Content, Is.Empty);
    }

    [Test]
    public void Should_Return_Failure_With_Corrupt_File_Input()
    {
        using var testHelper = new TestHelper();
        var path = testHelper.CreateCorruptFile();

        var input = DefaultInput();
        input.FilePath = path;

        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = Odf.ReadTextDocument(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void Should_Handle_Unicode_Content()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <office:document-content xmlns:office=""urn:oasis:names:tc:opendocument:xmlns:office:1.0"" xmlns:text=""urn:oasis:names:tc:opendocument:xmlns:text:1.0"">
                <office:body>
                    <office:text>
                        <text:h>AäÄaOöÖo.</text:h>
                        <text:p>ÖöÄä.</text:p>
                    </office:text>
                </office:body>
            </office:document-content>";

        using var testHelper = new TestHelper();
        var path = testHelper.CreateTestFile(xml);

        var input = DefaultInput();
        input.FilePath = path;

        var result = Odf.ReadTextDocument(input, DefaultOptions(), CancellationToken.None);

        var expectedOutput = "AäÄaOöÖo." + Environment.NewLine + "ÖöÄä.";

        Assert.That(result.Success, Is.True);
        Assert.That(result.Content, Is.EqualTo(expectedOutput));
    }

    [Test]
    public void Should_Handle_Malformed_Xml()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <office:document-content xmlns:office=""urn:oasis:names:tc:opendocument:xmlns:office:1.0"" xmlns:text=""urn:oasis:names:tc:opendocument:xmlns:text:1.0"">
                <office:body>
                    <office:text>
                        <text:p>This tag never closes.
                    </office:text>
                </office:body>
            </office:document-content>";

        using var testHelper = new TestHelper();
        var path = testHelper.CreateTestFile(xml);

        var input = DefaultInput();
        input.FilePath = path;

        var exception = Assert.Throws<Exception>(() => Odf.ReadTextDocument(input, DefaultOptions(), CancellationToken.None));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_Handle_Odf_Elements()
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <office:document-content xmlns:office=""urn:oasis:names:tc:opendocument:xmlns:office:1.0"" xmlns:text=""urn:oasis:names:tc:opendocument:xmlns:text:1.0"">
                <office:body>
                    <office:text>
                        <text:p>Test<text:tab/>Paragraph 1.</text:p>
                        <text:p>Test<text:s text:c=""4""/> Paragraph<text:line-break/> 2.</text:p>
                    </office:text>
                </office:body>
            </office:document-content>";

        using var testHelper = new TestHelper();
        var path = testHelper.CreateTestFile(xml);

        var input = DefaultInput();
        input.FilePath = path;

        var result = Odf.ReadTextDocument(input, DefaultOptions(), CancellationToken.None);

        var expectedOutput = "Test\tParagraph 1." + Environment.NewLine + "Test     Paragraph" + Environment.NewLine + " 2.";

        Assert.That(result.Success, Is.True);
        Assert.That(result.Content, Is.EqualTo(expectedOutput));
    }
}