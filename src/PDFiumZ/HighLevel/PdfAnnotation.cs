using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Base class for all PDF annotations.
/// Manages the lifecycle of a PDFium annotation handle and provides common functionality.
/// </summary>
public abstract class PdfAnnotation : IDisposable
{
    internal FpdfAnnotationT? _handle;
    /// <summary>
    /// Gets the page that owns this annotation.
    /// </summary>
    protected readonly PdfPage _page;
    protected bool _disposed;
    private int _index = -1;

    /// <summary>
    /// Gets the zero-based index of this annotation in the page.
    /// Returns -1 if the annotation is not yet added to a page.
    /// </summary>
    public int Index
    {
        get => _index;
        internal set => _index = value;
    }

    /// <summary>
    /// Gets the type of this annotation.
    /// </summary>
    public PdfAnnotationType Type { get; }

    /// <summary>
    /// Gets or sets the bounding rectangle of the annotation in page coordinates.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    public virtual PdfRectangle Bounds
    {
        get
        {
            ThrowIfDisposed();

            var rect = new FS_RECTF_();
            var result = fpdf_annot.FPDFAnnotGetRect(_handle!, rect);
            if (result == 0)
            {
                throw new PdfException("Failed to get annotation bounds.");
            }

            return new PdfRectangle(rect.Left, rect.Bottom, rect.Right - rect.Left, rect.Top - rect.Bottom);
        }
        set
        {
            ThrowIfDisposed();

            var rect = new FS_RECTF_
            {
                Left = (float)value.X,
                Bottom = (float)value.Y,
                Right = (float)(value.X + value.Width),
                Top = (float)(value.Y + value.Height)
            };

            var result = fpdf_annot.FPDFAnnotSetRect(_handle!, rect);
            if (result == 0)
            {
                throw new PdfException("Failed to set annotation bounds.");
            }
        }
    }

    /// <summary>
    /// Gets or sets the color of the annotation in ARGB format.
    /// Not all annotation types support color.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    public uint Color
    {
        get
        {
            ThrowIfDisposed();
            return GetColorInternal();
        }
        set
        {
            ThrowIfDisposed();
            SetColorInternal(value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfAnnotation"/> class.
    /// Internal constructor - created by derived classes only.
    /// </summary>
    internal PdfAnnotation(FpdfAnnotationT handle, PdfPage page, PdfAnnotationType type, int index)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        _page = page ?? throw new ArgumentNullException(nameof(page));
        Type = type;
        _index = index;
    }

    /// <summary>
    /// Gets the color of the annotation. Derived classes can override this.
    /// </summary>
    protected virtual uint GetColorInternal()
    {
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
    /// Sets the color of the annotation. Derived classes can override this.
    /// </summary>
    protected virtual void SetColorInternal(uint color)
    {
        uint a = (color >> 24) & 0xFF;
        uint r = (color >> 16) & 0xFF;
        uint g = (color >> 8) & 0xFF;
        uint b = color & 0xFF;

        var result = fpdf_annot.FPDFAnnotSetColor(_handle!,
            FPDFANNOT_COLORTYPE.FPDFANNOT_COLORTYPE_Color,
            r, g, b, a);

        if (result == 0)
        {
            throw new PdfException("Failed to set annotation color.");
        }
    }

    /// <summary>
    /// Gets the quad points that define regions for text markup annotations (highlight, underline, strikeout).
    /// Each quad point represents a quadrilateral region (typically a text selection).
    /// </summary>
    /// <returns>An array of rectangles representing the regions.</returns>
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
    /// Sets the quad points that define regions for text markup annotations.
    /// Each rectangle is converted to a quadrilateral for the annotation.
    /// </summary>
    /// <param name="rectangles">The rectangles to mark.</param>
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
    /// Adds a rectangular region to the annotation.
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

    /// <summary>
    /// Throws an exception if the annotation has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    /// <summary>
    /// Disposes the annotation and releases the native handle.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the annotation.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (_handle != null)
        {
            fpdf_annot.FPDFPageCloseAnnot(_handle);
            _handle = null;
        }

        _disposed = true;
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~PdfAnnotation()
    {
        Dispose(false);
    }
}
