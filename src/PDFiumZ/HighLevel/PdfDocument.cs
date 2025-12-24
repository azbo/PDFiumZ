using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PDFiumZ.HighLevel;

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
    private int? _pageCountCache;

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
            if (_pageCountCache is null)
                _pageCountCache = fpdfview.FPDF_GetPageCount(_handle!);
            return _pageCountCache.Value;
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
    /// Gets whether this document contains interactive form fields.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public bool HasForm
    {
        get
        {
            ThrowIfDisposed();
            // Check if document has form dictionary by creating a form handle
            // If form handle is null, document doesn't have forms
            var formHandle = CreateFormHandle();
            if (formHandle is null || formHandle.__Instance == IntPtr.Zero)
            {
                return false;
            }

            DestroyFormHandle(formHandle);
            return true;
        }
    }

    /// <summary>
    /// Gets the security and encryption information for this document.
    /// Includes encryption status, security handler version, and permission flags.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public PdfSecurityInfo Security
    {
        get
        {
            ThrowIfDisposed();

            // Get security handler revision (-1 if not encrypted)
            var revision = fpdfview.FPDF_GetSecurityHandlerRevision(_handle!);
            var isEncrypted = revision >= 0;

            // Get permissions
            var permissions = fpdfview.FPDF_GetDocPermissions(_handle!);
            var userPermissions = fpdfview.FPDF_GetDocUserPermissions(_handle!);

            return new PdfSecurityInfo(isEncrypted, revision, permissions, userPermissions);
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
        _pageCountCache = fpdfview.FPDF_GetPageCount(_handle);
    }

    /// <summary>
    /// Opens a PDF document from a file path.
    /// </summary>
    /// <param name="filePath">Path to the PDF file.</param>
    /// <param name="password">Optional password for encrypted PDFs (UTF-8 or Latin-1).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A new <see cref="PdfDocument"/> instance.</returns>
    /// <exception cref="ArgumentNullException">filePath is null.</exception>
    /// <exception cref="FileNotFoundException">File does not exist.</exception>
    /// <exception cref="PdfLoadException">Failed to load PDF (invalid format, wrong password, etc.).</exception>
    /// <exception cref="InvalidOperationException">PDFium library not initialized.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public static PdfDocument Open(string filePath, string? password = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            cancellationToken.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A new <see cref="PdfDocument"/> instance.</returns>
    /// <exception cref="ArgumentNullException">data is null.</exception>
    /// <exception cref="ArgumentException">data is empty.</exception>
    /// <exception cref="PdfLoadException">Failed to load PDF.</exception>
    /// <exception cref="InvalidOperationException">PDFium library not initialized.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public static PdfDocument Open(byte[] data, string? password = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            cancellationToken.ThrowIfCancellationRequested();

        if (data is null)
            throw new ArgumentNullException(nameof(data));
        if (data.Length == 0)
            throw new ArgumentException("PDF data cannot be empty.", nameof(data));

        EnsureLibraryInitialized();

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
    /// Opens a PDF document from a stream.
    /// </summary>
    /// <param name="stream">Stream containing the PDF file data.</param>
    /// <param name="password">Optional password for encrypted PDFs.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A new <see cref="PdfDocument"/> instance.</returns>
    /// <exception cref="ArgumentNullException">stream is null.</exception>
    /// <exception cref="PdfLoadException">Failed to load PDF.</exception>
    /// <exception cref="InvalidOperationException">PDFium library not initialized.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public static PdfDocument Open(Stream stream, string? password = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            cancellationToken.ThrowIfCancellationRequested();

        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        if (stream is MemoryStream ms)
        {
            return Open(ms.ToArray(), password, cancellationToken);
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return Open(memoryStream.ToArray(), password, cancellationToken);
    }

    /// <summary>
    /// Creates a new empty PDF document.
    /// </summary>
    /// <returns>A new <see cref="PdfDocument"/> instance.</returns>
    /// <exception cref="PdfException">Failed to create document.</exception>
    /// <exception cref="InvalidOperationException">PDFium library not initialized.</exception>
    public static PdfDocument CreateNew()
    {
        EnsureLibraryInitialized();

        var handle = fpdf_edit.FPDF_CreateNewDocument();
        if (handle is null || handle.__Instance == IntPtr.Zero)
            throw new PdfException("Failed to create new PDF document.");

        return new PdfDocument(handle, null);
    }

    /// <summary>
    /// Merges multiple PDF files into a single document.
    /// </summary>
    /// <param name="filePaths">The paths of the PDF files to merge.</param>
    /// <returns>A new <see cref="PdfDocument"/> containing all pages from the source files.</returns>
    /// <exception cref="ArgumentNullException">filePaths is null.</exception>
    /// <exception cref="ArgumentException">filePaths is empty or contains null/empty strings.</exception>
    /// <exception cref="FileNotFoundException">One or more files do not exist.</exception>
    /// <exception cref="PdfException">Failed to merge documents.</exception>
    /// <exception cref="InvalidOperationException">PDFium library not initialized.</exception>
    public static PdfDocument Merge(params string[] filePaths)
    {
        if (filePaths is null)
            throw new ArgumentNullException(nameof(filePaths));
        if (filePaths.Length == 0)
            throw new ArgumentException("At least one file path must be provided.", nameof(filePaths));

        EnsureLibraryInitialized();

        // Validate all file paths first
        foreach (var path in filePaths)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("File paths cannot be null or empty.", nameof(filePaths));
            if (!File.Exists(path))
                throw new FileNotFoundException($"PDF file not found: {path}", path);
        }

        // Create new document to hold merged content
        var mergedDoc = CreateNew();

        try
        {
            // Import pages from each source document
            foreach (var filePath in filePaths)
            {
                using var sourceDoc = Open(filePath);
                mergedDoc.ImportPages(sourceDoc, mergedDoc.PageCount);
            }

            return mergedDoc;
        }
        catch
        {
            mergedDoc.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Resolves ambiguity when calling Merge with no arguments.
    /// </summary>
    /// <exception cref="ArgumentException">Always thrown.</exception>
    public static PdfDocument Merge()
    {
        throw new ArgumentException("At least one document or file path must be provided.");
    }

    /// <summary>
    /// Merges multiple PDF documents into a single document.
    /// </summary>
    /// <param name="documents">The documents to merge.</param>
    /// <returns>A new <see cref="PdfDocument"/> containing all pages from the source documents.</returns>
    /// <exception cref="ArgumentNullException">documents is null.</exception>
    /// <exception cref="ArgumentException">documents is empty.</exception>
    /// <exception cref="PdfException">Failed to merge documents.</exception>
    public static PdfDocument Merge(params PdfDocument[] documents)
    {
        if (documents is null)
            throw new ArgumentNullException(nameof(documents));
        if (documents.Length == 0)
            throw new ArgumentException("At least one document must be provided.", nameof(documents));

        EnsureLibraryInitialized();

        var mergedDoc = CreateNew();
        try
        {
            foreach (var doc in documents)
            {
                if (doc == null) continue;
                mergedDoc.ImportPages(doc, mergedDoc.PageCount);
            }
            return mergedDoc;
        }
        catch
        {
            mergedDoc.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Splits this document by extracting specific pages into a new document.
    /// </summary>
    /// <param name="pageIndices">Array of zero-based page indices to extract. If null or empty, all pages are extracted.</param>
    /// <returns>A new <see cref="PdfDocument"/> containing the extracted pages.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Any index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to split document.</exception>
    public PdfDocument Split(params int[]? pageIndices)
    {
        ThrowIfDisposed();

        var indices = (pageIndices == null || pageIndices.Length == 0)
            ? Enumerable.Range(0, PageCount).ToArray()
            : pageIndices;

        // Validate all indices
        foreach (var index in indices)
        {
            if (index < 0 || index >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndices),
                    $"Page index {index} is out of range (0-{PageCount - 1}).");
        }

        // Create new document and import pages
        var splitDoc = CreateNew();
        try
        {
            splitDoc.ImportPages(this, 0, indices);

            // Copy viewer preferences
            fpdf_ppo.FPDF_CopyViewerPreferences(splitDoc._handle!, _handle!);

            return splitDoc;
        }
        catch
        {
            splitDoc.Dispose();
            throw;
        }
    }

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
    /// Gets a range of pages, specific pages by index, or all pages if no parameters are provided.
    /// </summary>
    /// <param name="startIndex">Zero-based index of the first page to get. Defaults to 0.</param>
    /// <param name="count">Number of pages to get. Defaults to all remaining pages.</param>
    /// <returns>An enumerable of <see cref="PdfPage"/> instances that must be disposed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">startIndex or count is invalid.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to load one or more pages.</exception>
    public IEnumerable<PdfPage> GetPages(int? startIndex = null, int? count = null)
    {
        ThrowIfDisposed();

        int start = startIndex ?? 0;
        int num = count ?? (PageCount - start);

        if (start < 0 || start >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(startIndex),
                $"Start index {start} is out of range (0-{PageCount - 1}).");

        if (num < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");

        if (start + num > PageCount)
            throw new ArgumentOutOfRangeException(nameof(count),
                $"Range ({start} + {num}) exceeds page count ({PageCount}).");

        // Load pages lazily
        for (int i = start; i < start + num; i++)
        {
            yield return GetPage(i);
        }
    }

    /// <summary>
    /// Gets specific pages by their indices.
    /// </summary>
    /// <param name="pageIndices">Array of zero-based page indices.</param>
    /// <returns>An enumerable of <see cref="PdfPage"/> instances that must be disposed.</returns>
    /// <exception cref="ArgumentNullException">pageIndices is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Any index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to load one or more pages.</exception>
    public IEnumerable<PdfPage> GetPages(params int[] pageIndices)
    {
        if (pageIndices is null)
            throw new ArgumentNullException(nameof(pageIndices));

        ThrowIfDisposed();

        foreach (var index in pageIndices)
        {
            yield return GetPage(index);
        }
    }

    /// <summary>
    /// Generates thumbnail images for all pages in the document.
    /// </summary>
    /// <param name="maxWidth">Maximum width of each thumbnail in pixels (default: 200).</param>
    /// <param name="quality">Rendering quality: 0 (low/fast) to 2 (high/slow) (default: 1).</param>
    /// <returns>An enumerable collection of <see cref="PdfImage"/> thumbnails, one per page.</returns>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">maxWidth must be positive.</exception>
    /// <exception cref="PdfRenderException">Thumbnail generation failed for one or more pages.</exception>
    /// <remarks>
    /// This method lazily generates thumbnails as they are enumerated.
    /// Each thumbnail must be disposed after use.
    /// </remarks>
    public IEnumerable<PdfImage> GenerateAllThumbnails(int maxWidth = 200, int quality = 1)
    {
        ThrowIfDisposed();

        if (maxWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxWidth), "Maximum width must be positive.");

        for (int i = 0; i < PageCount; i++)
        {
            using var page = GetPage(i);
            yield return page.GenerateThumbnail(maxWidth, quality);
        }
    }

    /// <summary>
    /// Generates thumbnail images for specified pages in the document.
    /// </summary>
    /// <param name="pageIndices">Zero-based indices of pages to generate thumbnails for.</param>
    /// <param name="maxWidth">Maximum width of each thumbnail in pixels (default: 200).</param>
    /// <param name="quality">Rendering quality: 0 (low/fast) to 2 (high/slow) (default: 1).</param>
    /// <returns>An enumerable collection of <see cref="PdfImage"/> thumbnails for the specified pages.</returns>
    /// <exception cref="ArgumentNullException">pageIndices is null.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">maxWidth must be positive or any page index is out of range.</exception>
    /// <exception cref="PdfRenderException">Thumbnail generation failed for one or more pages.</exception>
    public IEnumerable<PdfImage> GenerateThumbnails(int[] pageIndices, int maxWidth = 200, int quality = 1)
    {
        if (pageIndices is null)
            throw new ArgumentNullException(nameof(pageIndices));

        ThrowIfDisposed();

        if (maxWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxWidth), "Maximum width must be positive.");

        foreach (var index in pageIndices)
        {
            using var page = GetPage(index);
            yield return page.GenerateThumbnail(maxWidth, quality);
        }
    }

    /// <summary>
    /// Creates a new blank page and adds it to the document.
    /// </summary>
    /// <param name="width">Page width in points (1/72 inch). Default is A4 width.</param>
    /// <param name="height">Page height in points (1/72 inch). Default is A4 height.</param>
    /// <param name="index">Zero-based index where to insert the page (-1 for the end of the document).</param>
    /// <returns>The newly created <see cref="PdfPage"/> instance that must be disposed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Width, height, or index is invalid.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to create page.</exception>
    public PdfPage CreatePage(double width = 595, double height = 842, int index = -1)
    {
        ThrowIfDisposed();

        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

        var originalCount = PageCount;
        if (index < -1 || index > originalCount)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Insert index {index} is out of range (-1 to {originalCount}).");

        var pageIndex = index == -1 ? originalCount : index;
        var pageHandle = fpdf_edit.FPDFPageNew(_handle!, pageIndex, width, height);

        if (pageHandle is null || pageHandle.__Instance == IntPtr.Zero)
            throw new PdfException($"Failed to create new page at index {pageIndex} with dimensions {width}x{height}.");

        try
        {
            var result = fpdf_edit.FPDFPageGenerateContent(pageHandle);
            if (result == 0)
                throw new PdfException("Failed to generate content for new page.");
        }
        catch
        {
            fpdfview.FPDF_ClosePage(pageHandle);
            throw;
        }

        _pageCountCache = originalCount + 1;
        return new PdfPage(pageHandle, pageIndex, this);
    }

    /// <summary>
    /// Creates a new blank page with a standard size and adds it to the document.
    /// </summary>
    /// <param name="size">The standard page size.</param>
    /// <param name="index">Zero-based index where to insert the page (-1 for the end of the document).</param>
    /// <returns>The newly created <see cref="PdfPage"/> instance that must be disposed.</returns>
    public PdfPage CreatePage(PdfPageSize size, int index = -1)
    {
        var (width, height) = GetPageDimensions(size);
        return CreatePage(width, height, index);
    }

    private static (double Width, double Height) GetPageDimensions(PdfPageSize size)
    {
        return size switch
        {
            PdfPageSize.A0 => (2384, 3370),
            PdfPageSize.A1 => (1684, 2384),
            PdfPageSize.A2 => (1191, 1684),
            PdfPageSize.A3 => (842, 1191),
            PdfPageSize.A4 => (595, 842),
            PdfPageSize.A5 => (420, 595),
            PdfPageSize.A6 => (298, 420),
            PdfPageSize.A7 => (210, 298),
            PdfPageSize.A8 => (147, 210),
            PdfPageSize.A9 => (105, 147),
            PdfPageSize.A10 => (74, 105),

            PdfPageSize.B0 => (2835, 4008),
            PdfPageSize.B1 => (2004, 2835),
            PdfPageSize.B2 => (1417, 2004),
            PdfPageSize.B3 => (1001, 1417),
            PdfPageSize.B4 => (709, 1001),
            PdfPageSize.B5 => (499, 709),
            PdfPageSize.B6 => (354, 499),
            PdfPageSize.B7 => (249, 354),
            PdfPageSize.B8 => (176, 249),
            PdfPageSize.B9 => (125, 176),
            PdfPageSize.B10 => (88, 125),

            PdfPageSize.C0 => (2599, 3677),
            PdfPageSize.C1 => (1837, 2599),
            PdfPageSize.C2 => (1298, 1837),
            PdfPageSize.C3 => (918, 1298),
            PdfPageSize.C4 => (649, 918),
            PdfPageSize.C5 => (459, 649),
            PdfPageSize.C6 => (323, 459),
            PdfPageSize.C7 => (230, 323),
            PdfPageSize.C8 => (162, 230),
            PdfPageSize.C9 => (113, 162),
            PdfPageSize.C10 => (79, 113),

            PdfPageSize.Letter => (612, 792),
            PdfPageSize.Legal => (612, 1008),
            PdfPageSize.Tabloid => (792, 1224),
            PdfPageSize.Ledger => (1224, 792),
            PdfPageSize.Executive => (522, 756),
            PdfPageSize.Statement => (396, 612),
            PdfPageSize.Folio => (612, 936),
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, "Unsupported page size.")
        };
    }

    /// <summary>
    /// Gets page labels for the specified page indices, or all page labels if no indices are provided.
    /// Page labels allow custom page numbering (e.g., "i", "ii", "iii" for front matter).
    /// </summary>
    /// <param name="pageIndices">Optional zero-based page indices to get labels for. If null or empty, gets labels for all pages.</param>
    /// <returns>A dictionary mapping page index to its label string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Any index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public unsafe Dictionary<int, string> GetPageLabels(params int[]? pageIndices)
    {
        ThrowIfDisposed();

        var labels = new Dictionary<int, string>();
        var indices = (pageIndices == null || pageIndices.Length == 0)
            ? Enumerable.Range(0, PageCount)
            : pageIndices;

        foreach (var index in indices)
        {
            if (index < 0 || index >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndices),
                    $"Page index {index} is out of range (0-{PageCount - 1}).");

            // Get required buffer size
            var bufferSize = fpdf_doc.FPDF_GetPageLabel(_handle!, index, IntPtr.Zero, 0);

            if (bufferSize <= 2)
            {
                labels[index] = (index + 1).ToString();
                continue;
            }

            var buffer = new ushort[bufferSize / 2];
            fixed (ushort* pBuffer = buffer)
            {
                var bytesWritten = fpdf_doc.FPDF_GetPageLabel(
                    _handle!, index, (IntPtr)pBuffer, bufferSize);

                if (bytesWritten <= 2)
                    labels[index] = (index + 1).ToString();
                else
                    labels[index] = new string((char*)pBuffer, 0, (int)(bytesWritten / 2) - 1);
            }
        }

        return labels;
    }

    /// <summary>
    /// Deletes pages from the document.
    /// </summary>
    /// <param name="pageIndices">Zero-based indices of pages to delete.</param>
    /// <exception cref="ArgumentNullException">pageIndices is null.</exception>
    /// <exception cref="ArgumentException">pageIndices is empty.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public void DeletePages(params int[] pageIndices)
    {
        if (pageIndices is null)
            throw new ArgumentNullException(nameof(pageIndices));
        if (pageIndices.Length == 0)
            throw new ArgumentException("Page indices array cannot be empty.", nameof(pageIndices));

        ThrowIfDisposed();
        var originalCount = PageCount;

        // Validate all indices first
        foreach (var index in pageIndices)
        {
            if (index < 0 || index >= originalCount)
                throw new ArgumentException($"Page index {index} is out of range (0-{originalCount - 1}).", nameof(pageIndices));
        }

        // Sort indices in descending order to avoid index shifts during deletion
        var sortedIndices = pageIndices.Distinct().OrderByDescending(i => i).ToArray();

        // Delete pages in descending order
        foreach (var index in sortedIndices)
        {
            fpdf_edit.FPDFPageDelete(_handle!, index);
        }

        _pageCountCache = originalCount - sortedIndices.Length;
    }

    /// <summary>
    /// Moves pages to a new position within the document.
    /// </summary>
    /// <param name="destIndex">Destination index where pages will be moved.</param>
    /// <param name="pageIndices">Array of zero-based page indices to move.</param>
    /// <exception cref="ArgumentNullException">pageIndices is null.</exception>
    /// <exception cref="ArgumentException">pageIndices is empty or contains invalid indices.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to move pages.</exception>
    public void MovePages(int destIndex, params int[] pageIndices)
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
    /// <param name="insertAtIndex">Index where to insert pages in this document (-1 for end).</param>
    /// <param name="pageRange">Optional page range string (e.g., "1,3,5-7"). Null for all pages.</param>
    /// <exception cref="ArgumentNullException">sourceDoc is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">insertAtIndex is invalid.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to import pages.</exception>
    public void ImportPages(PdfDocument sourceDoc, int insertAtIndex = -1, string? pageRange = null)
    {
        ThrowIfDisposed();
        if (sourceDoc is null)
            throw new ArgumentNullException(nameof(sourceDoc));
        if (sourceDoc._disposed)
            throw new ObjectDisposedException(nameof(sourceDoc), "Source document has been disposed.");
        var originalCount = PageCount;
        if (insertAtIndex < -1 || insertAtIndex > originalCount)
            throw new ArgumentOutOfRangeException(nameof(insertAtIndex),
                $"Insert index {insertAtIndex} is out of range (-1 for end, 0-{originalCount}).");

        var result = fpdf_ppo.FPDF_ImportPages(_handle!, sourceDoc._handle!, pageRange, insertAtIndex);
        if (result == 0)
            throw new PdfException($"Failed to import pages from source document (range: {pageRange ?? "all"}).");

        if (pageRange is null)
            _pageCountCache = originalCount + sourceDoc.PageCount;
        else
            _pageCountCache = null;
    }

    /// <summary>
    /// Imports specific pages by index from another document.
    /// </summary>
    /// <param name="sourceDoc">Source PDF document to copy pages from.</param>
    /// <param name="insertAtIndex">Index where to insert pages in this document (-1 for end).</param>
    /// <param name="pageIndices">Array of zero-based page indices to import from source. If empty, imports all pages.</param>
    /// <exception cref="ArgumentNullException">sourceDoc or pageIndices is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">insertAtIndex is invalid.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to import pages.</exception>
    public void ImportPages(PdfDocument sourceDoc, int insertAtIndex, params int[] pageIndices)
    {
        if (pageIndices is null)
            throw new ArgumentNullException(nameof(pageIndices));

        if (pageIndices.Length == 0)
        {
            ImportPages(sourceDoc, insertAtIndex, (string?)null);
            return;
        }

        ThrowIfDisposed();
        if (sourceDoc is null)
            throw new ArgumentNullException(nameof(sourceDoc));
        if (sourceDoc._disposed)
            throw new ObjectDisposedException(nameof(sourceDoc), "Source document has been disposed.");

        var originalCount = PageCount;
        if (insertAtIndex < -1 || insertAtIndex > originalCount)
            throw new ArgumentOutOfRangeException(nameof(insertAtIndex),
                $"Insert index {insertAtIndex} is out of range (-1 for end, 0-{originalCount}).");

        var result = fpdf_ppo.FPDF_ImportPagesByIndex(_handle!, sourceDoc._handle!, ref pageIndices[0], (uint)pageIndices.Length, insertAtIndex);
        if (result == 0)
            throw new PdfException($"Failed to import {pageIndices.Length} page(s) from source document.");

        _pageCountCache = originalCount + pageIndices.Length;
    }

    /// <summary>
    /// Saves the document to a file.
    /// </summary>
    /// <param name="filePath">Path where to save the PDF file.</param>
    /// <exception cref="ArgumentNullException">filePath is null.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to save document.</exception>
    public void Save(string filePath)
    {
        ThrowIfDisposed();
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        Save(fileStream);
    }

    /// <summary>
    /// Saves the document to a stream.
    /// </summary>
    /// <param name="stream">The stream to save the PDF to.</param>
    /// <exception cref="ArgumentNullException">stream is null.</exception>
    /// <exception cref="ArgumentException">stream is not writable.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to save document.</exception>
    public void Save(Stream stream)
    {
        ThrowIfDisposed();
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));
        if (!stream.CanWrite)
            throw new ArgumentException("Stream must be writable.", nameof(stream));

        using var writer = new PdfFileWriter(stream);

        var result = fpdf_save.FPDF_SaveAsCopy(_handle!, writer.GetWriteStruct(), 0);
        if (result == 0)
            throw new PdfException("Failed to save PDF to stream.");
    }

    /// <summary>
    /// Asynchronously saves the document to a file.
    /// </summary>
    /// <param name="filePath">Path where to save the PDF file.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">filePath is null.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to save document.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public Task SaveAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Save(filePath);
        }, cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves the document to a stream.
    /// </summary>
    /// <param name="stream">The stream to save the PDF to.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">stream is null.</exception>
    /// <exception cref="ArgumentException">stream is not writable.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to save document.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public Task SaveAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Save(stream);
        }, cancellationToken);
    }

    /// <summary>
    /// Adds a text watermark to pages in the document.
    /// </summary>
    /// <param name="text">The watermark text.</param>
    /// <param name="position">The position of the watermark on each page.</param>
    /// <param name="options">Watermark appearance options. If null, default options are used.</param>
    /// <param name="pageIndices">Optional indices of pages to add watermark to. If null or empty, adds to all pages.</param>
    /// <exception cref="ArgumentNullException">text is null.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to add watermark to one or more pages.</exception>
    public void AddTextWatermark(string text, WatermarkPosition position, WatermarkOptions? options = null, params int[]? pageIndices)
    {
        if (text is null)
            throw new ArgumentNullException(nameof(text));

        ThrowIfDisposed();

        options ??= new WatermarkOptions();

        if (pageIndices == null || pageIndices.Length == 0)
        {
            var pageCount = PageCount;
            for (int i = 0; i < pageCount; i++)
            {
                using var page = GetPage(i);
                AddTextWatermarkToPage(page, text, position, options);
            }
        }
        else
        {
            foreach (var index in pageIndices)
            {
                if (index < 0 || index >= PageCount)
                    throw new ArgumentOutOfRangeException(nameof(pageIndices),
                        $"Page index {index} is out of range (0-{PageCount - 1}).");

                using var page = GetPage(index);
                AddTextWatermarkToPage(page, text, position, options);
            }
        }
    }

    /// <summary>
    /// Adds a header and footer to pages in the document.
    /// </summary>
    /// <param name="headerText">Text for the header. Can include {PageNumber} and {TotalPages} placeholders.</param>
    /// <param name="footerText">Text for the footer. Can include {PageNumber} and {TotalPages} placeholders.</param>
    /// <param name="options">Header/Footer appearance options.</param>
    /// <param name="pageIndices">Optional indices of pages to add header/footer to. If null or empty, adds to all pages.</param>
    public void AddHeaderFooter(string? headerText, string? footerText, HeaderFooterOptions? options = null, params int[]? pageIndices)
    {
        ThrowIfDisposed();

        options ??= new HeaderFooterOptions();

        if (options.Margin < 0)
            throw new ArgumentOutOfRangeException(nameof(options.Margin), "Margin must be non-negative.");
        if (options.FontSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.FontSize), "FontSize must be greater than 0.");
        if (options.Opacity < 0 || options.Opacity > 1)
            throw new ArgumentOutOfRangeException(nameof(options.Opacity), "Opacity must be in [0, 1].");

        if (string.IsNullOrEmpty(headerText) && string.IsNullOrEmpty(footerText))
            return;

        using var font = PdfFont.Load(this, options.Font);
        var totalPages = PageCount;

        if (pageIndices == null || pageIndices.Length == 0)
        {
            for (int i = 0; i < totalPages; i++)
            {
                ProcessPageForHeaderFooter(i);
            }
        }
        else
        {
            foreach (var i in pageIndices)
            {
                if (i < 0 || i >= totalPages)
                    throw new ArgumentOutOfRangeException(nameof(pageIndices),
                        $"Page index {i} is out of range (0-{totalPages - 1}).");
                ProcessPageForHeaderFooter(i);
            }
        }

        void ProcessPageForHeaderFooter(int i)
        {
            using var page = GetPage(i);
            using var editor = page.BeginEdit();

            var pageNumber = i + 1;
            if (!string.IsNullOrEmpty(headerText))
            {
                var y = page.Height - options.Margin - options.FontSize;
                AddHeaderFooterTextToPage(page, headerText, y, options.HeaderAlignment, options, font, pageNumber, totalPages);
            }

            if (!string.IsNullOrEmpty(footerText))
            {
                var y = options.Margin;
                AddHeaderFooterTextToPage(page, footerText, y, options.FooterAlignment, options, font, pageNumber, totalPages);
            }

            editor.GenerateContent();
        }
    }

    /// <summary>
    /// Adds a text watermark to a single page.
    /// </summary>
    private unsafe void AddTextWatermarkToPage(PdfPage page, string text, WatermarkPosition position, WatermarkOptions options)
    {
        // Load font
        var font = PdfFont.Load(this, options.Font);

        try
        {
            using var editor = page.BeginEdit();

            // Create text object
            var textObj = fpdf_edit.FPDFPageObjNewTextObj(
                _handle!,
                font.Name,
                (float)options.FontSize);

            if (textObj is null || textObj.__Instance == IntPtr.Zero)
                throw new PdfException($"Failed to create watermark text object.");

            try
            {
                // Set text content (UTF-16LE)
                var utf16Array = new ushort[text.Length + 1];
                for (int i = 0; i < text.Length; i++)
                {
                    utf16Array[i] = text[i];
                }
                utf16Array[text.Length] = 0;

                var result = fpdf_edit.FPDFTextSetText(textObj, ref utf16Array[0]);
                if (result == 0)
                    throw new PdfException("Failed to set watermark text content.");

                // Set text color with opacity (ARGB format)
                uint alpha = (uint)(options.Opacity * 255);
                uint color = options.Color;
                uint r = (color >> 16) & 0xFF;
                uint g = (color >> 8) & 0xFF;
                uint b = color & 0xFF;

                fpdf_edit.FPDFPageObjSetFillColor(textObj, r, g, b, alpha);

                // Calculate text dimensions (rough estimate based on font size)
                double textWidth = text.Length * options.FontSize * 0.6;  // Approximate width
                double textHeight = options.FontSize;

                // Calculate position based on page size and watermark position
                double pageWidth = page.Width;
                double pageHeight = page.Height;
                double x = 0, y = 0;

                switch (position)
                {
                    case WatermarkPosition.Center:
                        x = (pageWidth - textWidth) / 2;
                        y = (pageHeight - textHeight) / 2;
                        break;
                    case WatermarkPosition.TopLeft:
                        x = 50;
                        y = pageHeight - textHeight - 50;
                        break;
                    case WatermarkPosition.TopCenter:
                        x = (pageWidth - textWidth) / 2;
                        y = pageHeight - textHeight - 50;
                        break;
                    case WatermarkPosition.TopRight:
                        x = pageWidth - textWidth - 50;
                        y = pageHeight - textHeight - 50;
                        break;
                    case WatermarkPosition.MiddleLeft:
                        x = 50;
                        y = (pageHeight - textHeight) / 2;
                        break;
                    case WatermarkPosition.MiddleRight:
                        x = pageWidth - textWidth - 50;
                        y = (pageHeight - textHeight) / 2;
                        break;
                    case WatermarkPosition.BottomLeft:
                        x = 50;
                        y = 50;
                        break;
                    case WatermarkPosition.BottomCenter:
                        x = (pageWidth - textWidth) / 2;
                        y = 50;
                        break;
                    case WatermarkPosition.BottomRight:
                        x = pageWidth - textWidth - 50;
                        y = 50;
                        break;
                }

                // Apply transformation with rotation
                // For rotation, we need to rotate around the text center
                double rotationRad = options.Rotation * Math.PI / 180.0;
                double cos = Math.Cos(rotationRad);
                double sin = Math.Sin(rotationRad);

                // Calculate center point for rotation
                double centerX = x + textWidth / 2;
                double centerY = y + textHeight / 2;

                // Transformation matrix with rotation around center:
                // First translate to origin, rotate, then translate back
                // Matrix: [a b c d e f] where:
                // a = cos, b = sin, c = -sin, d = cos (for rotation)
                // e, f = translation
                double finalX = centerX - (centerX * cos - centerY * sin);
                double finalY = centerY - (centerX * sin + centerY * cos);

                fpdf_edit.FPDFPageObjTransform(textObj, cos, sin, -sin, cos, finalX, finalY);

                // Insert object into page
                fpdf_edit.FPDFPageInsertObject(page._handle!, textObj);

                // Generate content to apply changes
                editor.GenerateContent();
            }
            catch
            {
                // Clean up on failure
                fpdf_edit.FPDFPageObjDestroy(textObj);
                throw;
            }
        }
        finally
        {
            font.Dispose();
        }
    }

    /// <summary>
    /// Asynchronously adds a text watermark to all pages in the document.
    /// </summary>
    /// <param name="text">The watermark text.</param>
    /// <param name="position">The position of the watermark.</param>
    /// <param name="options">Watermark options (font, size, color, etc.).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">text is null.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public Task AddTextWatermarkAsync(string text, WatermarkPosition position, WatermarkOptions? options = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            AddTextWatermark(text, position, options);
        }, cancellationToken);
    }

    private unsafe void AddHeaderFooterTextToPage(
        PdfPage page,
        string templateText,
        double y,
        HeaderFooterAlignment alignment,
        HeaderFooterOptions options,
        PdfFont font,
        int pageNumber,
        int totalPages)
    {
        var text = ReplaceHeaderFooterTokens(templateText, pageNumber, totalPages);
        if (string.IsNullOrEmpty(text))
            return;

        var textObj = fpdf_edit.FPDFPageObjNewTextObj(
            _handle!,
            font.Name,
            (float)options.FontSize);

        if (textObj is null || textObj.__Instance == IntPtr.Zero)
            throw new PdfException("Failed to create header/footer text object.");

        try
        {
            var utf16Array = new ushort[text.Length + 1];
            for (int i = 0; i < text.Length; i++)
            {
                utf16Array[i] = text[i];
            }
            utf16Array[text.Length] = 0;

            var result = fpdf_edit.FPDFTextSetText(textObj, ref utf16Array[0]);
            if (result == 0)
                throw new PdfException("Failed to set header/footer text content.");

            uint alpha = (uint)(options.Opacity * 255);
            uint color = options.Color;
            uint r = (color >> 16) & 0xFF;
            uint g = (color >> 8) & 0xFF;
            uint b = color & 0xFF;

            fpdf_edit.FPDFPageObjSetFillColor(textObj, r, g, b, alpha);

            double textWidth = text.Length * options.FontSize * 0.6;
            double x = alignment switch
            {
                HeaderFooterAlignment.Left => options.Margin,
                HeaderFooterAlignment.Center => (page.Width - textWidth) / 2,
                HeaderFooterAlignment.Right => page.Width - textWidth - options.Margin,
                _ => options.Margin
            };

            if (x < 0)
                x = 0;

            if (y < 0)
                y = 0;
            else if (y > page.Height - options.FontSize)
                y = page.Height - options.FontSize;

            fpdf_edit.FPDFPageObjTransform(textObj, 1, 0, 0, 1, x, y);
            fpdf_edit.FPDFPageInsertObject(page._handle!, textObj);
        }
        catch
        {
            fpdf_edit.FPDFPageObjDestroy(textObj);
            throw;
        }
    }

    private static string ReplaceHeaderFooterTokens(string template, int pageNumber, int totalPages)
    {
        return template
            .Replace("{page}", pageNumber.ToString())
            .Replace("{pages}", totalPages.ToString())
            .Replace("{totalPages}", totalPages.ToString());
    }

    /// <summary>
    /// Rotates pages in the document.
    /// </summary>
    /// <param name="rotation">The rotation angle to apply.</param>
    /// <param name="pageIndices">Zero-based indices of pages to rotate. If null or empty, rotates all pages.</param>
    /// <exception cref="ArgumentOutOfRangeException">One or more page indices are out of range.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public void RotatePages(PdfRotation rotation, params int[]? pageIndices)
    {
        ThrowIfDisposed();

        if (pageIndices == null || pageIndices.Length == 0)
        {
            for (int i = 0; i < PageCount; i++)
            {
                using var page = GetPage(i);
                page.Rotation = rotation;
            }
        }
        else
        {
            foreach (var index in pageIndices)
            {
                if (index < 0 || index >= PageCount)
                    throw new ArgumentOutOfRangeException(nameof(pageIndices),
                        $"Page index {index} is out of range (0-{PageCount - 1}).");

                using var page = GetPage(index);
                page.Rotation = rotation;
            }
        }
    }

    /// <summary>
    /// Flattens all form fields and annotations in the document, converting them to static content.
    /// This operation is irreversible and makes the form non-interactive.
    /// </summary>
    /// <param name="printMode">If true, flattens for print mode (includes all content). If false, flattens for normal display mode (may exclude print-only content).</param>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="PdfException">Failed to flatten forms on one or more pages.</exception>
    public void FlattenForm(bool printMode = false)
    {
        ThrowIfDisposed();

        int flag = printMode ? 1 : 0; // FLAT_PRINT = 1, FLAT_NORMALDISPLAY = 0
        var failures = new System.Collections.Generic.List<int>();

        // Flatten each page
        for (int i = 0; i < PageCount; i++)
        {
            using var page = GetPage(i);
            var result = fpdf_flatten.FPDFPageFlatten(page._handle!, flag);

            // Result values: FLATTEN_SUCCESS = 0, FLATTEN_NOTHINGTODO = 1, FLATTEN_FAIL = 2
            if (result == 2) // FLATTEN_FAIL
            {
                failures.Add(i);
            }
        }

        if (failures.Count > 0)
        {
            throw new PdfException($"Failed to flatten forms on {failures.Count} page(s): {string.Join(", ", failures)}");
        }
    }

    /// <summary>
    /// Gets the first (root-level) bookmark in the document, or null if none.
    /// </summary>
    /// <returns>The first bookmark, or null if document has no bookmarks.</returns>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public PdfBookmark? FirstBookmark
    {
        get
        {
            ThrowIfDisposed();

            // Pass null as bookmark parameter to get first root bookmark
            var bookmarkHandle = fpdf_doc.FPDFBookmarkGetFirstChild(_handle!, null);
            if (bookmarkHandle is null || bookmarkHandle.__Instance == IntPtr.Zero)
                return null;

            return new PdfBookmark(bookmarkHandle, this);
        }
    }

    /// <summary>
    /// Gets root-level bookmarks in the document.
    /// </summary>
    /// <param name="title">Optional title to search for a specific bookmark.</param>
    /// <returns>An enumerable of bookmarks.</returns>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    public IEnumerable<PdfBookmark> GetBookmarks(string? title = null)
    {
        if (title != null)
        {
            var bookmark = FindBookmark(title);
            if (bookmark != null)
                yield return bookmark;
            yield break;
        }

        var firstBookmark = FirstBookmark;
        while (firstBookmark is not null)
        {
            yield return firstBookmark;
            firstBookmark = firstBookmark.NextSibling;
        }
    }

    private unsafe PdfBookmark? FindBookmark(string title)
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

    internal void ThrowIfDisposed()
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

public enum HeaderFooterAlignment
{
    Left,
    Center,
    Right
}

public sealed class HeaderFooterOptions
{
    public double Margin { get; set; } = 36;
    public double FontSize { get; set; } = 12;
    public double Opacity { get; set; } = 1.0;
    public uint Color { get; set; } = 0xFF000000;
    public PdfStandardFont Font { get; set; } = PdfStandardFont.Helvetica;
    public HeaderFooterAlignment HeaderAlignment { get; set; } = HeaderFooterAlignment.Center;
    public HeaderFooterAlignment FooterAlignment { get; set; } = HeaderFooterAlignment.Center;
}
