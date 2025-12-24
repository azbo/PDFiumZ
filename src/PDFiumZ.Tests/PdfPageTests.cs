using PDFiumZ.HighLevel;
using Xunit;

namespace PDFiumZ.Tests;

public class PdfPageTests : IDisposable
{
    private const string TestOutputDir = "test-output";

    public PdfPageTests()
    {
        PdfiumLibrary.Initialize();
        Directory.CreateDirectory(TestOutputDir);
    }

    public void Dispose()
    {
        PdfiumLibrary.Shutdown();
    }

    [Fact]
    public void Page_Rotation_ShouldDefaultToNone()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        // Assert
        Assert.Equal(PdfRotation.None, page.Rotation);
    }

    [Fact]
    public void Page_SetRotation_ShouldUpdateRotation()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        // Act
        page.Rotation = PdfRotation.Rotate90;

        // Assert
        Assert.Equal(PdfRotation.Rotate90, page.Rotation);
    }

    [Fact]
    public void Page_Rotate_ShouldSetRotation()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595, 842);

        // Act
        page.Rotate(PdfRotation.Rotate180);

        // Assert
        Assert.Equal(PdfRotation.Rotate180, page.Rotation);
    }

    [Fact]
    public void RotatePages_ShouldRotateAllPages()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();
        document.CreatePage(595, 842).Dispose();
        document.CreatePage(595, 842).Dispose();

        // Act
        document.RotatePages(PdfRotation.Rotate90);

        // Assert
        using var page0 = document.GetPages(0).First();
        using var page1 = document.GetPages(1).First();
        using var page2 = document.GetPages(2).First();
        Assert.Equal(PdfRotation.Rotate90, page0.Rotation);
        Assert.Equal(PdfRotation.Rotate90, page1.Rotation);
        Assert.Equal(PdfRotation.Rotate90, page2.Rotation);
    }

    [Fact]
    public void RotatePages_WithSpecificIndices_ShouldRotateOnlyThosePages()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();
        document.CreatePage(595, 842).Dispose();
        document.CreatePage(595, 842).Dispose();
        document.CreatePage(595, 842).Dispose();

        // Act - Rotate only pages 0, 2
        document.RotatePages(PdfRotation.Rotate180, 0, 2);

        // Assert
        using var page0 = document.GetPages(0).First();
        using var page1 = document.GetPages(1).First();
        using var page2 = document.GetPages(2).First();
        using var page3 = document.GetPages(3).First();
        Assert.Equal(PdfRotation.Rotate180, page0.Rotation);
        Assert.Equal(PdfRotation.None, page1.Rotation);
        Assert.Equal(PdfRotation.Rotate180, page2.Rotation);
        Assert.Equal(PdfRotation.None, page3.Rotation);
    }

    [Fact]
    public void RotatePages_WithInvalidIndex_ShouldThrowException()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();
        document.CreatePage(595, 842).Dispose();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            document.RotatePages(PdfRotation.Rotate90, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            document.RotatePages(PdfRotation.Rotate90, 5));
    }

    [Fact]
    public void RotatePages_ShouldPersistAfterSave()
    {
        // Arrange
        var filePath = Path.Combine(TestOutputDir, "test-rotation-persist.pdf");

        // Create and rotate document
        using (var document = PdfDocument.CreateNew())
        {
            document.CreatePage(595, 842).Dispose();
            document.CreatePage(595, 842).Dispose();
            document.RotatePages(PdfRotation.Rotate270, 0);
            document.Save(filePath);
        }

        // Act - Reopen and check
        using (var document = PdfDocument.Open(filePath))
        {
            using var page0 = document.GetPages(0).First();
            using var page1 = document.GetPages(1).First();

            // Assert
            Assert.Equal(PdfRotation.Rotate270, page0.Rotation);
            Assert.Equal(PdfRotation.None, page1.Rotation);
        }
    }

    [Fact]
    public void Page_WidthAndHeight_ShouldReturnCorrectDimensions()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(595.0, 842.0);

        // Assert
        Assert.Equal(595.0, page.Width, 0.1);
        Assert.Equal(842.0, page.Height, 0.1);
    }

    [Fact]
    public void Page_Size_ShouldReturnWidthAndHeight()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(612.0, 792.0);

        // Act
        var size = page.Size;

        // Assert
        Assert.Equal(612.0, size.Width, 0.1);
        Assert.Equal(792.0, size.Height, 0.1);
    }

    [Fact]
    public void GenerateThumbnail_ShouldCreateSmallerImage()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(600, 800);

        // Act
        using var thumbnail = page.GenerateThumbnail(maxWidth: 200);

        // Assert
        Assert.NotNull(thumbnail);
        Assert.True(thumbnail.Width <= 200);
        Assert.True(thumbnail.Height > 0);
        // Aspect ratio should be maintained: 600:800 = 3:4
        var expectedHeight = 200 * (800.0 / 600.0);
        Assert.Equal(expectedHeight, (double)thumbnail.Height, 2.0);  // Tolerance of 2 pixels
    }

    [Fact]
    public void GenerateThumbnail_WithDifferentQuality_ShouldWork()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(600, 800);

        // Act - Test all quality levels
        using var lowQuality = page.GenerateThumbnail(maxWidth: 150, quality: 0);
        using var mediumQuality = page.GenerateThumbnail(maxWidth: 150, quality: 1);
        using var highQuality = page.GenerateThumbnail(maxWidth: 150, quality: 2);

        // Assert
        Assert.NotNull(lowQuality);
        Assert.NotNull(mediumQuality);
        Assert.NotNull(highQuality);
        Assert.True(lowQuality.Width <= 150);
        Assert.True(mediumQuality.Width <= 150);
        Assert.True(highQuality.Width <= 150);
    }

    [Fact]
    public void GenerateThumbnail_InvalidMaxWidth_ShouldThrow()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(600, 800);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => page.GenerateThumbnail(maxWidth: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => page.GenerateThumbnail(maxWidth: -100));
    }

    [Fact]
    public void GenerateThumbnail_InvalidQuality_ShouldThrow()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(600, 800);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => page.GenerateThumbnail(quality: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => page.GenerateThumbnail(quality: 3));
    }

    [Fact]
    public async Task GenerateThumbnailAsync_ShouldCreateThumbnail()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        using var page = document.CreatePage(600, 800);

        // Act
        using var thumbnail = await page.GenerateThumbnailAsync(maxWidth: 200);

        // Assert
        Assert.NotNull(thumbnail);
        Assert.True(thumbnail.Width <= 200);
    }
}

