using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Abstract base class for text markup annotations (highlight, underline, strikeout).
/// Provides common creation logic for text markup types.
/// </summary>
public abstract class PdfTextMarkupAnnotation : PdfAnnotation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfTextMarkupAnnotation"/> class.
    /// Internal constructor - use factory methods in derived classes to create annotations.
    /// </summary>
    internal PdfTextMarkupAnnotation(
        FpdfAnnotationT handle,
        PdfPage page,
        PdfAnnotationType type,
        int index)
        : base(handle, page, type, index)
    {
    }

    /// <summary>
    /// Core factory method for creating text markup annotations.
    /// Used by derived classes to avoid code duplication.
    /// </summary>
    /// <param name="page">The page to add the annotation to.</param>
    /// <param name="bounds">The bounding rectangle of the markup.</param>
    /// <param name="color">The markup color in ARGB format.</param>
    /// <param name="annotationType">The specific type of text markup annotation.</param>
    /// <param name="factory">Factory function to create the annotation instance.</param>
    /// <typeparam name="T">The concrete annotation type.</typeparam>
    /// <returns>A new text markup annotation instance.</returns>
    /// <exception cref="ArgumentNullException">page is null.</exception>
    /// <exception cref="PdfException">Failed to create annotation.</exception>
    protected static T CreateMarkup<T>(
        PdfPage page,
        PdfRectangle bounds,
        uint color,
        PdfAnnotationType annotationType,
        Func<FpdfAnnotationT, PdfPage, int, T> factory)
        where T : PdfTextMarkupAnnotation
    {
        if (page is null)
            throw new ArgumentNullException(nameof(page));

        page.ThrowIfDisposed();

        // Create annotation with specified type
        var handle = fpdf_annot.FPDFPageCreateAnnot(page._handle!, (int)annotationType);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException($"Failed to create {annotationType} annotation.");
        }

        // Create instance using factory
        var annotation = factory(handle, page, -1);

        // Set bounds
        annotation.Bounds = bounds;

        // Set color
        annotation.Color = color;

        // Set default quad points (single rectangle matching bounds)
        annotation.SetQuadPoints(new[] { bounds });

        return annotation;
    }
}
