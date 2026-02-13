using System;
using System.IO;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

/// <summary>
/// 图像保存和文件操作测试
/// </summary>
[TestFixture]
public class PdfImageSavingTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void SaveAsPng_ValidPath_CreatesFile()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        string outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

        try
        {
            // Act
            using var bitmap = page.Render();
            bitmap.SaveAsPng(outputPath);

            // Assert
            Assert.That(File.Exists(outputPath), Is.True);
            Assert.That(new FileInfo(outputPath).Length, Is.GreaterThan(0));
        }
        finally
        {
            // Cleanup
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    [Test]
    public void SaveAsPng_ValidStream_WritesData()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        using var bitmap = page.Render();
        using var stream = new MemoryStream();

        // Act
        bitmap.SaveAsPng(stream);

        // Assert
        Assert.That(stream.Length, Is.GreaterThan(0));
    }

    [Test]
    public void SaveAsJpeg_ValidPath_CreatesFile()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        string outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");

        try
        {
            // Act
            using var bitmap = page.Render();
            bitmap.SaveAsJpeg(outputPath);

            // Assert
            Assert.That(File.Exists(outputPath), Is.True);
            Assert.That(new FileInfo(outputPath).Length, Is.GreaterThan(0));
        }
        finally
        {
            // Cleanup
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    [Test]
    public void SaveAsJpeg_ValidStream_WritesData()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        using var bitmap = page.Render();
        using var stream = new MemoryStream();

        // Act
        bitmap.SaveAsJpeg(stream);

        // Assert
        Assert.That(stream.Length, Is.GreaterThan(0));
    }

    [Test]
    public void SaveAsJpeg_DifferentQuality_AffectsFileSize()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        using var bitmap = page.Render();

        string lowPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_low.jpg");
        string highPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_high.jpg");

        try
        {
            // Act
            bitmap.SaveAsJpeg(lowPath, 50);
            bitmap.SaveAsJpeg(highPath, 95);

            // Assert
            var lowInfo = new FileInfo(lowPath);
            var highInfo = new FileInfo(highPath);
            Assert.That(highInfo.Length, Is.GreaterThan(lowInfo.Length));
        }
        finally
        {
            // Cleanup
            if (File.Exists(lowPath))
                File.Delete(lowPath);
            if (File.Exists(highPath))
                File.Delete(highPath);
        }
    }

    [Test]
    public void MultipleSaves_SameBitmap_Succeeds()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        using var bitmap = page.Render();

        string path1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
        string path2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

        try
        {
            // Act
            bitmap.SaveAsPng(path1);
            bitmap.SaveAsPng(path2);

            // Assert
            Assert.That(File.Exists(path1), Is.True);
            Assert.That(File.Exists(path2), Is.True);

            // Verify files have same size
            var info1 = new FileInfo(path1);
            var info2 = new FileInfo(path2);
            Assert.That(info1.Length, Is.EqualTo(info2.Length));
        }
        finally
        {
            // Cleanup
            if (File.Exists(path1))
                File.Delete(path1);
            if (File.Exists(path2))
                File.Delete(path2);
        }
    }
}
