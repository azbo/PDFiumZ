using PDFiumZ.HighLevel;
using Xunit;

namespace PDFiumZ.Tests;

public class PdfDocumentTests : IDisposable
{
    private const string TestOutputDir = "test-output";

    public PdfDocumentTests()
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
    public void CreateNew_ShouldCreateEmptyDocument()
    {
        // Act
        using var document = PdfDocument.CreateNew();

        // Assert
        Assert.NotNull(document);
        Assert.Equal(0, document.PageCount);
    }

    [Fact]
    public void CreatePage_ShouldAddPageToDocument()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();

        // Act
        using var page = document.CreatePage(595, 842); // A4

        // Assert
        Assert.NotNull(page);
        Assert.Equal(1, document.PageCount);
        Assert.Equal(595, page.Width, 0.1);
        Assert.Equal(842, page.Height, 0.1);
    }

    [Fact]
    public void CreatePage_Default_ShouldCreateA4()
    {
        using var document = PdfDocument.CreateNew();

        using var page = document.CreatePage();

        Assert.NotNull(page);
        Assert.Equal(1, document.PageCount);
        Assert.Equal(595, page.Width, 0.1);
        Assert.Equal(842, page.Height, 0.1);
    }

    [Fact]
    public void CreatePage_StandardSize_ShouldWork()
    {
        using var document = PdfDocument.CreateNew();

        using var page1 = document.CreatePage(PdfPageSize.A3);
        using var page2 = document.CreatePage(PdfPageSize.A4);
        using var page3 = document.CreatePage(PdfPageSize.Letter);
        using var page4 = document.CreatePage(PdfPageSize.Tabloid);

        Assert.Equal(4, document.PageCount);
        Assert.Equal(842, page1.Width, 0.1);
        Assert.Equal(1191, page1.Height, 0.1);
        Assert.Equal(595, page2.Width, 0.1);
        Assert.Equal(842, page2.Height, 0.1);
        Assert.Equal(612, page3.Width, 0.1);
        Assert.Equal(792, page3.Height, 0.1);
        Assert.Equal(792, page4.Width, 0.1);
        Assert.Equal(1224, page4.Height, 0.1);
    }

    [Fact]
    public void CreatePage_MultipleSizes_ShouldWork()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();

        // Act
        using var page1 = document.CreatePage(595, 842);  // A4
        using var page2 = document.CreatePage(612, 792);  // Letter
        using var page3 = document.CreatePage(800, 600);  // Custom

        // Assert
        Assert.Equal(3, document.PageCount);
    }

    [Fact]
    public void CreatePage_WithInvalidDimensions_ShouldThrowException()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => document.CreatePage(0, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => document.CreatePage(100, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => document.CreatePage(-1, 100));
    }

    [Fact]
    public void SaveToFile_ShouldCreateValidPDF()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();
        var filePath = Path.Combine(TestOutputDir, "test-save.pdf");

        // Act
        document.SaveToFile(filePath);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.True(new FileInfo(filePath).Length > 0);
    }

    [Fact]
    public void Merge_ShouldCombineMultiplePDFs()
    {
        // Arrange
        var file1 = Path.Combine(TestOutputDir, "merge-test1.pdf");
        var file2 = Path.Combine(TestOutputDir, "merge-test2.pdf");
        var file3 = Path.Combine(TestOutputDir, "merge-test3.pdf");

        using (var doc1 = PdfDocument.CreateNew())
        {
            doc1.CreatePage(595, 842).Dispose();
            doc1.SaveToFile(file1);
        }

        using (var doc2 = PdfDocument.CreateNew())
        {
            doc2.CreatePage(595, 842).Dispose();
            doc2.CreatePage(595, 842).Dispose();
            doc2.SaveToFile(file2);
        }

        using (var doc3 = PdfDocument.CreateNew())
        {
            doc3.CreatePage(595, 842).Dispose();
            doc3.CreatePage(595, 842).Dispose();
            doc3.CreatePage(595, 842).Dispose();
            doc3.SaveToFile(file3);
        }

        // Act
        using var merged = PdfDocument.Merge(file1, file2, file3);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(6, merged.PageCount); // 1 + 2 + 3 = 6
    }

    [Fact]
    public void Merge_WithNoFiles_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PdfDocument.Merge());
    }

    [Fact]
    public void Split_ShouldExtractPageRange()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        for (int i = 0; i < 6; i++)
        {
            document.CreatePage(595, 842).Dispose();
        }

        // Act
        using var split = document.Split(2, 3); // Extract pages 2, 3, 4

        // Assert
        Assert.NotNull(split);
        Assert.Equal(3, split.PageCount);
        Assert.Equal(6, document.PageCount); // Original unchanged
    }

    [Fact]
    public void Split_WithInvalidRange_ShouldThrowException()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();
        document.CreatePage(595, 842).Dispose();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => document.Split(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => document.Split(0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => document.Split(0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => document.Split(5, 1));
    }

    [Fact]
    public void AddTextWatermark_ShouldAddWatermarkToAllPages()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();
        document.CreatePage(595, 842).Dispose();

        var options = new WatermarkOptions
        {
            Opacity = 0.3,
            Rotation = 45,
            FontSize = 48
        };

        // Act
        document.AddTextWatermark("TEST", WatermarkPosition.Center, options);

        // Save to verify
        var filePath = Path.Combine(TestOutputDir, "test-watermark.pdf");
        document.SaveToFile(filePath);

        // Assert
        Assert.True(File.Exists(filePath));
        // Verify file is larger than empty document (watermark adds content)
        Assert.True(new FileInfo(filePath).Length > 1000);
    }

    [Fact]
    public void AddHeaderFooter_ShouldAddTextToAllPages()
    {
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();
        document.CreatePage(595, 842).Dispose();

        document.AddHeaderFooter("Header {page}/{pages}", "Footer {page}");

        using var page1 = document.GetPage(0);
        using var page2 = document.GetPage(1);

        var text1 = page1.ExtractText();
        var text2 = page2.ExtractText();

        Assert.Contains("Header 1/2", text1);
        Assert.Contains("Footer 1", text1);

        Assert.Contains("Header 2/2", text2);
        Assert.Contains("Footer 2", text2);
    }
}
