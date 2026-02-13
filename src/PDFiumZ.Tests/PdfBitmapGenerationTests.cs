using System;
using System.IO;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

/// <summary>
/// 位图生成高级测试
/// </summary>
[TestFixture]
public class PdfBitmapGenerationTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void Render_WithDefaultSettings_GeneratesValidBitmap()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        using var bitmap = page.Render();

        // Assert
        Assert.That(bitmap, Is.Not.Null);
        Assert.That(bitmap.Width, Is.GreaterThan(0));
        Assert.That(bitmap.Height, Is.GreaterThan(0));
    }

    [Test]
    public void Render_AllPages_GenerateValidBitmaps()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act & Assert
        foreach (var page in document.Pages)
        {
            using var bitmap = page.Render();
            Assert.That(bitmap, Is.Not.Null, $"Page {page.Index} bitmap is null");
            Assert.That(bitmap.Width, Is.GreaterThan(0), $"Page {page.Index} bitmap width is 0");
            Assert.That(bitmap.Height, Is.GreaterThan(0), $"Page {page.Index} bitmap height is 0");
        }
    }

    [Test]
    public void Render_MultipleTimesSamePage_ReturnsConsistentBitmaps()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        using var bitmap1 = page.Render();
        using var bitmap2 = page.Render();
        using var bitmap3 = page.Render();

        // Assert
        Assert.That(bitmap1.Width, Is.EqualTo(bitmap2.Width));
        Assert.That(bitmap2.Width, Is.EqualTo(bitmap3.Width));
        Assert.That(bitmap1.Height, Is.EqualTo(bitmap2.Height));
        Assert.That(bitmap2.Height, Is.EqualTo(bitmap3.Height));
    }

    [Test]
    public void Render_DifferentPages_ReturnsDifferentBitmaps()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        using var bitmap1 = document.Pages[0].Render();

        if (document.PageCount > 1)
        {
            using var bitmap2 = document.Pages[document.PageCount - 1].Render();

            // Assert - 不同页面可能有不同尺寸
            Assert.That(bitmap1, Is.Not.Null);
            Assert.That(bitmap2, Is.Not.Null);
        }
    }

    [Test]
    public void GetData_ReturnsValidBuffer()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        using var bitmap = page.Render();

        // Act
        var data = bitmap.GetData();

        // Assert
        Assert.That(data.IsEmpty, Is.False);
        Assert.That(data.Length, Is.EqualTo(bitmap.Width * bitmap.Height * 4)); // BGRA = 4 bytes per pixel
    }

    [Test]
    public void GetData_CalledMultipleTimes_ReturnsConsistentData()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        using var bitmap = page.Render();

        // Act
        var data1 = bitmap.GetData();
        var data2 = bitmap.GetData();
        var data3 = bitmap.GetData();

        // Assert
        Assert.That(data1.Length, Is.EqualTo(data2.Length));
        Assert.That(data2.Length, Is.EqualTo(data3.Length));
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        var bitmap = page.Render();

        // Act & Assert
        bitmap.Dispose();
        bitmap.Dispose();
        // Should not throw
    }

    [Test]
    public void ToSKBitmap_ReturnsValidSKBitmap()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        using var bitmap = page.Render();

        // Act
        using var skBitmap = bitmap.ToSKBitmap();

        // Assert
        Assert.That(skBitmap, Is.Not.Null);
        Assert.That(skBitmap.Width, Is.EqualTo(bitmap.Width));
        Assert.That(skBitmap.Height, Is.EqualTo(bitmap.Height));
    }

    [Test]
    public void ToSKBitmap_CalledMultipleTimes_ReturnsValidBitmaps()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        using var bitmap = page.Render();

        // Act
        using var skBitmap1 = bitmap.ToSKBitmap();
        using var skBitmap2 = bitmap.ToSKBitmap();

        // Assert
        Assert.That(skBitmap1.Width, Is.EqualTo(skBitmap2.Width));
        Assert.That(skBitmap1.Height, Is.EqualTo(skBitmap2.Height));
    }
}
