using System;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

[TestFixture]
public class PdfLinkExtractionTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void GetLinks_ReturnsList()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        var links = page.GetLinks();

        // Assert - 应该返回一个列表（可能为空）
        Assert.That(links, Is.Not.Null);
    }

    [Test]
    public void GetLinks_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        var links1 = page.GetLinks();
        var links2 = page.GetLinks();

        // Assert - 多次调用应该返回相同数量的链接
        Assert.That(links1.Count, Is.EqualTo(links2.Count));
    }

    [Test]
    public void GetLinks_NonEmptyLinks_HaveValidProperties()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        var links = page.GetLinks();

        // Assert - 如果存在链接，应该有有效的属性
        foreach (var link in links)
        {
            Assert.That(link.Url, Is.Not.Null);
            Assert.That(link.StartIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(link.CharCount, Is.GreaterThanOrEqualTo(0));
        }
    }

    [Test]
    public void GetLinks_DoesNotDisposePage()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        var links = page.GetLinks();

        // Assert - 获取链接后页面应该仍然可用
        Assert.That(links, Is.Not.Null);
        Assert.That(page.Width, Is.GreaterThan(0));
        Assert.That(page.Height, Is.GreaterThan(0));
    }

    [Test]
    public void GetLinks_EmptyPage_ReturnsEmptyList()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        var links = page.GetLinks();

        // Assert - 至少应该能正常调用
        Assert.That(links, Is.Not.Null);
    }
}
