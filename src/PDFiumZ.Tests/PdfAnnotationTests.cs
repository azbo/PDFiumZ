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
    public void HighlightAnnotation_Create_ShouldAddHighlightToPage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        
        // Act
        var highlight = PdfHighlightAnnotation.Create(
            page,
            new PdfRectangle(100, 550, 200, 15),
            color: 0x80FFFF00);

        // Assert
        Assert.NotNull(highlight);
        Assert.Equal(PdfAnnotationType.Highlight, highlight.Type);
        
        highlight.Dispose();
    }

    [Fact]
    public void StampAnnotation_Create_ShouldAddStampToPage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        
        // Act
        var stamp = PdfStampAnnotation.Create(
            page,
            new PdfRectangle(400, 700, 150, 50),
            PdfStampType.Approved);

        // Assert
        Assert.NotNull(stamp);
        Assert.Equal(PdfAnnotationType.Stamp, stamp.Type);
        Assert.Equal(PdfStampType.Approved, stamp.StampType);
        
        stamp.Dispose();
    }

    [Fact]
    public void InkAnnotation_Create_ShouldAddInkToPage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        var path1 = new List<(double X, double Y)>
        {
            (100, 100),
            (150, 150),
            (200, 100)
        };

        var path2 = new List<(double X, double Y)>
        {
            (250, 250),
            (300, 300)
        };

        var paths = new List<List<(double X, double Y)>> { path1, path2 };

        // Act
        var ink = PdfInkAnnotation.Create(
            page,
            paths,
            color: 0xFF0000FF,
            width: 2.0);

        // Assert
        Assert.NotNull(ink);
        Assert.Equal(PdfAnnotationType.Ink, ink.Type);

        var retrievedPaths = ink.GetPaths();
        Assert.Equal(2, retrievedPaths.Count);
        Assert.Equal(3, retrievedPaths[0].Count);
        Assert.Equal(2, retrievedPaths[1].Count);

        // Verify coordinates (approximate check due to float conversion)
        Assert.Equal(100, retrievedPaths[0][0].X, 1);
        Assert.Equal(100, retrievedPaths[0][0].Y, 1);

        ink.Dispose();
    }

    [Fact]
    public void FreeTextAnnotation_Create_ShouldAddFreeTextToPage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        var rect = new PdfRectangle(100, 500, 200, 50);
        var text = "Hello FreeText";

        // Act
        var freeText = PdfFreeTextAnnotation.Create(page, rect, text);

        // Assert
        Assert.NotNull(freeText);
        Assert.Equal(PdfAnnotationType.FreeText, freeText.Type);
        Assert.Equal(text, freeText.Contents);
        
        // Verify bounds
        var bounds = freeText.Bounds;
        Assert.Equal(rect.X, bounds.X, 1);
        Assert.Equal(rect.Y, bounds.Y, 1);
        Assert.Equal(rect.Width, bounds.Width, 1);
        Assert.Equal(rect.Height, bounds.Height, 1);

        freeText.Dispose();
    }

    [Fact]
    public void GetAnnotation_ShouldReturnSpecializedTypes_ForInkAndFreeText()
    {
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        var paths = new List<List<(double X, double Y)>>
        {
            new()
            {
                (100, 100),
                (150, 150),
                (200, 100)
            }
        };

        var ink = PdfInkAnnotation.Create(page, paths, color: 0xFF0000FF, width: 2.0);
        var freeText = PdfFreeTextAnnotation.Create(page, new PdfRectangle(100, 500, 200, 50), "Hello");

        var annot0 = page.GetAnnotations(0).First();
        Assert.NotNull(annot0);
        Assert.IsType<PdfInkAnnotation>(annot0);

        var annot1 = page.GetAnnotations(1).First();
        Assert.NotNull(annot1);
        Assert.IsType<PdfFreeTextAnnotation>(annot1);

        annot0!.Dispose();
        annot1!.Dispose();
        ink.Dispose();
        freeText.Dispose();
    }

/*
    [Fact]
    public void PolygonAnnotation_Create_ShouldAddPolygonToPage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        
        var points = new List<(double X, double Y)>
        {
            (100, 100),
            (150, 150),
            (200, 100)
        };

        // Act
        var polygon = PdfPolygonAnnotation.Create(
            page,
            points,
            color: 0xFF0000FF,
            fillColor: 0x8000FF00,
            width: 2.0);

        // Assert
        Assert.NotNull(polygon);
        Assert.Equal(PdfAnnotationType.Polygon, polygon.Type);
        
        // Note: GetVertices might return empty if FPDFAnnotSetVertices is not called (since we only set AP)
        // So we might not be able to assert vertices count unless we find a way to set them.
        // For now, we check if creation succeeded.
        
        polygon.Dispose();
    }

    [Fact]
    public void PolyLineAnnotation_Create_ShouldAddPolyLineToPage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        
        var points = new List<(double X, double Y)>
        {
            (100, 100),
            (150, 150),
            (200, 100)
        };

        // Act
        var polyline = PdfPolyLineAnnotation.Create(
            page,
            points,
            color: 0xFF0000FF,
            width: 2.0);

        // Assert
        Assert.NotNull(polyline);
        Assert.Equal(PdfAnnotationType.PolyLine, polyline.Type);
        
        polyline.Dispose();
    }
*/

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
        Assert.Equal(2, page.AnnotationCount);

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
            document.Save(filePath);
        }

        // Act - Reopen and check
        using (var document = PdfDocument.Open(filePath))
        {
            using var page = document.GetPages(0).First();
            var count = page.AnnotationCount;

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

        Assert.Equal(2, page.AnnotationCount);

        // Act
        page.RemoveAnnotations(0);

        // Assert
        Assert.Equal(1, page.AnnotationCount);
    }
}

