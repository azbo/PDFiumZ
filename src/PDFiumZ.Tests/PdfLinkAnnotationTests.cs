using PDFiumZ.HighLevel;
using Xunit;

namespace PDFiumZ.Tests;

public class PdfLinkAnnotationTests : IDisposable
{
    private const string TestOutputDir = "test-output";

    public PdfLinkAnnotationTests()
    {
        // Initialize PDFium library
        PdfiumLibrary.Initialize();

        // Create test output directory
        Directory.CreateDirectory(TestOutputDir);
    }

    public void Dispose()
    {
        // Cleanup PDFium library
        PdfiumLibrary.Shutdown();
    }

    [Fact]
    public void CreateExternal_WithValidParameters_ShouldCreateLink()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842); // A4

        var bounds = new PdfRectangle(50, 700, 200, 30);
        var uri = "https://github.com/casbin-net/pdfiumz";

        // Act
        using var link = PdfLinkAnnotation.CreateExternal(page, bounds, uri);

        // Assert
        Assert.NotNull(link);
        Assert.Equal(bounds, link.Bounds);
        Assert.Equal(PdfAnnotationType.Link, link.Type);
    }

    [Fact]
    public void CreateExternal_WithCustomColor_ShouldSetColor()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        var bounds = new PdfRectangle(50, 700, 200, 30);
        var uri = "https://example.com";
        uint customColor = 0xFF0000FF; // Opaque red

        // Act
        using var link = PdfLinkAnnotation.CreateExternal(page, bounds, uri, customColor);

        // Assert
        Assert.NotNull(link);
        Assert.Equal(customColor, link.Color);
    }

    [Fact]
    public void CreateExternal_WithNullPage_ShouldThrowArgumentNullException()
    {
        // Arrange
        var bounds = new PdfRectangle(50, 700, 200, 30);
        var uri = "https://example.com";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PdfLinkAnnotation.CreateExternal(null!, bounds, uri));
    }

    [Fact]
    public void CreateExternal_WithNullUri_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        var bounds = new PdfRectangle(50, 700, 200, 30);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PdfLinkAnnotation.CreateExternal(page, bounds, null!));
    }

    [Fact]
    public void CreateExternal_WithDisposedPage_ShouldThrowObjectDisposedException()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        var page = document.CreatePage(595, 842);
        page.Dispose();

        var bounds = new PdfRectangle(50, 700, 200, 30);
        var uri = "https://example.com";

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() =>
            PdfLinkAnnotation.CreateExternal(page, bounds, uri));
    }

    [Fact]
    public void SetUri_WithValidUri_ShouldUpdateUri()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        var bounds = new PdfRectangle(50, 700, 200, 30);
        using var link = PdfLinkAnnotation.CreateExternal(page, bounds, "https://example.com");

        // Act
        link.Uri = "https://github.com";

        // Assert - No exception means success (Uri is write-only)
        Assert.NotNull(link);
    }

    [Fact]
    public void SetUri_WithNullUri_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);
        var bounds = new PdfRectangle(50, 700, 200, 30);
        using var link = PdfLinkAnnotation.CreateExternal(page, bounds, "https://example.com");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => link.Uri = null!);
    }

    [Fact]
    public void CreateExternal_SaveAndLoad_ShouldPersistLink()
    {
        // Arrange
        var filePath = Path.Combine(TestOutputDir, "link-annotation-test.pdf");
        var bounds = new PdfRectangle(50, 700, 200, 30);
        var uri = "https://github.com/casbin-net/pdfiumz";

        // Create document with link
        using (var document = PdfDocument.CreateNew())
        {
            using var page = document.CreatePage(595, 842);
            using var link = PdfLinkAnnotation.CreateExternal(page, bounds, uri);

            document.Save(filePath);
        }

        // Assert - Reload and verify link exists
        using (var loadedDoc = PdfDocument.Open(filePath))
        {
            Assert.Equal(1, loadedDoc.PageCount);

            using var loadedPage = loadedDoc.GetPage(0);
            var annotations = loadedPage.GetAnnotations().ToList();

            Assert.NotEmpty(annotations);
            Assert.Contains(annotations, a => a.Type == PdfAnnotationType.Link);
        }
    }

    [Fact]
    public void CreateExternal_MultipleLinks_ShouldCreateAllLinks()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        // Act - Create multiple links
        using var link1 = PdfLinkAnnotation.CreateExternal(
            page, new PdfRectangle(50, 700, 200, 30), "https://example1.com");
        using var link2 = PdfLinkAnnotation.CreateExternal(
            page, new PdfRectangle(50, 650, 200, 30), "https://example2.com");
        using var link3 = PdfLinkAnnotation.CreateExternal(
            page, new PdfRectangle(50, 600, 200, 30), "mailto:test@example.com");

        // Assert
        var annotations = page.GetAnnotations().ToList();
        Assert.Equal(3, annotations.Count);
        Assert.All(annotations, a => Assert.Equal(PdfAnnotationType.Link, a.Type));
    }

    [Fact]
    public void CreateExternal_WithEmailUri_ShouldCreateMailtoLink()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        var bounds = new PdfRectangle(50, 700, 200, 30);
        var uri = "mailto:support@example.com";

        // Act
        using var link = PdfLinkAnnotation.CreateExternal(page, bounds, uri);

        // Assert
        Assert.NotNull(link);
        Assert.Equal(PdfAnnotationType.Link, link.Type);
    }

    [Fact]
    public void CreateExternal_WithComplexUri_ShouldHandleParameters()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        var bounds = new PdfRectangle(50, 700, 200, 30);
        var uri = "https://example.com/search?q=test&lang=en#section1";

        // Act
        using var link = PdfLinkAnnotation.CreateExternal(page, bounds, uri);

        // Assert
        Assert.NotNull(link);
        Assert.Equal(PdfAnnotationType.Link, link.Type);
    }

    [Fact]
    public void CreateExternal_WithDefaultColor_ShouldUseTransparentBlue()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        var bounds = new PdfRectangle(50, 700, 200, 30);
        var uri = "https://example.com";

        // Act - Use default color
        using var link = PdfLinkAnnotation.CreateExternal(page, bounds, uri);

        // Assert - Default is 0x400000FF (transparent blue)
        Assert.Equal(0x400000FFu, link.Color);
    }
}
