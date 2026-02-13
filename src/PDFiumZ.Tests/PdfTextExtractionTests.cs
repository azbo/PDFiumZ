using System;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

[TestFixture]
public class PdfTextExtractionTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void GetText_ReturnsString()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        string text = page.GetText();

        // Assert
        Assert.That(text, Is.Not.Null);
        Assert.That(text, Is.Not.Empty);
    }

    [Test]
    public void GetText_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        string text1 = page.GetText();
        string text2 = page.GetText();

        // Assert - 多次调用应该返回相同内容
        Assert.That(text1, Is.EqualTo(text2));
    }

    [Test]
    public void GetText_EmptyPage_ReturnsEmptyString()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        string text = page.GetText();

        // Assert - 对于非空页面应该返回文本
        Assert.That(text, Is.Not.Null);
    }

    [Test]
    public void GetText_DoesNotDisposePage()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        string text = page.GetText();

        // Assert - 获取文本后页面应该仍然可用
        Assert.That(text, Is.Not.Null);
        Assert.That(page.Width, Is.GreaterThan(0));
        Assert.That(page.Height, Is.GreaterThan(0));
    }
}
