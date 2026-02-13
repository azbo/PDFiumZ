using System;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

[TestFixture]
public class PdfMetadataTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void Metadata_ReturnsValidMetadata()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var metadata = document.Metadata;

        // Assert - 元数据对象不应为 null
        Assert.That(metadata, Is.Not.Null);
    }

    [Test]
    public void Metadata_CanBeAccessedMultipleTimes()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var metadata1 = document.Metadata;
        var metadata2 = document.Metadata;

        // Assert - 应该返回相同的缓存实例
        Assert.That(metadata1, Is.SameAs(metadata2));
    }
}
