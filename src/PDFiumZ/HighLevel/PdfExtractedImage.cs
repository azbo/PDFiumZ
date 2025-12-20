using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents an image extracted from a PDF page.
/// Provides access to the rendered image as a bitmap.
/// </summary>
public sealed class PdfExtractedImage : IDisposable
{
    private readonly PdfImage _image;
    private bool _disposed;

    /// <summary>
    /// Gets the zero-based index of the image object on the page.
    /// </summary>
    public int ObjectIndex { get; }

    /// <summary>
    /// Gets the rendered image.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Image has been disposed.</exception>
    public PdfImage Image
    {
        get
        {
            ThrowIfDisposed();
            return _image;
        }
    }

    /// <summary>
    /// Gets the width of the rendered image in pixels.
    /// </summary>
    public int Width => _image.Width;

    /// <summary>
    /// Gets the height of the rendered image in pixels.
    /// </summary>
    public int Height => _image.Height;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfExtractedImage"/> class.
    /// </summary>
    internal PdfExtractedImage(int objectIndex, PdfImage image)
    {
        ObjectIndex = objectIndex;
        _image = image ?? throw new ArgumentNullException(nameof(image));
    }

    /// <summary>
    /// Releases all resources used by the <see cref="PdfExtractedImage"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _image?.Dispose();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PdfExtractedImage));
    }
}
