using System;
using System.Runtime.InteropServices;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a rendered PDF page as a bitmap.
/// Provides access to raw pixel buffer. Dispose to free resources.
/// </summary>
public sealed unsafe class PdfImage : IDisposable
{
    private FpdfBitmapT? _bitmap;
    private readonly IntPtr _buffer;
    private readonly int _stride;
    private bool _disposed;

    /// <summary>
    /// Gets the image width in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the image height in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the stride (bytes per row) of the bitmap.
    /// </summary>
    public int Stride => _stride;

    /// <summary>
    /// Gets the pixel format.
    /// </summary>
    public FPDFBitmapFormat Format { get; }

    /// <summary>
    /// Gets a pointer to the raw pixel buffer (BGRA or BGRx format).
    /// WARNING: Pointer is only valid while this object is not disposed.
    /// </summary>
    public IntPtr Buffer
    {
        get
        {
            ThrowIfDisposed();
            return _buffer;
        }
    }

    /// <summary>
    /// Gets a span over the raw pixel buffer.
    /// </summary>
    /// <returns>A span containing the pixel data.</returns>
    /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
    public Span<byte> GetPixelSpan()
    {
        ThrowIfDisposed();
        return new Span<byte>(_buffer.ToPointer(), _stride * Height);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfImage"/> class.
    /// Internal constructor - created by PdfPage only.
    /// </summary>
    internal PdfImage(FpdfBitmapT bitmap, int width, int height)
    {
        _bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        Width = width;
        Height = height;
        _buffer = fpdfview.FPDFBitmapGetBuffer(bitmap);
        _stride = fpdfview.FPDFBitmapGetStride(bitmap);
        Format = (FPDFBitmapFormat)fpdfview.FPDFBitmapGetFormat(bitmap);

        if (_buffer == IntPtr.Zero)
            throw new PdfException("Failed to get bitmap buffer.");
    }

    /// <summary>
    /// Saves the image to a file in PNG format (requires extension method).
    /// Core library only provides raw buffer access.
    /// Use PDFiumZ.SkiaSharp package for easy image export.
    /// </summary>
    /// <param name="filePath">The file path to save to.</param>
    /// <exception cref="NotSupportedException">Always thrown - use extension package.</exception>
    public void SaveAsPng(string filePath)
    {
        throw new NotSupportedException(
            "Core library does not include image encoding. " +
            "Install PDFiumZ.SkiaSharp package and use .SaveAsSkiaPng() extension.");
    }

    /// <summary>
    /// Copies pixel data to a byte array.
    /// </summary>
    /// <returns>A byte array containing the pixel data.</returns>
    /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
    public byte[] ToByteArray()
    {
        ThrowIfDisposed();
        var buffer = new byte[_stride * Height];
        Marshal.Copy(_buffer, buffer, 0, buffer.Length);
        return buffer;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="PdfImage"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        if (_bitmap is not null)
        {
            fpdfview.FPDFBitmapDestroy(_bitmap);
            _bitmap = null;
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PdfImage));
    }
}
