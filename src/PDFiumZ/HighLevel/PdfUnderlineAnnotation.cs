using System;
using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents an underline annotation in a PDF document.
/// Underline annotations mark selected text with an underline.
/// </summary>
public sealed class PdfUnderlineAnnotation : PdfAnnotation
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
        if (page is null)
            throw new ArgumentNullException(nameof(page));

        page.ThrowIfDisposed();

        // Create underline annotation (type 10)
        var handle = fpdf_annot.FPDFPageCreateAnnot(page._handle!, (int)PdfAnnotationType.Underline);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create underline annotation.");
        }

        var annotation = new PdfUnderlineAnnotation(handle, page, -1);

        // Set bounds
        annotation.Bounds = bounds;

        // Set color
        annotation.Color = color;

        // Set default quad points (single rectangle matching bounds)
        annotation.SetQuadPoints(new[] { bounds });

        return annotation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfUnderlineAnnotation"/> class.
    /// Internal constructor - use factory methods to create annotations.
    /// </summary>
    internal PdfUnderlineAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Underline, index)
    {
    }

    /// <summary>
    /// Gets the quad points that define the underlined regions.
    /// Each quad point represents a quadrilateral region (typically a text selection).
    /// </summary>
    /// <returns>An array of rectangles representing the underlined regions.</returns>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    public PdfRectangle[] GetQuadPoints()
    {
        ThrowIfDisposed();

        var count = (int)fpdf_annot.FPDFAnnotCountAttachmentPoints(_handle!);
        if (count == 0)
            return Array.Empty<PdfRectangle>();

        var quadPoints = new PdfRectangle[count];

        for (int i = 0; i < count; i++)
        {
            var quad = new FS_QUADPOINTSF();
            var result = fpdf_annot.FPDFAnnotGetAttachmentPoints(_handle!, (ulong)i, quad);

            if (result != 0)
            {
                // Convert quad points to rectangle
                // Quad points are in order: bottom-left, bottom-right, top-left, top-right
                var x1 = quad.X1;
                var y1 = quad.Y1;
                var x2 = quad.X2;
                var y2 = quad.Y2;
                var x3 = quad.X3;
                var y3 = quad.Y3;
                var x4 = quad.X4;
                var y4 = quad.Y4;

                // Calculate bounding rectangle
                var minX = Math.Min(Math.Min(x1, x2), Math.Min(x3, x4));
                var minY = Math.Min(Math.Min(y1, y2), Math.Min(y3, y4));
                var maxX = Math.Max(Math.Max(x1, x2), Math.Max(x3, x4));
                var maxY = Math.Max(Math.Max(y1, y2), Math.Max(y3, y4));

                quadPoints[i] = new PdfRectangle(minX, minY, maxX - minX, maxY - minY);
            }
        }

        return quadPoints;
    }

    /// <summary>
    /// Sets the quad points that define the underlined regions.
    /// Each rectangle is converted to a quadrilateral for the underline.
    /// </summary>
    /// <param name="rectangles">The rectangles to underline.</param>
    /// <exception cref="ArgumentNullException">rectangles is null.</exception>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    /// <exception cref="PdfException">Failed to set quad points.</exception>
    public void SetQuadPoints(PdfRectangle[] rectangles)
    {
        if (rectangles is null)
            throw new ArgumentNullException(nameof(rectangles));

        ThrowIfDisposed();

        // Clear existing attachment points
        var existingCount = (int)fpdf_annot.FPDFAnnotCountAttachmentPoints(_handle!);

        // Add each rectangle as a quad
        for (int i = 0; i < rectangles.Length; i++)
        {
            var rect = rectangles[i];
            var quad = CreateQuadFromRectangle(rect);

            int result;
            if (i < existingCount)
            {
                // Update existing quad
                result = fpdf_annot.FPDFAnnotSetAttachmentPoints(_handle!, (ulong)i, quad);
            }
            else
            {
                // Append new quad
                result = fpdf_annot.FPDFAnnotAppendAttachmentPoints(_handle!, quad);
            }

            if (result == 0)
            {
                throw new PdfException($"Failed to set quad point at index {i}.");
            }
        }
    }

    /// <summary>
    /// Adds a rectangular region to the underline.
    /// </summary>
    /// <param name="rectangle">The rectangle to add.</param>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    /// <exception cref="PdfException">Failed to add quad point.</exception>
    public void AddQuadPoint(PdfRectangle rectangle)
    {
        ThrowIfDisposed();

        var quad = CreateQuadFromRectangle(rectangle);
        var result = fpdf_annot.FPDFAnnotAppendAttachmentPoints(_handle!, quad);

        if (result == 0)
        {
            throw new PdfException("Failed to add quad point.");
        }
    }

    /// <summary>
    /// Creates a FS_QUADPOINTSF structure from a rectangle.
    /// </summary>
    private FS_QUADPOINTSF CreateQuadFromRectangle(PdfRectangle rect)
    {
        // Quad points order: bottom-left, bottom-right, top-left, top-right
        var quad = new FS_QUADPOINTSF
        {
            X1 = (float)rect.X,                    // bottom-left x
            Y1 = (float)rect.Y,                    // bottom-left y
            X2 = (float)(rect.X + rect.Width),     // bottom-right x
            Y2 = (float)rect.Y,                    // bottom-right y
            X3 = (float)rect.X,                    // top-left x
            Y3 = (float)(rect.Y + rect.Height),    // top-left y
            X4 = (float)(rect.X + rect.Width),     // top-right x
            Y4 = (float)(rect.Y + rect.Height)     // top-right y
        };

        return quad;
    }
}
