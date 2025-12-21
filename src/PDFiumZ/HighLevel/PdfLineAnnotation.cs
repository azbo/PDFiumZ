using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a line annotation in a PDF document.
/// Line annotations draw a straight line between two points.
/// </summary>
public sealed class PdfLineAnnotation : PdfAnnotation
{
    /// <summary>
    /// Creates a new line annotation on the specified page.
    /// </summary>
    /// <param name="page">The page to add the annotation to.</param>
    /// <param name="startX">The X coordinate of the line start point.</param>
    /// <param name="startY">The Y coordinate of the line start point.</param>
    /// <param name="endX">The X coordinate of the line end point.</param>
    /// <param name="endY">The Y coordinate of the line end point.</param>
    /// <param name="color">The line color in ARGB format (default: solid red).</param>
    /// <param name="width">The line width in points (default: 1.0).</param>
    /// <returns>A new <see cref="PdfLineAnnotation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">page is null.</exception>
    /// <exception cref="PdfException">Failed to create annotation.</exception>
    public static PdfLineAnnotation Create(
        PdfPage page,
        double startX,
        double startY,
        double endX,
        double endY,
        uint color = 0xFFFF0000,
        double width = 1.0)
    {
        if (page is null)
            throw new ArgumentNullException(nameof(page));

        page.ThrowIfDisposed();

        // Create line annotation (type 4)
        var handle = fpdf_annot.FPDFPageCreateAnnot(page._handle!, (int)PdfAnnotationType.Line);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create line annotation.");
        }

        var annotation = new PdfLineAnnotation(handle, page, -1);

        try
        {
            // Calculate bounding rectangle that encompasses the line
            var minX = Math.Min(startX, endX);
            var minY = Math.Min(startY, endY);
            var maxX = Math.Max(startX, endX);
            var maxY = Math.Max(startY, endY);

            // Add some padding for the line width
            var padding = width / 2.0;
            annotation.Bounds = new PdfRectangle(
                minX - padding,
                minY - padding,
                (maxX - minX) + width,
                (maxY - minY) + width);

            // Set color
            annotation.Color = color;

            // Set border width
            fpdf_annot.FPDFAnnotSetBorder(annotation._handle!, 0, 0, (float)width);

            return annotation;
        }
        catch
        {
            annotation.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLineAnnotation"/> class.
    /// Internal constructor - use factory methods to create annotations.
    /// </summary>
    internal PdfLineAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Line, index)
    {
    }
}
