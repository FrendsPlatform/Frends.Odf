using System;
using System.Threading;
using NUnit.Framework;

namespace Frends.Odf.ReadTextDocument.Tests;

[TestFixture]
internal class FunctionalTests : TestBase
{
    [Test]
    public void Should_Extract_File_Content()
    {
        var result = Odf.ReadTextDocument(DefaultInput(), DefaultOptions(), CancellationToken.None);

        Assert.IsTrue(result.Success, "Task failed to execute successfully.");

        var expectedOutput = "Test Heading" + Environment.NewLine + "Test paragraph 1." + Environment.NewLine + "Test paragraph 2.";
        Assert.AreEqual(expectedOutput, result.Content);
    }
}