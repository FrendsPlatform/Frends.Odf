using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace Frends.Odf.WriteTextDocument.Tests;

[TestFixture]
internal class ErrorHandlerTest : TestBase
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    // Fake path to trigger DirectoryNotFoundException.
    private readonly string fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "fake_path.odt");

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        // Inject the fake path into the default input.
        var input = DefaultInput();
        input.FilePath = fakePath;

        var ex = Assert.Throws<Exception>(() =>
           Odf.WriteTextDocument(input, DefaultOptions(), CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        // Inject the fake path into the default input.
        var input = DefaultInput();
        input.FilePath = fakePath;

        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = Odf.WriteTextDocument(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        // Inject the fake path into the default input.
        var input = DefaultInput();
        input.FilePath = fakePath;

        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;

        var ex = Assert.Throws<Exception>(() =>
             Odf.WriteTextDocument(input, options, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }
}