using System;
using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents an underline annotation in a PDF document.
/// Underline annotations mark selected text with an underline.
/// </summary>
public sealed class PdfUnderlineAnnotation : PdfTextMarkupAnnotation
{
    /// <summary>
    /// Creates a new underline annotation on the specified page.
    /// </summary>
    /// <param name="page">The page to add the annotation to.</param>
    /// <param name="bounds">The bounding rectangle of the underline.</param>
    /// <param name="color">The underline color in ARGB format (default: red with full opacity).</param>
    /// <returns>A new <see cref="PdfUnderlineAnnotation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">page is null.</exception>
    /// <exception cref="PdfException">Failed to create annotation.</exception>
    public static PdfUnderlineAnnotation Create(PdfPage page, PdfRectangle bounds, uint color = 0xFFFF0000)
    {
        return CreateMarkup(
            page,
            bounds,
            color,
            PdfAnnotationType.Underline,
            (handle, pg, index) => new PdfUnderlineAnnotation(handle, pg, index));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfUnderlineAnnotation"/> class.
    /// Internal constructor - use factory methods to create annotations.
    /// </summary>
    internal PdfUnderlineAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Underline, index)
    {
    }
}
