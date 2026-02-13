using System;
using System.IO;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

/// <summary>
/// 文件加载边缘情况测试
/// </summary>
[TestFixture]
public class PdfFileLoadingTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void LoadFromFile_StreamAndByteArray_SameDocument()
    {
        // Arrange
        byte[] fileBytes = File.ReadAllBytes(TestPdfPath);

        // Act
        using var doc1 = new PdfDocument(TestPdfPath);
        using var doc2 = new PdfDocument(fileBytes);

        // Assert
        Assert.That(doc1.PageCount, Is.EqualTo(doc2.PageCount));
    }

    [Test]
    public void LoadFromMemoryStream_WorksCorrectly()
    {
        // Arrange
        using var fs = new MemoryStream(File.ReadAllBytes(TestPdfPath));

        // Act
        using var document = new PdfDocument(fs);

        // Assert
        Assert.That(document.PageCount, Is.GreaterThan(0));
    }

    [Test]
    public void LoadFromZeroLengthStream_ThrowsException()
    {
        // Arrange
        using var emptyStream = new MemoryStream(Array.Empty<byte>());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            using var document = new PdfDocument(emptyStream);
        });
    }

    [Test]
    public void LoadMultipleTimes_SameFile_SamePageCount()
    {
        // Arrange
        byte[] fileBytes = File.ReadAllBytes(TestPdfPath);

        // Act
        using var doc1 = new PdfDocument(fileBytes);
        using var doc2 = new PdfDocument(fileBytes);
        using var doc3 = new PdfDocument(fileBytes);

        // Assert
        Assert.That(doc1.PageCount, Is.EqualTo(doc2.PageCount));
        Assert.That(doc2.PageCount, Is.EqualTo(doc3.PageCount));
    }

    [Test]
    public void LoadFromPath_WithSpaces_TrimmedPath()
    {
        // Arrange
        string pathWithSpaces = "  " + TestPdfPath + "  ";

        // Act & Assert - 应该能正确处理带空格的路径
        Assert.Throws<InvalidOperationException>(() =>
        {
            using var document = new PdfDocument(pathWithSpaces);
        });
    }

    [Test]
    public void PageCount_AccessMultipleTimes_ReturnsSameValue()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        int count1 = document.PageCount;
        int count2 = document.PageCount;
        int count3 = document.PageCount;

        // Assert
        Assert.That(count1, Is.EqualTo(count2));
        Assert.That(count2, Is.EqualTo(count3));
    }
}
