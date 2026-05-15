using System;
using System.Threading;
using NUnit.Framework;

namespace Frends.Odf.ReadTextDocument.Tests;

[TestFixture]
internal class FunctionalTests : TestBase
{
    // Tests the main read functionality using default definition parameters and the test ODF XML framework generated in TestBase.cs.
    [Test]
    public void Should_Extract_File_Content()
    {
        var result = Odf.ReadTextDocument(DefaultInput(), DefaultOptions(), CancellationToken.None);

        Assert.IsTrue(result.Success, "Task failed to execute successfully.");

        // Compare the extracted content from the Task with the expected output.
        var expectedOutput = "Test Heading" + Environment.NewLine + "Test paragraph 1." + Environment.NewLine + "Test paragraph 2.";
        Assert.AreEqual(expectedOutput, result.Content);
    }
}