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
