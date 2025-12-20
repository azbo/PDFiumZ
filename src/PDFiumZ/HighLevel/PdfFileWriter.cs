using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Helper class to wrap a Stream for PDF save operations.
/// Implements the FPDF_FILEWRITE_ callback interface.
/// </summary>
internal sealed unsafe class PdfFileWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly FPDF_FILEWRITE_ _writeStruct;
    private readonly Delegates.Func_int___IntPtr___IntPtr_uint _writeCallback;
    private bool _disposed;

    public PdfFileWriter(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        if (!stream.CanWrite)
            throw new ArgumentException("Stream must be writable.", nameof(stream));

        // Create the callback delegate
        _writeCallback = WriteBlock;

        // Create and initialize the FPDF_FILEWRITE_ structure
        _writeStruct = new FPDF_FILEWRITE_
        {
            Version = 1,
            WriteBlock = _writeCallback
        };
    }

    /// <summary>
    /// Gets the FPDF_FILEWRITE_ structure to pass to PDFium save functions.
    /// </summary>
    public FPDF_FILEWRITE_ GetWriteStruct()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PdfFileWriter));
        return _writeStruct;
    }

    /// <summary>
    /// Callback function invoked by PDFium to write data.
    /// </summary>
    private int WriteBlock(IntPtr pThis, IntPtr pData, uint size)
    {
        try
        {
            if (_disposed || size == 0)
                return 0;

            // Copy data from unmanaged pointer to managed byte array
            var buffer = new byte[size];
            Marshal.Copy(pData, buffer, 0, (int)size);

            // Write to stream
            _stream.Write(buffer, 0, (int)size);
            return 1; // Success
        }
        catch
        {
            return 0; // Failure
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _writeStruct?.Dispose();
        _disposed = true;
    }
}
