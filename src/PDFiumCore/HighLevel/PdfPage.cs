using System;
using System.Collections.Generic;
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
    internal readonly PdfDocument _document;
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
    /// Searches for text on the page and returns all matches.
    /// </summary>
    /// <param name="searchText">Text to search for.</param>
    /// <param name="matchCase">If true, performs case-sensitive search.</param>
    /// <param name="matchWholeWord">If true, only matches whole words.</param>
    /// <returns>A list of search results with position information.</returns>
    /// <exception cref="ArgumentNullException">searchText is null.</exception>
    /// <exception cref="ArgumentException">searchText is empty.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to load text or perform search.</exception>
    public IReadOnlyList<PdfTextSearchResult> SearchText(string searchText, bool matchCase = false, bool matchWholeWord = false)
    {
        ThrowIfDisposed();
        if (searchText is null)
            throw new ArgumentNullException(nameof(searchText));
        if (string.IsNullOrEmpty(searchText))
            throw new ArgumentException("Search text cannot be empty.", nameof(searchText));

        var results = new List<PdfTextSearchResult>();

        var textPage = fpdf_text.FPDFTextLoadPage(_handle!);
        if (textPage is null || textPage.__Instance == IntPtr.Zero)
            throw new PdfException($"Failed to load text for page {_index}.");

        try
        {
            // Convert search text to UTF-16
            var searchUtf16 = new ushort[searchText.Length + 1];
            for (int i = 0; i < searchText.Length; i++)
            {
                searchUtf16[i] = searchText[i];
            }
            searchUtf16[searchText.Length] = 0; // Null terminator

            // Build search flags
            uint flags = 0;
            if (matchCase)
                flags |= 0x00000001; // FPDF_MATCHCASE
            if (matchWholeWord)
                flags |= 0x00000002; // FPDF_MATCHWHOLEWORD

            // Start search
            var searchHandle = fpdf_text.FPDFTextFindStart(textPage, ref searchUtf16[0], flags, 0);
            if (searchHandle is null || searchHandle.__Instance == IntPtr.Zero)
            {
                return results;
            }

            try
            {
                // Find all occurrences
                while (fpdf_text.FPDFTextFindNext(searchHandle) != 0)
                {
                    var charIndex = fpdf_text.FPDFTextGetSchResultIndex(searchHandle);
                    var charCount = fpdf_text.FPDFTextGetSchCount(searchHandle);

                    if (charIndex < 0 || charCount <= 0)
                        continue;

                    // Get matched text
                    var matchedText = GetTextRange(textPage, charIndex, charCount);

                    // Get bounding rectangles
                    var rectangles = GetTextRectangles(textPage, charIndex, charCount);

                    results.Add(new PdfTextSearchResult(charIndex, charCount, matchedText, rectangles));
                }
            }
            finally
            {
                fpdf_text.FPDFTextFindClose(searchHandle);
            }

            return results;
        }
        finally
        {
            fpdf_text.FPDFTextClosePage(textPage);
        }
    }

    /// <summary>
    /// Gets text for a specific character range.
    /// </summary>
    private string GetTextRange(FpdfTextpageT textPage, int startIndex, int count)
    {
        var buffer = new ushort[count + 1];
        var bytesWritten = fpdf_text.FPDFTextGetText(textPage, startIndex, count, ref buffer[0]);

        if (bytesWritten <= 1)
            return string.Empty;

        fixed (ushort* pBuffer = buffer)
        {
            return new string((char*)pBuffer, 0, bytesWritten - 1);
        }
    }

    /// <summary>
    /// Gets bounding rectangles for a text range.
    /// </summary>
    private IReadOnlyList<PdfRectangle> GetTextRectangles(FpdfTextpageT textPage, int startIndex, int count)
    {
        var rectangles = new List<PdfRectangle>();

        // Get number of rectangles for this text range
        var rectCount = fpdf_text.FPDFTextCountRects(textPage, startIndex, count);

        for (int i = 0; i < rectCount; i++)
        {
            double left = 0, top = 0, right = 0, bottom = 0;

            if (fpdf_text.FPDFTextGetRect(textPage, i, ref left, ref top, ref right, ref bottom) != 0)
            {
                // Convert from PDFium's (left, top, right, bottom) to PdfRectangle's (x, y, width, height)
                // PDFium's coordinate system: origin at bottom-left, Y increases upward
                rectangles.Add(new PdfRectangle(left, bottom, right - left, top - bottom));
            }
        }

        return rectangles;
    }

    /// <summary>
    /// Gets the number of form field annotations on this page.
    /// </summary>
    /// <returns>The count of form field annotations.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public int GetFormFieldCount()
    {
        ThrowIfDisposed();
        return fpdf_annot.FPDFPageGetAnnotCount(_handle!);
    }

    /// <summary>
    /// Gets a form field annotation at the specified index.
    /// Note: This returns form fields as well as other annotations. Check FieldType to verify it's a form field.
    /// </summary>
    /// <param name="index">Zero-based annotation index.</param>
    /// <returns>A <see cref="PdfFormField"/> instance, or null if the annotation is not a form field.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public PdfFormField? GetFormField(int index)
    {
        ThrowIfDisposed();

        var annotCount = GetFormFieldCount();
        if (index < 0 || index >= annotCount)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Annotation index {index} is out of range (0-{annotCount - 1}).");

        var annotHandle = fpdf_annot.FPDFPageGetAnnot(_handle!, index);
        if (annotHandle is null || annotHandle.__Instance == IntPtr.Zero)
            return null;

        // Create form handle for this document
        var formHandle = _document.CreateFormHandle();
        if (formHandle is null || formHandle.__Instance == IntPtr.Zero)
        {
            fpdf_annot.FPDFPageCloseAnnot(annotHandle);
            return null;
        }

        try
        {
            // Check if this is a form field
            var fieldType = fpdf_annot.FPDFAnnotGetFormFieldType(formHandle, annotHandle);
            if (fieldType < 0) // Not a form field
            {
                fpdf_annot.FPDFPageCloseAnnot(annotHandle);
                _document.DestroyFormHandle(formHandle);
                return null;
            }

            return new PdfFormField(annotHandle, formHandle, this, index);
        }
        catch
        {
            fpdf_annot.FPDFPageCloseAnnot(annotHandle);
            _document.DestroyFormHandle(formHandle);
            throw;
        }
    }

    /// <summary>
    /// Gets all form field annotations on this page.
    /// </summary>
    /// <returns>An enumerable of form fields.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public IEnumerable<PdfFormField> GetFormFields()
    {
        var count = GetFormFieldCount();
        for (int i = 0; i < count; i++)
        {
            var field = GetFormField(i);
            if (field != null)
            {
                yield return field;
            }
        }
    }

    /// <summary>
    /// Gets a hyperlink at the specified point on the page.
    /// </summary>
    /// <param name="x">X coordinate in page coordinate system.</param>
    /// <param name="y">Y coordinate in page coordinate system.</param>
    /// <returns>A <see cref="PdfLink"/> instance, or null if no link exists at the point.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public PdfLink? GetLinkAtPoint(double x, double y)
    {
        ThrowIfDisposed();

        var linkHandle = fpdf_doc.FPDFLinkGetLinkAtPoint(_handle!, x, y);
        if (linkHandle == null || linkHandle.__Instance == IntPtr.Zero)
            return null;

        return new PdfLink(linkHandle, this);
    }

    /// <summary>
    /// Gets the number of page objects (images, text, paths, etc.) on this page.
    /// </summary>
    /// <returns>The count of page objects.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public int GetPageObjectCount()
    {
        ThrowIfDisposed();
        return fpdf_edit.FPDFPageCountObjects(_handle!);
    }

    /// <summary>
    /// Gets information about a page object at the specified index.
    /// </summary>
    /// <param name="index">Zero-based object index.</param>
    /// <returns>A <see cref="PdfPageObject"/> with object information.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to get page object.</exception>
    public PdfPageObject GetPageObject(int index)
    {
        ThrowIfDisposed();

        var objectCount = GetPageObjectCount();
        if (index < 0 || index >= objectCount)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Object index {index} is out of range (0-{objectCount - 1}).");

        var objectHandle = fpdf_edit.FPDFPageGetObject(_handle!, index);
        if (objectHandle is null || objectHandle.__Instance == IntPtr.Zero)
            throw new PdfException($"Failed to get page object at index {index}.");

        var objectType = (PdfPageObjectType)fpdf_edit.FPDFPageObjGetType(objectHandle);

        return new PdfPageObject(index, objectType);
    }

    /// <summary>
    /// Extracts all images from the page.
    /// </summary>
    /// <returns>A list of extracted images.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public IReadOnlyList<PdfExtractedImage> ExtractImages()
    {
        ThrowIfDisposed();

        var images = new List<PdfExtractedImage>();
        var objectCount = GetPageObjectCount();

        for (int i = 0; i < objectCount; i++)
        {
            try
            {
                var objectHandle = fpdf_edit.FPDFPageGetObject(_handle!, i);
                if (objectHandle is null || objectHandle.__Instance == IntPtr.Zero)
                    continue;

                var objectType = fpdf_edit.FPDFPageObjGetType(objectHandle);
                if (objectType != 3) // FPDF_PAGEOBJ_IMAGE = 3
                    continue;

                // Get rendered bitmap for this image object
                var bitmapHandle = fpdf_edit.FPDFImageObjGetRenderedBitmap(
                    _document._handle!, _handle!, objectHandle);

                if (bitmapHandle is null || bitmapHandle.__Instance == IntPtr.Zero)
                    continue;

                // Get bitmap dimensions
                var width = fpdfview.FPDFBitmapGetWidth(bitmapHandle);
                var height = fpdfview.FPDFBitmapGetHeight(bitmapHandle);

                if (width <= 0 || height <= 0)
                {
                    fpdfview.FPDFBitmapDestroy(bitmapHandle);
                    continue;
                }

                // Wrap in managed objects
                var pdfImage = new PdfImage(bitmapHandle, width, height);
                var extractedImage = new PdfExtractedImage(i, pdfImage);

                images.Add(extractedImage);
            }
            catch
            {
                // Skip objects that fail to extract
                continue;
            }
        }

        return images;
    }

    /// <summary>
    /// Extracts a specific image object from the page.
    /// </summary>
    /// <param name="objectIndex">Zero-based page object index.</param>
    /// <returns>The extracted image, or null if the object is not an image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public PdfExtractedImage? ExtractImage(int objectIndex)
    {
        ThrowIfDisposed();

        var objectCount = GetPageObjectCount();
        if (objectIndex < 0 || objectIndex >= objectCount)
            throw new ArgumentOutOfRangeException(nameof(objectIndex),
                $"Object index {objectIndex} is out of range (0-{objectCount - 1}).");

        var objectHandle = fpdf_edit.FPDFPageGetObject(_handle!, objectIndex);
        if (objectHandle is null || objectHandle.__Instance == IntPtr.Zero)
            return null;

        var objectType = fpdf_edit.FPDFPageObjGetType(objectHandle);
        if (objectType != 3) // FPDF_PAGEOBJ_IMAGE = 3
            return null;

        // Get rendered bitmap for this image object
        var bitmapHandle = fpdf_edit.FPDFImageObjGetRenderedBitmap(
            _document._handle!, _handle!, objectHandle);

        if (bitmapHandle is null || bitmapHandle.__Instance == IntPtr.Zero)
            return null;

        // Get bitmap dimensions
        var width = fpdfview.FPDFBitmapGetWidth(bitmapHandle);
        var height = fpdfview.FPDFBitmapGetHeight(bitmapHandle);

        if (width <= 0 || height <= 0)
        {
            fpdfview.FPDFBitmapDestroy(bitmapHandle);
            return null;
        }

        // Wrap in managed objects
        var pdfImage = new PdfImage(bitmapHandle, width, height);
        return new PdfExtractedImage(objectIndex, pdfImage);
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
