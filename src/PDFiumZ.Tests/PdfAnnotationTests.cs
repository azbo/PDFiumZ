using System;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

[TestFixture]
public class PdfAnnotationTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void GetAnnotationCount_ReturnsCount()
    {
        using var document = new PdfDocument(TestPdfPath);

        // Act
        int count = document.GetAnnotationCount(0);

        // Assert
        Assert.That(count, Is.GreaterThanOrEqualTo(0), "Annotation count should be non-negative");
    }

    [Test]
    public void CreateAnnotation_ShouldCreateNewAnnotation()
    {
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var annot = document.CreateAnnotation(0, PdfAnnotationSubtype.Text);

        // Assert
        Assert.That(annot, Is.Not.Null, "Annotation should be created");
    }
}
