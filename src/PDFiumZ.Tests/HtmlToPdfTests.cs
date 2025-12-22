using PDFiumZ.HighLevel;
using Xunit;

namespace PDFiumZ.Tests;

public class HtmlToPdfTests : IDisposable
{
    private const string TestOutputDir = "test-output";

    public HtmlToPdfTests()
    {
        PdfiumLibrary.Initialize();
        Directory.CreateDirectory(TestOutputDir);
    }

    public void Dispose()
    {
        PdfiumLibrary.Shutdown();
    }

    [Fact]
    public void CreatePageFromHtml_BasicHtml_ShouldCreatePage()
    {
        using var document = PdfDocument.CreateNew();

        string html = "<h1>Test</h1><p>This is a test.</p>";
        using var page = document.CreatePageFromHtml(html);

        Assert.NotNull(page);
        Assert.Equal(1, document.PageCount);
    }

    [Fact]
    public void CreatePageFromHtml_WithStyles_ShouldApplyStyles()
    {
        using var document = PdfDocument.CreateNew();

        string html = @"
            <h1 style='color: #FF0000; text-align: center;'>Styled Heading</h1>
            <p style='font-size: 14pt;'>Paragraph with custom font size.</p>
        ";

        using var page = document.CreatePageFromHtml(html);

        Assert.NotNull(page);
        document.SaveToFile(Path.Combine(TestOutputDir, "html-with-styles.pdf"));
    }

    [Fact]
    public void CreatePageFromHtml_WithFormatting_ShouldWork()
    {
        using var document = PdfDocument.CreateNew();

        string html = @"
            <p>This is <b>bold</b>, <i>italic</i>, and <u>underlined</u> text.</p>
            <p>This combines <b><i>bold and italic</i></b>.</p>
        ";

        using var page = document.CreatePageFromHtml(html);

        Assert.NotNull(page);
    }

    [Fact]
    public void CreatePageFromHtml_WithAlignment_ShouldWork()
    {
        using var document = PdfDocument.CreateNew();

        string html = @"
            <p style='text-align: left;'>Left aligned</p>
            <p style='text-align: center;'>Center aligned</p>
            <p style='text-align: right;'>Right aligned</p>
        ";

        using var page = document.CreatePageFromHtml(html);

        Assert.NotNull(page);
    }

    [Fact]
    public void CreatePageFromHtml_WithCustomMargins_ShouldWork()
    {
        using var document = PdfDocument.CreateNew();

        string html = "<h1>Custom Margins</h1><p>This page has custom margins.</p>";

        using var page = document.CreatePageFromHtml(html, 30, 30, 40, 40);

        Assert.NotNull(page);
    }

    [Fact]
    public void CreatePageFromHtml_EmptyHtml_ShouldThrow()
    {
        using var document = PdfDocument.CreateNew();

        Assert.Throws<ArgumentException>(() => document.CreatePageFromHtml(""));
    }

    [Fact]
    public void CreatePageFromHtml_ComplexContent_ShouldCreateMultiplePages()
    {
        using var document = PdfDocument.CreateNew();

        string html = @"
            <h1 style='text-align: center; color: #2C3E50;'>HTML to PDF Test</h1>
            <h2 style='color: #E74C3C;'>Introduction</h2>
            <p>This is a test of the HTML to PDF converter with complex content.</p>

            <h2 style='color: #3498DB;'>Features</h2>
            <p><b>Headings:</b> h1, h2, h3, h4, h5, h6</p>
            <p><b>Text formatting:</b> bold, italic, underline</p>
            <p><b>Styles:</b> colors, font sizes, alignment</p>

            <h3 style='color: #27AE60;'>Conclusion</h3>
            <p style='text-align: center;'>The converter works great!</p>
        ";

        using var page = document.CreatePageFromHtml(html);

        Assert.NotNull(page);
        document.SaveToFile(Path.Combine(TestOutputDir, "html-complex.pdf"));
    }

    [Fact]
    public void CreatePageFromHtml_UnorderedList_ShouldWork()
    {
        using var document = PdfDocument.CreateNew();

        string html = @"
            <h1>Unordered List</h1>
            <ul>
                <li>Item 1</li>
                <li>Item 2</li>
                <li>Item 3</li>
            </ul>
        ";

        using var page = document.CreatePageFromHtml(html);

        Assert.NotNull(page);
        document.SaveToFile(Path.Combine(TestOutputDir, "html-ul.pdf"));
    }

    [Fact]
    public void CreatePageFromHtml_OrderedList_ShouldWork()
    {
        using var document = PdfDocument.CreateNew();

        string html = @"
            <h1>Ordered List</h1>
            <ol>
                <li>First step</li>
                <li>Second step</li>
                <li>Third step</li>
            </ol>
        ";

        using var page = document.CreatePageFromHtml(html);

        Assert.NotNull(page);
        document.SaveToFile(Path.Combine(TestOutputDir, "html-ol.pdf"));
    }

    [Fact]
    public void CreatePageFromHtml_NestedLists_ShouldWork()
    {
        using var document = PdfDocument.CreateNew();

        string html = @"
            <h1>Nested Lists</h1>
            <ul>
                <li>Main item 1</li>
                <li>Main item 2
                    <ul>
                        <li>Sub item 2.1</li>
                        <li>Sub item 2.2</li>
                    </ul>
                </li>
                <li>Main item 3</li>
            </ul>
        ";

        using var page = document.CreatePageFromHtml(html);

        Assert.NotNull(page);
        document.SaveToFile(Path.Combine(TestOutputDir, "html-nested-lists.pdf"));
    }

    [Fact]
    public void CreatePageFromHtml_MixedListTypes_ShouldWork()
    {
        using var document = PdfDocument.CreateNew();

        string html = @"
            <h1>Mixed Lists</h1>
            <ol>
                <li>Ordered item 1
                    <ul>
                        <li>Unordered sub-item</li>
                        <li>Another unordered sub-item</li>
                    </ul>
                </li>
                <li>Ordered item 2</li>
            </ol>
        ";

        using var page = document.CreatePageFromHtml(html);

        Assert.NotNull(page);
        document.SaveToFile(Path.Combine(TestOutputDir, "html-mixed-lists.pdf"));
    }

    [Fact]
    public void CreatePageFromHtml_ListsWithFormatting_ShouldWork()
    {
        using var document = PdfDocument.CreateNew();

        string html = @"
            <h1>Lists with Formatting</h1>
            <ul>
                <li><b>Bold item</b></li>
                <li><i>Italic item</i></li>
                <li><u>Underlined item</u></li>
                <li>Mixed <b><i>bold and italic</i></b></li>
            </ul>
        ";

        using var page = document.CreatePageFromHtml(html);

        Assert.NotNull(page);
        document.SaveToFile(Path.Combine(TestOutputDir, "html-lists-formatting.pdf"));
    }
}
