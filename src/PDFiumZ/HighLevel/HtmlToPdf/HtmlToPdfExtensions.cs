using System;
using PDFiumZ.HighLevel.HtmlToPdf;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Extension methods for HTML to PDF conversion.
/// </summary>
public static class HtmlToPdfExtensions
{
    /// <summary>
    /// Creates a new page from HTML content.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="html">The HTML content to convert.</param>
    /// <param name="pageWidth">Page width in points (default: A4 width = 595).</param>
    /// <param name="pageHeight">Page height in points (default: A4 height = 842).</param>
    /// <returns>The created PDF page.</returns>
    public static PdfPage CreatePageFromHtml(this PdfDocument document, string html, double pageWidth = 595, double pageHeight = 842)
    {
        using var converter = new HtmlToPdfConverter(document);
        return converter.ConvertToPdf(html, pageWidth, pageHeight);
    }

    /// <summary>
    /// Creates a new page from HTML content with custom margins.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="html">The HTML content to convert.</param>
    /// <param name="marginLeft">Left margin in points.</param>
    /// <param name="marginRight">Right margin in points.</param>
    /// <param name="marginTop">Top margin in points.</param>
    /// <param name="marginBottom">Bottom margin in points.</param>
    /// <param name="pageWidth">Page width in points (default: A4 width = 595).</param>
    /// <param name="pageHeight">Page height in points (default: A4 height = 842).</param>
    /// <returns>The created PDF page.</returns>
    public static PdfPage CreatePageFromHtml(this PdfDocument document, string html,
        double marginLeft, double marginRight, double marginTop, double marginBottom,
        double pageWidth = 595, double pageHeight = 842)
    {
        using var converter = new HtmlToPdfConverter(document)
        {
            MarginLeft = marginLeft,
            MarginRight = marginRight,
            MarginTop = marginTop,
            MarginBottom = marginBottom
        };
        return converter.ConvertToPdf(html, pageWidth, pageHeight);
    }
}
