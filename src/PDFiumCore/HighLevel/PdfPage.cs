using System;
using System.Runtime.InteropServices;

namespace PDFiumCore.HighLevel;

/// <summary>
/// Represents a single page in a PDF document.
/// Must not outlive its parent PdfDocument.
/// </summary>
public sealed unsafe class PdfPage : IDisposable
{
    private FpdfPageT? _handle;
    private readonly int _index;
    private readonly PdfDocument _document;
    private bool _disposed;

    /// <summary>
    /// Gets the zero-based page index.
    /// </summary>
    public int Index => _index;

    /// <summary>
    /// Gets the page width in points (1/72 inch).
    /// </summary>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public double Width
    {
        get
        {
            ThrowIfDisposed();
            return fpdfview.FPDF_GetPageWidth(_handle!);
        }
    }

    /// <summary>
    /// Gets the page height in points (1/72 inch).
    /// </summary>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public double Height
    {
        get
        {
            ThrowIfDisposed();
            return fpdfview.FPDF_GetPageHeight(_handle!);
        }
    }

    /// <summary>
    /// Gets the page rotation (0, 90, 180, 270 degrees).
    /// </summary>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public int Rotation
    {
        get
        {
            ThrowIfDisposed();
            return fpdf_edit.FPDFPageGetRotation(_handle!);
        }
    }

    /// <summary>
    /// Gets the page size as a tuple (width, height).
    /// </summary>
    public (double Width, double Height) Size => (Width, Height);

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfPage"/> class.
    /// Internal constructor - created by PdfDocument only.
    /// </summary>
    internal PdfPage(FpdfPageT handle, int index, PdfDocument document)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        _index = index;
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <summary>
    /// Renders the page to a bitmap image with default options.
    /// </summary>
    /// <returns>A <see cref="PdfImage"/> containing the rendered result.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfRenderException">Rendering failed.</exception>
    public PdfImage RenderToImage()
    {
        return RenderToImage(RenderOptions.Default);
    }

    /// <summary>
    /// Renders the page to a bitmap image with specified options.
    /// </summary>
    /// <param name="options">Rendering configuration.</param>
    /// <returns>A <see cref="PdfImage"/> containing the rendered result.</returns>
    /// <exception cref="ArgumentNullException">options is null.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfRenderException">Rendering failed.</exception>
    public PdfImage RenderToImage(RenderOptions options)
    {
        ThrowIfDisposed();
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        // Apply options to calculate final dimensions
        var (width, height) = options.CalculateDimensions(Width, Height);
        var format = options.HasTransparency ? FPDFBitmapFormat.BGRA : FPDFBitmapFormat.BGRx;

        // Create bitmap
        var bitmap = fpdfview.FPDFBitmapCreateEx(
            (int)width,
            (int)height,
            (int)format,
            IntPtr.Zero,
            0);

        if (bitmap is null || bitmap.__Instance == IntPtr.Zero)
            throw new PdfRenderException($"Failed to create bitmap ({width}x{height}).");

        try
        {
            // Fill background if not transparent
            if (!options.HasTransparency)
            {
                fpdfview.FPDFBitmapFillRect(
                    bitmap, 0, 0, (int)width, (int)height, options.BackgroundColor);
            }

            // Prepare matrix and clipping
            using var matrix = options.CreateMatrix(Width, Height);
            using var clipping = options.CreateClipping(width, height);

            // Render
            fpdfview.FPDF_RenderPageBitmapWithMatrix(
                bitmap, _handle!, matrix, clipping, (int)options.Flags);

            // Wrap in managed object
            return new PdfImage(bitmap, (int)width, (int)height);
        }
        catch
        {
            // Cleanup on error
            fpdfview.FPDFBitmapDestroy(bitmap);
            throw;
        }
    }

    /// <summary>
    /// Extracts text content from the page.
    /// </summary>
    /// <returns>Extracted text as a string.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to load text from the page.</exception>
    public string ExtractText()
    {
        ThrowIfDisposed();

        var textPage = fpdf_text.FPDFTextLoadPage(_handle!);
        if (textPage is null || textPage.__Instance == IntPtr.Zero)
            throw new PdfException($"Failed to load text for page {_index}.");

        try
        {
            var charCount = fpdf_text.FPDFTextCountChars(textPage);
            if (charCount == 0) return string.Empty;

            // Allocate buffer (UTF-16 array)
            var buffer = new ushort[charCount + 1];

            var bytesWritten = fpdf_text.FPDFTextGetText(textPage, 0, charCount, ref buffer[0]);

            if (bytesWritten <= 1) return string.Empty;

            // Convert UTF-16 to .NET string (bytesWritten includes null terminator)
            fixed (ushort* pBuffer = buffer)
            {
                return new string((char*)pBuffer, 0, bytesWritten - 1);
            }
        }
        finally
        {
            fpdf_text.FPDFTextClosePage(textPage);
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="PdfPage"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        if (_handle is not null)
        {
            fpdfview.FPDF_ClosePage(_handle);
            _handle = null;
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PdfPage));
    }
}
