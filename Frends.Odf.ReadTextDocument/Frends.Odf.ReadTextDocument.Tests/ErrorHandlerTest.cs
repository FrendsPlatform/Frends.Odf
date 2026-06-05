using System;
using System.Threading;
using NUnit.Framework;

namespace Frends.Odf.ReadTextDocument.Tests;

[TestFixture]
internal class ErrorHandlerTest : TestBase
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    // Fake path to trigger errors
    private readonly string fakePath = @"C:\temp\fake_path.odt";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var input = DefaultInput();
        input.FilePath = fakePath;

        var ex = Assert.Throws<Exception>(() =>
           Odf.ReadTextDocument(input, DefaultOptions(), CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var input = DefaultInput();
        input.FilePath = fakePath;

        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = Odf.ReadTextDocument(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var input = DefaultInput();
        input.FilePath = fakePath;

        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;

        var ex = Assert.Throws<Exception>(() =>
            Odf.ReadTextDocument(input, options, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }
}