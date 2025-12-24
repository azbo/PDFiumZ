using System;
using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a highlight annotation in a PDF document.
/// Highlight annotations mark selected text with a colored background.
/// </summary>
public sealed class PdfHighlightAnnotation : PdfTextMarkupAnnotation
{
    /// <summary>
    /// Creates a new highlight annotation on the specified page.
    /// </summary>
    /// <param name="page">The page to add the annotation to.</param>
    /// <param name="bounds">The bounding rectangle of the highlight.</param>
    /// <param name="color">The highlight color in ARGB format (default: yellow with 50% opacity).</param>
    /// <returns>A new <see cref="PdfHighlightAnnotation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">page is null.</exception>
    /// <exception cref="PdfException">Failed to create annotation.</exception>
    public static PdfHighlightAnnotation Create(PdfPage page, PdfRectangle bounds, uint color = 0x80FFFF00)
    {
        return CreateMarkup(
            page,
            bounds,
            color,
            PdfAnnotationType.Highlight,
            (handle, pg, index) => new PdfHighlightAnnotation(handle, pg, index));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfHighlightAnnotation"/> class.
    /// Internal constructor - use factory methods to create annotations.
    /// </summary>
    internal PdfHighlightAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Highlight, index)
    {
    }
}
