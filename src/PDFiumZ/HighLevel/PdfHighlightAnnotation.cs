using System;
using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a highlight annotation in a PDF document.
/// Highlight annotations mark selected text with a colored background.
/// </summary>
public sealed class PdfHighlightAnnotation : PdfAnnotation
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
        if (page is null)
            throw new ArgumentNullException(nameof(page));

        page.ThrowIfDisposed();

        // Create highlight annotation (type 9)
        var handle = fpdf_annot.FPDFPageCreateAnnot(page._handle!, (int)PdfAnnotationType.Highlight);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create highlight annotation.");
        }

        var annotation = new PdfHighlightAnnotation(handle, page, -1);

        // Set bounds
        annotation.Bounds = bounds;

        // Set color
        annotation.Color = color;

        // Set default quad points (single rectangle matching bounds)
        annotation.SetQuadPoints(new[] { bounds });

        return annotation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfHighlightAnnotation"/> class.
    /// Internal constructor - use factory methods to create annotations.
    /// </summary>
    internal PdfHighlightAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Highlight, index)
    {
    }

    /// <summary>
    /// Gets the quad points that define the highlighted regions.
    /// Each quad point represents a quadrilateral region (typically a text selection).
    /// </summary>
    /// <returns>An array of rectangles representing the highlighted regions.</returns>
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
    /// Sets the quad points that define the highlighted regions.
    /// Each rectangle is converted to a quadrilateral for the highlight.
    /// </summary>
    /// <param name="rectangles">The rectangles to highlight.</param>
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

        // Note: PDFium doesn't have a clear API, so we'll append new points
        // In practice, we need to work around this limitation

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
    /// Adds a rectangular region to the highlight.
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
