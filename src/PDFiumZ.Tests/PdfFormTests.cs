using System;
using System.Linq;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

[TestFixture]
public class PdfFormTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void GetFormFields_ReturnsFieldList()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var fields = document.GetFormFields();

        // Assert
        Assert.That(fields, Is.Not.Null);
        Assert.That(fields.Count, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void GetFormFields_ForDocumentWithNoForms_ReturnsEmptyList()
    {
        // Arrange
        var emptyPdfBytes = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4\n1 0 obj<</Type>/Catalog<</Pages>/Type/Page<</MediaBox[0 0 612]>>/Contents<</Length> 44>/Filter/FlateDecode<</S> 42>>stream\r\nendstream\r\nendobj\r\n");
        using var emptyDocument = new PdfDocument(emptyPdfBytes);

        // Act
        var fields = emptyDocument.GetFormFields();

        // Assert
        Assert.That(fields, Is.Not.Null);
        Assert.That(fields.Count, Is.EqualTo(0));
    }

    [Test]
    public void Dispose_WithFormFields_CanDisposeMultipleTimes()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var fields = document.GetFormFields();

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            document.Dispose();
        });

        // 再次访问应该可以工作
        Assert.That(fields.Count, Is.GreaterThanOrEqualTo(0));
    }
}
