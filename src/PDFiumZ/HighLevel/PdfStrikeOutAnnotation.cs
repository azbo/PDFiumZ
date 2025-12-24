using System;
using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a strikeout annotation in a PDF document.
/// Strikeout annotations mark selected text with a line through it.
/// </summary>
public sealed class PdfStrikeOutAnnotation : PdfTextMarkupAnnotation
{
    /// <summary>
    /// Creates a new strikeout annotation on the specified page.
    /// </summary>
    /// <param name="page">The page to add the annotation to.</param>
    /// <param name="bounds">The bounding rectangle of the strikeout.</param>
    /// <param name="color">The strikeout color in ARGB format (default: red with full opacity).</param>
    /// <returns>A new <see cref="PdfStrikeOutAnnotation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">page is null.</exception>
    /// <exception cref="PdfException">Failed to create annotation.</exception>
    public static PdfStrikeOutAnnotation Create(PdfPage page, PdfRectangle bounds, uint color = 0xFFFF0000)
    {
        return CreateMarkup(
            page,
            bounds,
            color,
            PdfAnnotationType.StrikeOut,
            (handle, pg, index) => new PdfStrikeOutAnnotation(handle, pg, index));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfStrikeOutAnnotation"/> class.
    /// Internal constructor - use factory methods to create annotations.
    /// </summary>
    internal PdfStrikeOutAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.StrikeOut, index)
    {
    }
}
