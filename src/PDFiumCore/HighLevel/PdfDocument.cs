using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PDFiumCore.HighLevel;

/// <summary>
/// Represents a PDF document with automatic resource management.
/// Owns the native document handle and ensures proper cleanup.
/// </summary>
public sealed class PdfDocument : IDisposable
{
    private FpdfDocumentT? _handle;
    private readonly string? _filePath;
    private bool _disposed;

    /// <summary>
    /// Gets the file path if document was loaded from file, null if from memory.
    /// </summary>
    public string? FilePath => _filePath;

    /// <summary>
    /// Gets the number of pages in the document.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public int PageCount
    {
        get
        {
            ThrowIfDisposed();
            return fpdfview.FPDF_GetPageCount(_handle!);
        }
    }

    /// <summary>
    /// Gets document permissions as a bitmask.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public uint Permissions
    {
        get
        {
            ThrowIfDisposed();
            return fpdfview.FPDF_GetDocPermissions(_handle!);
        }
    }

    /// <summary>
    /// Gets the PDF file version (14 for 1.4, 17 for 1.7, etc.).
    /// </summary>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public int FileVersion
    {
        get
        {
            ThrowIfDisposed();
            int version = 0;
            fpdfview.FPDF_GetFileVersion(_handle!, ref version);
            return version;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfDocument"/> class.
    /// Internal constructor - factory methods only.
    /// </summary>
    private PdfDocument(FpdfDocumentT handle, string? filePath)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        _filePath = filePath;
    }

    /// <summary>
    /// Opens a PDF document from a file path.
    /// </summary>
    /// <param name="filePath">Path to the PDF file.</param>
    /// <param name="password">Optional password for encrypted PDFs (UTF-8 or Latin-1).</param>
    /// <returns>A new <see cref="PdfDocument"/> instance.</returns>
    /// <exception cref="ArgumentNullException">filePath is null.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    /// <exception cref="PdfLoadException">Failed to load PDF (invalid format, wrong password, etc.).</exception>
    /// <exception cref="InvalidOperationException">PDFium library not initialized.</exception>
    public static PdfDocument Open(string filePath, string? password = null)
    {
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"PDF file not found: {filePath}", filePath);

        EnsureLibraryInitialized();

        var handle = fpdfview.FPDF_LoadDocument(filePath, password);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            var error = GetLastError();
            throw new PdfLoadException($"Failed to load PDF from '{filePath}': {error}");
        }

        return new PdfDocument(handle, filePath);
    }

    /// <summary>
    /// Opens a PDF document from a byte array in memory.
    /// </summary>
    /// <param name="data">PDF file content as byte array.</param>
    /// <param name="password">Optional password for encrypted PDFs.</param>
    /// <returns>A new <see cref="PdfDocument"/> instance.</returns>
    /// <exception cref="ArgumentNullException">data is null.</exception>
    /// <exception cref="ArgumentException">data is empty.</exception>
    /// <exception cref="PdfLoadException">Failed to load PDF.</exception>
    /// <exception cref="InvalidOperationException">PDFium library not initialized.</exception>
    public static PdfDocument OpenFromMemory(byte[] data, string? password = null)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length == 0)
            throw new ArgumentException("PDF data cannot be empty.", nameof(data));

        EnsureLibraryInitialized();

        // Pin memory and load
        var gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            var handle = fpdfview.FPDF_LoadMemDocument(gcHandle.AddrOfPinnedObject(), data.Length, password);
            if (handle is null || handle.__Instance == IntPtr.Zero)
            {
                var error = GetLastError();
                throw new PdfLoadException($"Failed to load PDF from memory: {error}");
            }

            return new PdfDocument(handle, null);
        }
        finally
        {
            gcHandle.Free();
        }
    }

    /// <summary>
    /// Gets a page by zero-based index.
    /// </summary>
    /// <param name="index">Zero-based page index.</param>
    /// <returns>A <see cref="PdfPage"/> instance that must be disposed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to load page.</exception>
    public PdfPage GetPage(int index)
    {
        ThrowIfDisposed();
        if (index < 0 || index >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Page index {index} is out of range (0-{PageCount - 1}).");

        var pageHandle = fpdfview.FPDF_LoadPage(_handle!, index);
        if (pageHandle is null || pageHandle.__Instance == IntPtr.Zero)
            throw new PdfException($"Failed to load page {index}.");

        return new PdfPage(pageHandle, index, this);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="PdfDocument"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        if (_handle is not null)
        {
            fpdfview.FPDF_CloseDocument(_handle);
            _handle = null;
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PdfDocument));
    }

    private static void EnsureLibraryInitialized()
    {
        if (!PdfiumLibrary.IsInitialized)
            throw new InvalidOperationException(
                "PDFium library not initialized. Call PdfiumLibrary.Initialize() first.");
    }

    private static string GetLastError()
    {
        var error = fpdfview.FPDF_GetLastError();
        return error switch
        {
            0 => "Success",
            1 => "Unknown error",
            2 => "File not found or could not be opened",
            3 => "Invalid PDF format",
            4 => "Password required or incorrect",
            5 => "Unsupported security scheme",
            6 => "Page not found or content error",
            _ => $"Error code {error}"
        };
    }
}
