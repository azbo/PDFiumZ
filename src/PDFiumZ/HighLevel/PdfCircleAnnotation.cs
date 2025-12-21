using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a circle/ellipse annotation in a PDF document.
/// Circle annotations display a circular or elliptical shape.
/// </summary>
public sealed class PdfCircleAnnotation : PdfAnnotation
{
    /// <summary>
    /// Creates a new circle/ellipse annotation on the specified page.
    /// </summary>
    /// <param name="page">The page to add the annotation to.</param>
    /// <param name="bounds">The bounding rectangle of the circle/ellipse.</param>
    /// <param name="borderColor">The border color in ARGB format (default: solid blue). Set to 0 for no border.</param>
    /// <param name="fillColor">The fill color in ARGB format (default: transparent). Set to 0 for no fill.</param>
    /// <param name="borderWidth">The border width in points (default: 1.0).</param>
    /// <returns>A new <see cref="PdfCircleAnnotation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">page is null.</exception>
    /// <exception cref="PdfException">Failed to create annotation.</exception>
    public static PdfCircleAnnotation Create(
        PdfPage page,
        PdfRectangle bounds,
        uint borderColor = 0xFF0000FF,
        uint fillColor = 0x00000000,
        double borderWidth = 1.0)
    {
        if (page is null)
            throw new ArgumentNullException(nameof(page));

        page.ThrowIfDisposed();

        // Create circle annotation (type 6)
        var handle = fpdf_annot.FPDFPageCreateAnnot(page._handle!, (int)PdfAnnotationType.Circle);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create circle annotation.");
        }

        var annotation = new PdfCircleAnnotation(handle, page, -1);

        try
        {
            // Set bounds
            annotation.Bounds = bounds;

            // Set border color
            if (borderColor != 0)
            {
                annotation.SetBorderColor(borderColor);
            }

            // Set fill color (interior color)
            if (fillColor != 0)
            {
                annotation.SetFillColor(fillColor);
            }

            // Set border width
            fpdf_annot.FPDFAnnotSetBorder(annotation._handle!, 0, 0, (float)borderWidth);

            return annotation;
        }
        catch
        {
            annotation.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfCircleAnnotation"/> class.
    /// Internal constructor - use factory methods to create annotations.
    /// </summary>
    internal PdfCircleAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Circle, index)
    {
    }

    /// <summary>
    /// Sets the border (stroke) color of the circle.
    /// </summary>
    /// <param name="color">The border color in ARGB format.</param>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    /// <exception cref="PdfException">Failed to set border color.</exception>
    public void SetBorderColor(uint color)
    {
        ThrowIfDisposed();

        uint a = (color >> 24) & 0xFF;
        uint r = (color >> 16) & 0xFF;
        uint g = (color >> 8) & 0xFF;
        uint b = color & 0xFF;

        var result = fpdf_annot.FPDFAnnotSetColor(_handle!,
            FPDFANNOT_COLORTYPE.FPDFANNOT_COLORTYPE_Color,
            r, g, b, a);

        if (result == 0)
        {
            throw new PdfException("Failed to set border color.");
        }
    }

    /// <summary>
    /// Sets the fill (interior) color of the circle.
    /// </summary>
    /// <param name="color">The fill color in ARGB format.</param>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    /// <exception cref="PdfException">Failed to set fill color.</exception>
    public void SetFillColor(uint color)
    {
        ThrowIfDisposed();

        uint a = (color >> 24) & 0xFF;
        uint r = (color >> 16) & 0xFF;
        uint g = (color >> 8) & 0xFF;
        uint b = color & 0xFF;

        var result = fpdf_annot.FPDFAnnotSetColor(_handle!,
            FPDFANNOT_COLORTYPE.FPDFANNOT_COLORTYPE_InteriorColor,
            r, g, b, a);

        if (result == 0)
        {
            throw new PdfException("Failed to set fill color.");
        }
    }

    /// <summary>
    /// Gets the border (stroke) color of the circle.
    /// </summary>
    /// <returns>The border color in ARGB format.</returns>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    public uint GetBorderColor()
    {
        ThrowIfDisposed();

        uint r = 0, g = 0, b = 0, a = 255;

        var result = fpdf_annot.FPDFAnnotGetColor(_handle!,
            FPDFANNOT_COLORTYPE.FPDFANNOT_COLORTYPE_Color,
            ref r, ref g, ref b, ref a);

        if (result != 0)
        {
            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        return 0xFF000000; // Default to black
    }

    /// <summary>
    /// Gets the fill (interior) color of the circle.
    /// </summary>
    /// <returns>The fill color in ARGB format.</returns>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    public uint GetFillColor()
    {
        ThrowIfDisposed();

        uint r = 0, g = 0, b = 0, a = 0;

        var result = fpdf_annot.FPDFAnnotGetColor(_handle!,
            FPDFANNOT_COLORTYPE.FPDFANNOT_COLORTYPE_InteriorColor,
            ref r, ref g, ref b, ref a);

        if (result != 0)
        {
            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        return 0x00000000; // Default to transparent
    }
}
