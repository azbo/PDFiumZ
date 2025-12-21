using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a single page in a PDF document.
/// Must not outlive its parent PdfDocument.
/// </summary>
public sealed unsafe class PdfPage : IDisposable
{
    internal FpdfPageT? _handle;
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
    /// Gets or sets the page rotation.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="ArgumentException">Invalid rotation value.</exception>
    public PdfRotation Rotation
    {
        get
        {
            ThrowIfDisposed();
            var rotation = fpdf_edit.FPDFPageGetRotation(_handle!);
            return (PdfRotation)rotation;
        }
        set
        {
            ThrowIfDisposed();
            if (!Enum.IsDefined(typeof(PdfRotation), value))
                throw new ArgumentException($"Invalid rotation value: {value}", nameof(value));

            fpdf_edit.FPDFPageSetRotation(_handle!, (int)value);
        }
    }

    /// <summary>
    /// Rotates the page by the specified angle.
    /// </summary>
    /// <param name="rotation">The rotation angle to apply.</param>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public void Rotate(PdfRotation rotation)
    {
        Rotation = rotation;
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
    /// Asynchronously renders the page to a bitmap image with default options.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PdfImage"/> with the rendered result.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfRenderException">Rendering failed.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public Task<PdfImage> RenderToImageAsync(CancellationToken cancellationToken = default)
    {
        return RenderToImageAsync(RenderOptions.Default, cancellationToken);
    }

    /// <summary>
    /// Asynchronously renders the page to a bitmap image with specified options.
    /// </summary>
    /// <param name="options">Rendering configuration.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PdfImage"/> with the rendered result.</returns>
    /// <exception cref="ArgumentNullException">options is null.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfRenderException">Rendering failed.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public Task<PdfImage> RenderToImageAsync(RenderOptions options, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return RenderToImage(options);
        }, cancellationToken);
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

    #region Annotation Management

    /// <summary>
    /// Gets the number of annotations on this page.
    /// </summary>
    /// <returns>The number of annotations.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public int GetAnnotationCount()
    {
        ThrowIfDisposed();
        return fpdf_annot.FPDFPageGetAnnotCount(_handle!);
    }

    /// <summary>
    /// Gets an annotation by its index.
    /// </summary>
    /// <param name="index">The zero-based index of the annotation.</param>
    /// <returns>The annotation at the specified index, or null if not found or unsupported type.</returns>
    /// <exception cref="ArgumentOutOfRangeException">index is negative.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public PdfAnnotation? GetAnnotation(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");

        ThrowIfDisposed();

        var handle = fpdf_annot.FPDFPageGetAnnot(_handle!, index);
        if (handle is null || handle.__Instance == IntPtr.Zero)
            return null;

        // Get annotation type
        var subtype = fpdf_annot.FPDFAnnotGetSubtype(handle);
        var annotType = (PdfAnnotationType)subtype;

        // Create appropriate annotation instance
        return annotType switch
        {
            PdfAnnotationType.Highlight => new PdfHighlightAnnotation(handle, this, index),
            PdfAnnotationType.Text => new PdfTextAnnotation(handle, this, index),
            PdfAnnotationType.Stamp => new PdfStampAnnotation(handle, this, index),
            // Add other annotation types as they are implemented
            _ => new GenericAnnotation(handle, this, annotType, index)
        };
    }

    /// <summary>
    /// Gets all annotations on the page.
    /// </summary>
    /// <returns>An enumerable of all annotations on the page.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public IEnumerable<PdfAnnotation> GetAnnotations()
    {
        ThrowIfDisposed();

        var count = GetAnnotationCount();
        for (int i = 0; i < count; i++)
        {
            var annotation = GetAnnotation(i);
            if (annotation is not null)
                yield return annotation;
        }
    }

    /// <summary>
    /// Gets all annotations of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of annotation to retrieve.</typeparam>
    /// <returns>An enumerable of annotations of the specified type.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public IEnumerable<T> GetAnnotations<T>() where T : PdfAnnotation
    {
        return GetAnnotations().OfType<T>();
    }

    /// <summary>
    /// Removes an annotation from the page by its index.
    /// </summary>
    /// <param name="index">The zero-based index of the annotation to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">index is negative.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to remove annotation.</exception>
    public void RemoveAnnotation(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");

        ThrowIfDisposed();

        var result = fpdf_annot.FPDFPageRemoveAnnot(_handle!, index);
        if (result == 0)
        {
            throw new PdfException($"Failed to remove annotation at index {index}.");
        }
    }

    /// <summary>
    /// Removes the specified annotation from the page.
    /// </summary>
    /// <param name="annotation">The annotation to remove.</param>
    /// <exception cref="ArgumentNullException">annotation is null.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to remove annotation.</exception>
    public void RemoveAnnotation(PdfAnnotation annotation)
    {
        if (annotation is null)
            throw new ArgumentNullException(nameof(annotation));

        if (annotation.Index < 0)
            throw new ArgumentException("Annotation is not added to a page.", nameof(annotation));

        RemoveAnnotation(annotation.Index);
    }

    #endregion

    #region Content Editing

    /// <summary>
    /// Begins editing page content by creating a content editor.
    /// The editor must be disposed after use.
    /// </summary>
    /// <returns>A new <see cref="PdfContentEditor"/> instance for editing page content.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public PdfContentEditor BeginEdit()
    {
        ThrowIfDisposed();
        return new PdfContentEditor(this);
    }

    /// <summary>
    /// Regenerates the page content stream to persist all changes made through content editing.
    /// This must be called after all editing operations and before saving the document.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to generate page content.</exception>
    public void GenerateContent()
    {
        ThrowIfDisposed();

        var result = fpdf_edit.FPDFPageGenerateContent(_handle!);
        if (result == 0)
        {
            throw new PdfException("Failed to generate page content.");
        }
    }

    #endregion

    internal void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PdfPage));
    }
}
