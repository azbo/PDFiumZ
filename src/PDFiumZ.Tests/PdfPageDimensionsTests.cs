using System;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

/// <summary>
/// 页面尺寸和属性测试
/// </summary>
[TestFixture]
public class PdfPageDimensionsTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void Width_IsGreaterThanZero()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        foreach (var page in document.Pages)
        {
            // Assert
            Assert.That(page.Width, Is.GreaterThan(0), $"Page {page.Index} width should be greater than 0");
        }
    }

    [Test]
    public void Height_IsGreaterThanZero()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        foreach (var page in document.Pages)
        {
            // Assert
            Assert.That(page.Height, Is.GreaterThan(0), $"Page {page.Index} height should be greater than 0");
        }
    }

    [Test]
    public void Width_HasReasonableUpperBound()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        foreach (var page in document.Pages)
        {
            // Assert - 页面宽度不应该超过 10000 点
            Assert.That(page.Width, Is.LessThan(10000), $"Page {page.Index} width is too large");
        }
    }

    [Test]
    public void Height_HasReasonableUpperBound()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        foreach (var page in document.Pages)
        {
            // Assert - 页面高度不应该超过 10000 点
            Assert.That(page.Height, Is.LessThan(10000), $"Page {page.Index} height is too large");
        }
    }

    [Test]
    public void Index_MatchesPageCollectionIndex()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act & Assert
        for (int i = 0; i < document.PageCount; i++)
        {
            var page = document.Pages[i];
            Assert.That(page.Index, Is.EqualTo(i), $"Page index should match collection index");
        }
    }

    [Test]
    public void Dimensions_AreConsistentAcrossAccesses()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        float width1 = page.Width;
        float width2 = page.Width;
        float width3 = page.Width;

        float height1 = page.Height;
        float height2 = page.Height;
        float height3 = page.Height;

        // Assert - 多次访问应该返回相同值
        Assert.That(width2, Is.EqualTo(width1));
        Assert.That(width3, Is.EqualTo(width1));
        Assert.That(height2, Is.EqualTo(height1));
        Assert.That(height3, Is.EqualTo(height1));
    }

    [Test]
    public void Dimensions_ArePositiveForAllPages()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act & Assert
        foreach (var page in document.Pages)
        {
            Assert.That(page.Width, Is.GreaterThan(0), $"Page {page.Index} width is not positive");
            Assert.That(page.Height, Is.GreaterThan(0), $"Page {page.Index} height is not positive");
        }
    }
}
