using PDFiumZ.HighLevel;
using Xunit;

namespace PDFiumZ.Tests;

public class PdfAnnotationTests : IDisposable
{
    private const string TestOutputDir = "test-output";

    public PdfAnnotationTests()
    {
        PdfiumLibrary.Initialize();
        Directory.CreateDirectory(TestOutputDir);
    }

    public void Dispose()
    {
        PdfiumLibrary.Shutdown();
    }

    [Fact]
    public void SquareAnnotation_Create_ShouldAddRectangleToPage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        // Act
        var square = PdfSquareAnnotation.Create(
            page,
            new PdfRectangle(100, 700, 150, 100),
            borderColor: 0xFFFF0000,
            fillColor: 0x400000FF,
            borderWidth: 2.0);

        // Assert
        Assert.NotNull(square);
        Assert.Equal(PdfAnnotationType.Square, square.Type);
        square.Dispose();
    }

    [Fact]
    public void CircleAnnotation_Create_ShouldAddCircleToPage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        // Act
        var circle = PdfCircleAnnotation.Create(
            page,
            new PdfRectangle(300, 550, 100, 100),
            borderColor: 0xFF00FF00,
            fillColor: 0x4000FF00,
            borderWidth: 2.0);

        // Assert
        Assert.NotNull(circle);
        Assert.Equal(PdfAnnotationType.Circle, circle.Type);
        circle.Dispose();
    }

    [Fact]
    public void UnderlineAnnotation_Create_ShouldAddUnderlineToPage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        // Act
        var underline = PdfUnderlineAnnotation.Create(
            page,
            new PdfRectangle(100, 650, 200, 15),
            color: 0xFFFF0000);

        // Assert
        Assert.NotNull(underline);
        Assert.Equal(PdfAnnotationType.Underline, underline.Type);
        underline.Dispose();
    }

    [Fact]
    public void StrikeOutAnnotation_Create_ShouldAddStrikeoutToPage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        // Act
        var strikeout = PdfStrikeOutAnnotation.Create(
            page,
            new PdfRectangle(100, 600, 200, 15),
            color: 0xFF0000FF);

        // Assert
        Assert.NotNull(strikeout);
        Assert.Equal(PdfAnnotationType.StrikeOut, strikeout.Type);
        strikeout.Dispose();
    }

    [Fact]
    public void GetAnnotationCount_WithAnnotations_ShouldReturnCorrectCount()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        // Act
        var square = PdfSquareAnnotation.Create(page, new PdfRectangle(100, 100, 50, 50));
        var circle = PdfCircleAnnotation.Create(page, new PdfRectangle(200, 200, 50, 50));

        // Assert
        Assert.Equal(2, page.GetAnnotationCount());

        square.Dispose();
        circle.Dispose();
    }

    [Fact]
    public void GetAnnotations_ShouldReturnAllAnnotations()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        var square = PdfSquareAnnotation.Create(page, new PdfRectangle(100, 100, 50, 50));
        var circle = PdfCircleAnnotation.Create(page, new PdfRectangle(200, 200, 50, 50));
        var underline = PdfUnderlineAnnotation.Create(page, new PdfRectangle(100, 650, 200, 15));

        // Act
        var annotations = page.GetAnnotations().ToList();

        // Assert
        Assert.Equal(3, annotations.Count);
        Assert.Contains(annotations, a => a.Type == PdfAnnotationType.Square);
        Assert.Contains(annotations, a => a.Type == PdfAnnotationType.Circle);
        Assert.Contains(annotations, a => a.Type == PdfAnnotationType.Underline);

        // Cleanup
        square.Dispose();
        circle.Dispose();
        underline.Dispose();
        foreach (var annot in annotations)
        {
            annot.Dispose();
        }
    }

    [Fact]
    public void GetAnnotations_Generic_ShouldReturnSpecificType()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        var square1 = PdfSquareAnnotation.Create(page, new PdfRectangle(100, 100, 50, 50));
        var square2 = PdfSquareAnnotation.Create(page, new PdfRectangle(200, 200, 50, 50));
        var circle = PdfCircleAnnotation.Create(page, new PdfRectangle(300, 300, 50, 50));

        // Act
        var squareAnnotations = page.GetAnnotations<PdfSquareAnnotation>().ToList();
        var circleAnnotations = page.GetAnnotations<PdfCircleAnnotation>().ToList();

        // Assert
        Assert.Equal(2, squareAnnotations.Count);
        Assert.Single(circleAnnotations);

        // Cleanup
        square1.Dispose();
        square2.Dispose();
        circle.Dispose();
        foreach (var annot in squareAnnotations)
        {
            annot.Dispose();
        }
        foreach (var annot in circleAnnotations)
        {
            annot.Dispose();
        }
    }

    [Fact]
    public void UnderlineAnnotation_QuadPoints_ShouldSetAndGetCorrectly()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        var underline = PdfUnderlineAnnotation.Create(
            page,
            new PdfRectangle(100, 650, 200, 15));

        var rectangles = new[]
        {
            new PdfRectangle(100, 650, 200, 15),
            new PdfRectangle(100, 635, 150, 15)
        };

        // Act
        underline.SetQuadPoints(rectangles);
        var quadPoints = underline.GetQuadPoints();

        // Assert
        Assert.Equal(2, quadPoints.Length);

        // Cleanup
        underline.Dispose();
    }

    [Fact]
    public void Annotations_ShouldPersistAfterSave()
    {
        // Arrange
        var filePath = Path.Combine(TestOutputDir, "test-annotations-persist.pdf");

        // Create document with annotations
        using (var document = PdfDocument.CreateNew())
        {
            using var page = document.CreatePage(595, 842);
            var square = PdfSquareAnnotation.Create(page, new PdfRectangle(100, 100, 50, 50));
            var circle = PdfCircleAnnotation.Create(page, new PdfRectangle(200, 200, 50, 50));
            square.Dispose();
            circle.Dispose();
            document.SaveToFile(filePath);
        }

        // Act - Reopen and check
        using (var document = PdfDocument.Open(filePath))
        {
            using var page = document.GetPage(0);
            var count = page.GetAnnotationCount();

            // Assert
            Assert.Equal(2, count);
        }
    }

    [Fact]
    public void RemoveAnnotation_ShouldRemoveAnnotation()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        var square = PdfSquareAnnotation.Create(page, new PdfRectangle(100, 100, 50, 50));
        var circle = PdfCircleAnnotation.Create(page, new PdfRectangle(200, 200, 50, 50));
        square.Dispose();
        circle.Dispose();

        Assert.Equal(2, page.GetAnnotationCount());

        // Act
        page.RemoveAnnotation(0);

        // Assert
        Assert.Equal(1, page.GetAnnotationCount());
    }
}
