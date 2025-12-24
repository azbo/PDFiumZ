using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PDFiumZ.Utilities;

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
    private FpdfTextpageT? _textPageHandle;

    /// <summary>
    /// Gets the internal text page handle, creating it if necessary.
    /// The handle is cached until the page is disposed.
    /// </summary>
    internal FpdfTextpageT GetTextPageHandle()
    {
        ThrowIfDisposed();
        if (_textPageHandle != null && _textPageHandle.__Instance != IntPtr.Zero)
        {
            return _textPageHandle;
        }

        _textPageHandle = fpdf_text.FPDFTextLoadPage(_handle!);
        if (_textPageHandle is null || _textPageHandle.__Instance == IntPtr.Zero)
            throw new PdfException($"Failed to load text for page {_index}.");

        return _textPageHandle;
    }

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
    /// Renders the page to a bitmap image.
    /// </summary>
    /// <param name="options">Rendering configuration. Omit to use default options.</param>
    /// <returns>A <see cref="PdfImage"/> containing the rendered result.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfRenderException">Rendering failed.</exception>
    public PdfImage RenderToImage(RenderOptions? options = null)
    {
        ThrowIfDisposed();
        var opt = options ?? RenderOptions.Default;

        // Apply options to calculate final dimensions
        var (width, height) = opt.CalculateDimensions(Width, Height);
        var format = opt.HasTransparency ? FPDFBitmapFormat.BGRA : FPDFBitmapFormat.BGRx;

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
            if (!opt.HasTransparency)
            {
                fpdfview.FPDFBitmapFillRect(
                    bitmap, 0, 0, (int)width, (int)height, opt.BackgroundColor);
            }

            // Prepare matrix and clipping
            using var matrix = opt.CreateMatrix(Width, Height);
            using var clipping = opt.CreateClipping(width, height);

            // Render
            fpdfview.FPDF_RenderPageBitmapWithMatrix(
                bitmap, _handle!, matrix, clipping, (int)opt.Flags);

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
    /// Asynchronously renders the page to a bitmap image.
    /// </summary>
    /// <param name="options">Rendering configuration. Omit to use default options.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PdfImage"/> with the rendered result.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfRenderException">Rendering failed.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public Task<PdfImage> RenderToImageAsync(RenderOptions? options = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return RenderToImage(options);
        }, cancellationToken);
    }

    /// <summary>
    /// Generates a thumbnail image of the page with specified maximum width.
    /// Height is calculated to maintain aspect ratio.
    /// </summary>
    /// <param name="maxWidth">Maximum width of the thumbnail in pixels (default: 200).</param>
    /// <param name="quality">Rendering quality: 0 (low/fast) to 2 (high/slow) (default: 1).</param>
    /// <returns>A <see cref="PdfImage"/> containing the thumbnail.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">maxWidth must be positive.</exception>
    /// <exception cref="PdfRenderException">Thumbnail generation failed.</exception>
    public PdfImage GenerateThumbnail(int maxWidth = 200, int quality = 1)
    {
        ThrowIfDisposed();

        if (maxWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxWidth), "Maximum width must be positive.");

        if (quality < 0 || quality > 2)
            throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 0 (low) and 2 (high).");

        // Calculate scale to fit within maxWidth while maintaining aspect ratio
        var scale = maxWidth / Width;

        // Build render options based on quality level
        var options = quality switch
        {
            0 => RenderOptions.Default
                .WithScale(scale)
                .RemoveFlags(RenderFlags.RenderAnnotations), // Low quality: skip annotations
            1 => RenderOptions.Default
                .WithScale(scale), // Medium quality: default settings
            2 => RenderOptions.Default
                .WithScale(scale)
                .AddFlags(RenderFlags.OptimizeTextForLcd), // High quality: LCD text rendering
            _ => RenderOptions.Default.WithScale(scale)
        };

        return RenderToImage(options);
    }

    /// <summary>
    /// Asynchronously generates a thumbnail image of the page with specified maximum width.
    /// </summary>
    /// <param name="maxWidth">Maximum width of the thumbnail in pixels (default: 200).</param>
    /// <param name="quality">Rendering quality: 0 (low/fast) to 2 (high/slow) (default: 1).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the thumbnail image.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">maxWidth must be positive.</exception>
    /// <exception cref="PdfRenderException">Thumbnail generation failed.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public Task<PdfImage> GenerateThumbnailAsync(int maxWidth = 200, int quality = 1, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GenerateThumbnail(maxWidth, quality);
        }, cancellationToken);
    }

    /// <summary>
    /// Extracts text content from the page.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Extracted text as a string.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to load text from the page.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public string ExtractText(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        var textPage = GetTextPageHandle();

        var charCount = fpdf_text.FPDFTextCountChars(textPage);
        if (charCount == 0) return string.Empty;

        // Use ArrayPool to avoid allocating large arrays on the heap
        var buffer = ArrayPool<ushort>.Shared.Rent(charCount + 1);
        try
        {
            var bytesWritten = fpdf_text.FPDFTextGetText(textPage, 0, charCount, ref buffer[0]);

            if (bytesWritten <= 1) return string.Empty;

            unsafe
            {
                fixed (ushort* pBuffer = buffer)
                {
                    return new string((char*)pBuffer, 0, bytesWritten - 1);
                }
            }
        }
        finally
        {
            ArrayPool<ushort>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Searches for text on the page and returns all matches.
    /// </summary>
    /// <param name="searchText">Text to search for.</param>
    /// <param name="matchCase">If true, performs case-sensitive search.</param>
    /// <param name="matchWholeWord">If true, only matches whole words.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of search results with position information.</returns>
    /// <exception cref="ArgumentNullException">searchText is null.</exception>
    /// <exception cref="ArgumentException">searchText is empty.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to load text or perform search.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public IReadOnlyList<PdfTextSearchResult> SearchText(string searchText, bool matchCase = false, bool matchWholeWord = false, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        if (searchText is null)
            throw new ArgumentNullException(nameof(searchText));
        if (string.IsNullOrEmpty(searchText))
            throw new ArgumentException("Search text cannot be empty.", nameof(searchText));

        var results = new List<PdfTextSearchResult>();

        var textPage = GetTextPageHandle();

        uint flags = 0;
        if (matchCase)
            flags |= 0x00000001;
        if (matchWholeWord)
            flags |= 0x00000002;

        // Use optimized UTF-16 conversion
        var searchUtf16 = searchText.ToNullTerminatedUtf16Array();

        var searchHandle = fpdf_text.FPDFTextFindStart(textPage, ref searchUtf16[0], flags, 0);
        if (searchHandle is null || searchHandle.__Instance == IntPtr.Zero)
        {
            return results;
        }

        try
        {
            while (fpdf_text.FPDFTextFindNext(searchHandle) != 0)
            {
                var charIndex = fpdf_text.FPDFTextGetSchResultIndex(searchHandle);
                var charCount = fpdf_text.FPDFTextGetSchCount(searchHandle);

                if (charIndex < 0 || charCount <= 0)
                    continue;

                var matchedText = GetTextRange(textPage, charIndex, charCount);
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

    /// <summary>
    /// Gets text for a specific character range.
    /// </summary>
    private string GetTextRange(FpdfTextpageT textPage, int startIndex, int count)
    {
        // Use ArrayPool for buffer reuse
        var buffer = ArrayPool<ushort>.Shared.Rent(count + 1);
        try
        {
            var bytesWritten = fpdf_text.FPDFTextGetText(textPage, startIndex, count, ref buffer[0]);

            if (bytesWritten <= 1)
                return string.Empty;

            unsafe
            {
                fixed (ushort* pBuffer = buffer)
                {
                    return new string((char*)pBuffer, 0, bytesWritten - 1);
                }
            }
        }
        finally
        {
            ArrayPool<ushort>.Shared.Return(buffer);
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
    /// Gets form field annotations from the page.
    /// </summary>
    /// <param name="index">Optional zero-based annotation index to get a specific form field. If null, gets all form fields.</param>
    /// <returns>An enumerable of form fields.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public IEnumerable<PdfFormField> GetFormFields(int? index = null)
    {
        if (index.HasValue)
        {
            var field = GetAnnotation(index.Value) as PdfFormField;
            return field != null ? [field] : [];
        }
        return GetAnnotations<PdfFormField>();
    }

    /// <summary>
    /// Gets hyperlinks from the page.
    /// </summary>
    /// <param name="x">Optional X coordinate to get a link at a specific point.</param>
    /// <param name="y">Optional Y coordinate to get a link at a specific point.</param>
    /// <returns>An enumerable of links.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public IEnumerable<PdfLink> GetLinks(double? x = null, double? y = null)
    {
        ThrowIfDisposed();

        if (x.HasValue && y.HasValue)
        {
            var link = GetLinkAtPoint(x.Value, y.Value);
            if (link != null)
                yield return link;
            yield break;
        }

        foreach (var link in EnumerateLinks())
        {
            yield return link;
        }
    }

    private IEnumerable<PdfLink> EnumerateLinks()
    {
        var links = new List<PdfLink>();
        unsafe
        {
            int pos = 0;
            while (true)
            {
                IntPtr linkPtr = IntPtr.Zero;
                int result = fpdf_doc.__Internal.FPDFLinkEnumerate(_handle!.__Instance, &pos, (IntPtr)(&linkPtr));
                if (result == 0 || linkPtr == IntPtr.Zero)
                    break;

                links.Add(new PdfLink(FpdfLinkT.__GetOrCreateInstance(linkPtr, false), this));
            }
        }
        return links;
    }

    private PdfLink? GetLinkAtPoint(double x, double y)
    {
        ThrowIfDisposed();

        var linkHandle = fpdf_doc.FPDFLinkGetLinkAtPoint(_handle!, x, y);
        if (linkHandle == null || linkHandle.__Instance == IntPtr.Zero)
            return null;

        return new PdfLink(linkHandle, this);
    }

    /// <summary>
    /// Gets page objects (images, text, paths, etc.) from the page.
    /// </summary>
    /// <param name="index">Optional zero-based object index to get a specific object. If null, gets all objects.</param>
    /// <returns>An enumerable of page objects.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public IEnumerable<PdfPageObject> GetPageObjects(int? index = null)
    {
        ThrowIfDisposed();

        if (index.HasValue)
        {
            yield return GetPageObject(index.Value);
            yield break;
        }

        var count = PageObjectCount;
        for (int i = 0; i < count; i++)
        {
            yield return GetPageObject(i);
        }
    }

    /// <summary>
    /// Gets the number of page objects (images, text, paths, etc.) on this page.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public int PageObjectCount
    {
        get
        {
            ThrowIfDisposed();
            return fpdf_edit.FPDFPageCountObjects(_handle!);
        }
    }

    private PdfPageObject GetPageObject(int index)
    {
        ThrowIfDisposed();

        var objectCount = PageObjectCount;
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
    /// Extracts images from the page.
    /// </summary>
    /// <param name="index">Optional zero-based page object index to extract a specific image. If null, extracts all images.</param>
    /// <returns>A list of extracted images.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public IReadOnlyList<PdfExtractedImage> ExtractImages(int? index = null)
    {
        ThrowIfDisposed();

        if (index.HasValue)
        {
            var extracted = ExtractImage(index.Value);
            return extracted != null ? new[] { extracted } : Array.Empty<PdfExtractedImage>();
        }

        var images = new List<PdfExtractedImage>();
        var objectCount = PageObjectCount;

        for (int i = 0; i < objectCount; i++)
        {
            var extracted = ExtractImage(i);
            if (extracted != null)
            {
                images.Add(extracted);
            }
        }

        return images;
    }

    private PdfExtractedImage? ExtractImage(int objectIndex)
    {
        var objectCount = PageObjectCount;
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

        if (_textPageHandle is not null)
        {
            fpdf_text.FPDFTextClosePage(_textPageHandle);
            _textPageHandle = null;
        }

        if (_handle is not null)
        {
            fpdfview.FPDF_ClosePage(_handle);
            _handle = null;
        }

        _disposed = true;
    }

    #region Annotation Management

    /// <summary>
    /// Gets annotations from the page, optionally filtered by type and/or index.
    /// </summary>
    /// <typeparam name="T">The type of annotation to retrieve. Defaults to <see cref="PdfAnnotation"/>.</typeparam>
    /// <param name="index">Optional zero-based annotation index to get a specific annotation. If null, gets all annotations.</param>
    /// <returns>An enumerable of annotations.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public IEnumerable<T> GetAnnotations<T>(int? index = null) where T : PdfAnnotation
    {
        ThrowIfDisposed();

        if (index.HasValue)
        {
            var annotation = GetAnnotation(index.Value);
            if (annotation is T typedAnnotation)
                yield return typedAnnotation;
            yield break;
        }

        var count = AnnotationCount;
        for (int i = 0; i < count; i++)
        {
            var annotation = GetAnnotation(i);
            if (annotation is T typedAnnotation)
                yield return typedAnnotation;
        }
    }

    /// <summary>
    /// Gets all annotations on the page.
    /// </summary>
    /// <param name="index">Optional zero-based annotation index to get a specific annotation.</param>
    /// <returns>An enumerable of annotations.</returns>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public IEnumerable<PdfAnnotation> GetAnnotations(int? index = null) => GetAnnotations<PdfAnnotation>(index);

    /// <summary>
    /// Gets the number of annotations on this page.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    public int AnnotationCount
    {
        get
        {
            ThrowIfDisposed();
            return fpdf_annot.FPDFPageGetAnnotCount(_handle!);
        }
    }

    private PdfAnnotation? GetAnnotation(int index)
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
            PdfAnnotationType.Square => new PdfSquareAnnotation(handle, this, index),
            PdfAnnotationType.Circle => new PdfCircleAnnotation(handle, this, index),
            PdfAnnotationType.Line => new PdfLineAnnotation(handle, this, index),
            PdfAnnotationType.Underline => new PdfUnderlineAnnotation(handle, this, index),
            PdfAnnotationType.StrikeOut => new PdfStrikeOutAnnotation(handle, this, index),
            PdfAnnotationType.Ink => new PdfInkAnnotation(handle, this, index),
            PdfAnnotationType.FreeText => new PdfFreeTextAnnotation(handle, this, index),
            PdfAnnotationType.Widget => new PdfFormField(handle, _document.CreateFormHandle(), this, index),
            PdfAnnotationType.Link => new PdfLinkAnnotation(handle, this, index),
            // Add other annotation types as they are implemented
            _ => new GenericAnnotation(handle, this, annotType, index)
        };
    }

    /// <summary>
    /// Removes annotations from the page by their indices.
    /// Indices are processed in descending order to maintain correct mapping during removal.
    /// </summary>
    /// <param name="indices">The zero-based indices of the annotations to remove.</param>
    /// <exception cref="ArgumentNullException">indices is null.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to remove one or more annotations.</exception>
    public void RemoveAnnotations(params int[] indices)
    {
        if (indices is null)
            throw new ArgumentNullException(nameof(indices));

        ThrowIfDisposed();

        // Sort indices in descending order to avoid index shifts during removal
        var sortedIndices = indices.Distinct().OrderByDescending(i => i).ToArray();

        foreach (var index in sortedIndices)
        {
            if (index < 0)
                continue;

            var result = fpdf_annot.FPDFPageRemoveAnnot(_handle!, index);
            if (result == 0)
            {
                throw new PdfException($"Failed to remove annotation at index {index}.");
            }
        }
    }

    /// <summary>
    /// Removes the specified annotations from the page.
    /// </summary>
    /// <param name="annotations">The annotations to remove.</param>
    /// <exception cref="ArgumentNullException">annotations is null.</exception>
    /// <exception cref="ObjectDisposedException">The page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to remove one or more annotations.</exception>
    public void RemoveAnnotations(params PdfAnnotation[] annotations)
    {
        if (annotations is null)
            throw new ArgumentNullException(nameof(annotations));

        var indices = annotations
            .Where(a => a != null && a.Index >= 0)
            .Select(a => a.Index)
            .ToArray();

        if (indices.Length > 0)
        {
            RemoveAnnotations(indices);
        }
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
