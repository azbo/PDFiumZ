using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace PDFiumCore.HighLevel;

/// <summary>
/// Represents a PDF document with automatic resource management.
/// Owns the native document handle and ensures proper cleanup.
/// </summary>
public sealed class PdfDocument : IDisposable
{
    internal FpdfDocumentT? _handle;
    private readonly string? _filePath;
    private bool _disposed;
    private PdfMetadata? _metadata;

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
    /// Gets the document metadata including title, author, subject, keywords, etc.
    /// Metadata is cached after first access.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public PdfMetadata Metadata
    {
        get
        {
            ThrowIfDisposed();

            if (_metadata == null)
            {
                _metadata = new PdfMetadata(
                    title: GetMetadataString("Title"),
                    author: GetMetadataString("Author"),
                    subject: GetMetadataString("Subject"),
                    keywords: GetMetadataString("Keywords"),
                    creator: GetMetadataString("Creator"),
                    producer: GetMetadataString("Producer"),
                    creationDate: GetMetadataString("CreationDate", allowEmpty: true),
                    modificationDate: GetMetadataString("ModDate", allowEmpty: true)
                );
            }

            return _metadata;
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
    /// Gets the page label for the specified page index.
    /// Page labels allow custom page numbering (e.g., "i", "ii", "iii" for front matter).
    /// Returns the numeric page number if no custom label is defined.
    /// </summary>
    /// <param name="index">Zero-based page index.</param>
    /// <returns>The page label string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public unsafe string GetPageLabel(int index)
    {
        ThrowIfDisposed();
        if (index < 0 || index >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Page index {index} is out of range (0-{PageCount - 1}).");

        // Get required buffer size (in bytes, including null terminator)
        var bufferSize = fpdf_doc.FPDF_GetPageLabel(_handle!, index, IntPtr.Zero, 0);

        if (bufferSize <= 2) // 2 bytes = just null terminator for UTF-16
        {
            // No custom label, return numeric page number (1-based)
            return (index + 1).ToString();
        }

        // Allocate buffer (UTF-16, so divide by 2 for ushort count)
        var buffer = new ushort[bufferSize / 2];

        fixed (ushort* pBuffer = buffer)
        {
            var bytesWritten = fpdf_doc.FPDF_GetPageLabel(
                _handle!, index, (IntPtr)pBuffer, bufferSize);

            if (bytesWritten <= 2)
            {
                // Failed to get label, return numeric page number
                return (index + 1).ToString();
            }

            // Convert UTF-16 to string (bytesWritten includes null terminator)
            return new string((char*)pBuffer, 0, (int)(bytesWritten / 2) - 1);
        }
    }

    /// <summary>
    /// Gets all page labels as a dictionary mapping page index to label.
    /// Useful for generating table of contents or navigation.
    /// </summary>
    /// <returns>Dictionary with page indices (0-based) as keys and labels as values.</returns>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public Dictionary<int, string> GetAllPageLabels()
    {
        ThrowIfDisposed();

        var labels = new Dictionary<int, string>();
        for (int i = 0; i < PageCount; i++)
        {
            labels[i] = GetPageLabel(i);
        }

        return labels;
    }

    /// <summary>
    /// Inserts a new blank page at the specified index.
    /// </summary>
    /// <param name="index">Zero-based index where to insert (0 = beginning, PageCount = end).</param>
    /// <param name="width">Page width in points (1/72 inch). Standard A4 = 595.</param>
    /// <param name="height">Page height in points (1/72 inch). Standard A4 = 842.</param>
    /// <exception cref="ArgumentOutOfRangeException">Index or dimensions are invalid.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to create page.</exception>
    public void InsertBlankPage(int index, double width, double height)
    {
        ThrowIfDisposed();
        if (index < 0 || index > PageCount)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Insert index {index} is out of range (0-{PageCount}).");
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width),
                "Page dimensions must be positive.");

        var pageHandle = fpdf_edit.FPDFPageNew(_handle!, index, width, height);
        if (pageHandle is null || pageHandle.__Instance == IntPtr.Zero)
            throw new PdfException($"Failed to create blank page at index {index}.");

        // Close the page immediately as we don't need to return it
        fpdfview.FPDF_ClosePage(pageHandle);
    }

    /// <summary>
    /// Deletes the page at the specified index.
    /// </summary>
    /// <param name="index">Zero-based page index to delete.</param>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public void DeletePage(int index)
    {
        ThrowIfDisposed();
        if (index < 0 || index >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Page index {index} is out of range (0-{PageCount - 1}).");

        fpdf_edit.FPDFPageDelete(_handle!, index);
    }

    /// <summary>
    /// Moves pages to a new position within the document.
    /// </summary>
    /// <param name="pageIndices">Array of zero-based page indices to move.</param>
    /// <param name="destIndex">Destination index where pages will be moved.</param>
    /// <exception cref="ArgumentNullException">pageIndices is null.</exception>
    /// <exception cref="ArgumentException">pageIndices is empty or contains invalid indices.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to move pages.</exception>
    public void MovePages(int[] pageIndices, int destIndex)
    {
        ThrowIfDisposed();
        if (pageIndices is null)
            throw new ArgumentNullException(nameof(pageIndices));
        if (pageIndices.Length == 0)
            throw new ArgumentException("Page indices array cannot be empty.", nameof(pageIndices));
        if (destIndex < 0 || destIndex >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(destIndex),
                $"Destination index {destIndex} is out of range (0-{PageCount - 1}).");

        // Validate all indices
        foreach (var idx in pageIndices)
        {
            if (idx < 0 || idx >= PageCount)
                throw new ArgumentException($"Page index {idx} is out of range (0-{PageCount - 1}).", nameof(pageIndices));
        }

        var result = fpdf_edit.FPDF_MovePages(_handle!, ref pageIndices[0], (uint)pageIndices.Length, destIndex);
        if (result == 0)
            throw new PdfException($"Failed to move {pageIndices.Length} page(s) to index {destIndex}.");
    }

    /// <summary>
    /// Imports pages from another document using a page range string.
    /// </summary>
    /// <param name="sourceDoc">Source PDF document to copy pages from.</param>
    /// <param name="pageRange">Page range string (e.g., "1,3,5-7" or null for all pages).</param>
    /// <param name="insertAtIndex">Index where to insert pages in this document.</param>
    /// <exception cref="ArgumentNullException">sourceDoc is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">insertAtIndex is invalid.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to import pages.</exception>
    public void ImportPages(PdfDocument sourceDoc, string? pageRange = null, int insertAtIndex = -1)
    {
        ThrowIfDisposed();
        if (sourceDoc is null)
            throw new ArgumentNullException(nameof(sourceDoc));
        if (sourceDoc._disposed)
            throw new ObjectDisposedException(nameof(sourceDoc), "Source document has been disposed.");
        if (insertAtIndex < -1 || insertAtIndex > PageCount)
            throw new ArgumentOutOfRangeException(nameof(insertAtIndex),
                $"Insert index {insertAtIndex} is out of range (-1 for end, 0-{PageCount}).");

        var result = fpdf_ppo.FPDF_ImportPages(_handle!, sourceDoc._handle!, pageRange, insertAtIndex);
        if (result == 0)
            throw new PdfException($"Failed to import pages from source document (range: {pageRange ?? "all"}).");
    }

    /// <summary>
    /// Imports specific pages by index from another document.
    /// </summary>
    /// <param name="sourceDoc">Source PDF document to copy pages from.</param>
    /// <param name="pageIndices">Array of zero-based page indices to import from source.</param>
    /// <param name="insertAtIndex">Index where to insert pages in this document.</param>
    /// <exception cref="ArgumentNullException">sourceDoc or pageIndices is null.</exception>
    /// <exception cref="ArgumentException">pageIndices is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">insertAtIndex is invalid.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to import pages.</exception>
    public void ImportPagesAt(PdfDocument sourceDoc, int[] pageIndices, int insertAtIndex = -1)
    {
        ThrowIfDisposed();
        if (sourceDoc is null)
            throw new ArgumentNullException(nameof(sourceDoc));
        if (sourceDoc._disposed)
            throw new ObjectDisposedException(nameof(sourceDoc), "Source document has been disposed.");
        if (pageIndices is null)
            throw new ArgumentNullException(nameof(pageIndices));
        if (pageIndices.Length == 0)
            throw new ArgumentException("Page indices array cannot be empty.", nameof(pageIndices));
        if (insertAtIndex < -1 || insertAtIndex > PageCount)
            throw new ArgumentOutOfRangeException(nameof(insertAtIndex),
                $"Insert index {insertAtIndex} is out of range (-1 for end, 0-{PageCount}).");

        var result = fpdf_ppo.FPDF_ImportPagesByIndex(_handle!, sourceDoc._handle!, ref pageIndices[0], (uint)pageIndices.Length, insertAtIndex);
        if (result == 0)
            throw new PdfException($"Failed to import {pageIndices.Length} page(s) from source document.");
    }

    /// <summary>
    /// Saves the document to a file.
    /// </summary>
    /// <param name="filePath">Path where to save the PDF file.</param>
    /// <exception cref="ArgumentNullException">filePath is null.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to save document.</exception>
    public void SaveToFile(string filePath)
    {
        ThrowIfDisposed();
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        var writer = new PdfFileWriter(fileStream);

        var result = fpdf_save.FPDF_SaveAsCopy(_handle!, writer.GetWriteStruct(), 0);
        if (result == 0)
            throw new PdfException($"Failed to save PDF to '{filePath}'.");
    }

    /// <summary>
    /// Gets the first (root-level) bookmark in the document, or null if none.
    /// </summary>
    /// <returns>The first bookmark, or null if document has no bookmarks.</returns>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public PdfBookmark? GetFirstBookmark()
    {
        ThrowIfDisposed();

        // Pass null as bookmark parameter to get first root bookmark
        var bookmarkHandle = fpdf_doc.FPDFBookmarkGetFirstChild(_handle!, null);
        if (bookmarkHandle is null || bookmarkHandle.__Instance == IntPtr.Zero)
            return null;

        return new PdfBookmark(bookmarkHandle, this);
    }

    /// <summary>
    /// Gets all root-level bookmarks in the document.
    /// </summary>
    /// <returns>An enumerable of root bookmarks.</returns>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public IEnumerable<PdfBookmark> GetBookmarks()
    {
        var bookmark = GetFirstBookmark();
        while (bookmark is not null)
        {
            yield return bookmark;
            bookmark = bookmark.GetNextSibling();
        }
    }

    /// <summary>
    /// Finds a bookmark by its title text.
    /// </summary>
    /// <param name="title">The bookmark title to search for.</param>
    /// <returns>The bookmark if found, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">title is null.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public unsafe PdfBookmark? FindBookmark(string title)
    {
        ThrowIfDisposed();
        if (title is null)
            throw new ArgumentNullException(nameof(title));

        // Convert string to UTF-16 (ushort array)
        var titleUtf16 = new ushort[title.Length + 1];
        for (int i = 0; i < title.Length; i++)
        {
            titleUtf16[i] = title[i];
        }
        titleUtf16[title.Length] = 0; // Null terminator

        var bookmarkHandle = fpdf_doc.FPDFBookmarkFind(_handle!, ref titleUtf16[0]);
        if (bookmarkHandle is null || bookmarkHandle.__Instance == IntPtr.Zero)
            return null;

        return new PdfBookmark(bookmarkHandle, this);
    }

    /// <summary>
    /// Creates a form handle for form field operations.
    /// Internal method - used by PdfPage for form field access.
    /// </summary>
    internal FpdfFormHandleT? CreateFormHandle()
    {
        ThrowIfDisposed();

        // Pass null for formInfo to create a basic form handle (read-only operations)
        var formHandle = fpdf_formfill.FPDFDOC_InitFormFillEnvironment(_handle!, null);
        return formHandle;
    }

    /// <summary>
    /// Destroys a form handle created by CreateFormHandle.
    /// </summary>
    internal void DestroyFormHandle(FpdfFormHandleT? formHandle)
    {
        if (formHandle is not null && formHandle.__Instance != IntPtr.Zero)
        {
            fpdf_formfill.FPDFDOC_ExitFormFillEnvironment(formHandle);
        }
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

    /// <summary>
    /// Gets a metadata string for the specified tag.
    /// </summary>
    /// <param name="tag">Metadata tag (e.g., "Title", "Author", "Subject").</param>
    /// <param name="allowEmpty">If true, returns null for empty strings; otherwise returns empty string.</param>
    /// <returns>Metadata value or empty string/null if not available.</returns>
    private unsafe string? GetMetadataString(string tag, bool allowEmpty = false)
    {
        ThrowIfDisposed();

        // Get required buffer size (in bytes, including null terminator)
        var bufferSize = fpdf_doc.FPDF_GetMetaText(_handle!, tag, IntPtr.Zero, 0);

        if (bufferSize <= 2) // 2 bytes = just null terminator for UTF-16
        {
            return allowEmpty ? null : string.Empty;
        }

        // Allocate buffer (UTF-16, so divide by 2 for ushort count)
        var buffer = new ushort[bufferSize / 2];

        fixed (ushort* pBuffer = buffer)
        {
            var bytesWritten = fpdf_doc.FPDF_GetMetaText(
                _handle!, tag, (IntPtr)pBuffer, bufferSize);

            if (bytesWritten <= 2)
            {
                return allowEmpty ? null : string.Empty;
            }

            // Convert UTF-16 to string (bytesWritten includes null terminator)
            var text = new string((char*)pBuffer, 0, (int)(bytesWritten / 2) - 1);

            // Return null for empty strings if allowEmpty is true
            return (allowEmpty && string.IsNullOrEmpty(text)) ? null : text;
        }
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
