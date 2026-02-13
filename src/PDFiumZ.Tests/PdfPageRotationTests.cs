using System;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

[TestFixture]
public class PdfPageRotationTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void Rotation_DefaultValue_IsRotate0()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        var rotation = page.Rotation;

        // Assert - 默认应该是 Rotate0
        Assert.That(rotation, Is.EqualTo(PdfPageRotation.Rotate0));
    }

    [Test]
    public void Rotation_CanBeSetToRotate90()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        page.Rotation = PdfPageRotation.Rotate90;
        var rotation = page.Rotation;

        // Assert
        Assert.That(rotation, Is.EqualTo(PdfPageRotation.Rotate90));
    }

    [Test]
    public void Rotation_CanBeSetToRotate180()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        page.Rotation = PdfPageRotation.Rotate180;
        var rotation = page.Rotation;

        // Assert
        Assert.That(rotation, Is.EqualTo(PdfPageRotation.Rotate180));
    }

    [Test]
    public void Rotation_CanBeSetToRotate270()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act
        page.Rotation = PdfPageRotation.Rotate270;
        var rotation = page.Rotation;

        // Assert
        Assert.That(rotation, Is.EqualTo(PdfPageRotation.Rotate270));
    }

    [Test]
    public void Rotation_MultipleChanges_WorksCorrectly()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // Act & Assert
        page.Rotation = PdfPageRotation.Rotate90;
        Assert.That(page.Rotation, Is.EqualTo(PdfPageRotation.Rotate90));

        page.Rotation = PdfPageRotation.Rotate180;
        Assert.That(page.Rotation, Is.EqualTo(PdfPageRotation.Rotate180));

        page.Rotation = PdfPageRotation.Rotate0;
        Assert.That(page.Rotation, Is.EqualTo(PdfPageRotation.Rotate0));
    }
}
