using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Provides methods for editing PDF page content by adding text, images, and shapes.
/// Must be disposed properly to free native resources.
/// </summary>
public sealed unsafe class PdfContentEditor : IDisposable
{
    private readonly PdfPage _page;
    private bool _disposed;
    private readonly List<FpdfBitmapT> _bitmaps = new();

    // Default state for fluent API
    private PdfFont? _defaultFont;
    private double _defaultFontSize = 12;
    private uint _defaultTextColor = PdfColor.Black;
    private uint _defaultStrokeColor = PdfColor.Black;
    private uint _defaultFillColor = PdfColor.Transparent;
    private double _defaultLineWidth = 1.0;

    /// <summary>
    /// Gets the page being edited.
    /// </summary>
    public PdfPage Page => _page;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfContentEditor"/> class.
    /// Internal constructor - created by PdfPage only.
    /// </summary>
    internal PdfContentEditor(PdfPage page)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }

    /// <summary>
    /// Adds text to the page at the specified position.
    /// </summary>
    /// <param name="text">The text to add.</param>
    /// <param name="x">X coordinate in page units.</param>
    /// <param name="y">Y coordinate in page units.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="fontSize">Font size in points.</param>
    /// <returns>The created text object handle for further manipulation if needed.</returns>
    /// <exception cref="ArgumentNullException">text or font is null.</exception>
    /// <exception cref="ObjectDisposedException">Editor has been disposed.</exception>
    /// <exception cref="PdfException">Failed to create or add text object.</exception>
    public FpdfPageobjectT AddText(string text, double x, double y, PdfFont font, double fontSize)
    {
        if (text is null)
            throw new ArgumentNullException(nameof(text));
        if (font is null)
            throw new ArgumentNullException(nameof(font));

        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        // Create text object
        var textObj = fpdf_edit.FPDFPageObjNewTextObj(
            _page._document._handle!,
            font.Name,
            (float)fontSize);

        if (textObj is null || textObj.__Instance == IntPtr.Zero)
        {
            throw new PdfException($"Failed to create text object with font '{font.Name}'.");
        }

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
            {
                throw new PdfException("Failed to set text content.");
            }

            // Position the text object using transformation matrix
            // Matrix: [a b c d e f] where:
            // a, d = scale (1.0 = no scale)
            // b, c = rotation/skew (0 = no rotation)
            // e, f = translation (position)
            fpdf_edit.FPDFPageObjTransform(textObj, 1, 0, 0, 1, x, y);

            // Insert object into page
            fpdf_edit.FPDFPageInsertObject(_page._handle!, textObj);

            return textObj;
        }
        catch
        {
            // Clean up on failure
            fpdf_edit.FPDFPageObjDestroy(textObj);
            throw;
        }
    }

    /// <summary>
    /// Adds an image to the page from raw bitmap data.
    /// </summary>
    /// <param name="imageData">Raw BGRA bitmap data (4 bytes per pixel).</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="bounds">Position and size on the page.</param>
    /// <returns>The created image object handle.</returns>
    /// <exception cref="ArgumentNullException">imageData is null.</exception>
    /// <exception cref="ArgumentException">imageData size doesn't match width*height*4.</exception>
    /// <exception cref="ObjectDisposedException">Editor has been disposed.</exception>
    /// <exception cref="PdfException">Failed to create or add image object.</exception>
    public FpdfPageobjectT AddImage(byte[] imageData, int width, int height, PdfRectangle bounds)
    {
        if (imageData is null)
            throw new ArgumentNullException(nameof(imageData));
        if (imageData.Length != width * height * 4)
            throw new ArgumentException($"Image data size ({imageData.Length}) doesn't match expected size ({width * height * 4}).", nameof(imageData));

        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        // Create image object
        var imageObj = fpdf_edit.FPDFPageObjNewImageObj(_page._document._handle!);
        if (imageObj is null || imageObj.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create image object.");
        }

        FpdfBitmapT? bitmap = null;
        try
        {
            // Create bitmap and copy data to it
            // alpha = 1 means BGRA format
            bitmap = fpdfview.FPDFBitmapCreate(width, height, 1);
            if (bitmap is null || bitmap.__Instance == IntPtr.Zero)
            {
                throw new PdfException("Failed to create bitmap.");
            }

            IntPtr pBuffer = fpdfview.FPDFBitmapGetBuffer(bitmap);
            if (pBuffer == IntPtr.Zero)
            {
                throw new PdfException("Failed to get bitmap buffer.");
            }

            // Copy image data to bitmap buffer
            Marshal.Copy(imageData, 0, pBuffer, imageData.Length);

            // Set bitmap to image object
            // Pass the current page handle in the pages array
            var result = fpdf_edit.FPDFImageObjSetBitmap(_page._handle!, 1, imageObj, bitmap);
            if (result == 0)
            {
                throw new PdfException("Failed to set bitmap to image object.");
            }

            // Calculate transformation matrix to position and scale image
            // In PDFium, image objects are 1x1 units by default.
            // To display at bounds size, we scale by bounds.Width and bounds.Height.
            double scaleX = bounds.Width;
            double scaleY = bounds.Height;

            // Matrix: [a b c d e f]
            // a = scaleX, d = scaleY (scale)
            // e = bounds.X, f = bounds.Y (translation)
            fpdf_edit.FPDFPageObjTransform(imageObj, scaleX, 0, 0, scaleY, bounds.X, bounds.Y);

            // Insert object into page
            fpdf_edit.FPDFPageInsertObject(_page._handle!, imageObj);

            // Keep bitmap alive until Commit/Dispose
            _bitmaps.Add(bitmap);

            return imageObj;
        }
        catch
        {
            // Clean up on failure
            if (bitmap is not null && bitmap.__Instance != IntPtr.Zero)
            {
                fpdfview.FPDFBitmapDestroy(bitmap);
            }
            fpdf_edit.FPDFPageObjDestroy(imageObj);
            throw;
        }
    }

    /// <summary>
    /// Adds a rectangle to the page.
    /// </summary>
    /// <param name="bounds">Rectangle position and size.</param>
    /// <param name="strokeColor">Stroke color in ARGB format (0xAARRGGBB). Use 0 for no stroke.</param>
    /// <param name="fillColor">Fill color in ARGB format (0xAARRGGBB). Use 0 for no fill.</param>
    /// <returns>The created rectangle object handle.</returns>
    /// <exception cref="ObjectDisposedException">Editor has been disposed.</exception>
    /// <exception cref="PdfException">Failed to create or add rectangle object.</exception>
    public FpdfPageobjectT AddRectangle(PdfRectangle bounds, uint strokeColor, uint fillColor)
    {
        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        // Create rectangle path object
        var rectObj = fpdf_edit.FPDFPageObjCreateNewRect(
            (float)bounds.X,
            (float)bounds.Y,
            (float)bounds.Width,
            (float)bounds.Height);

        if (rectObj is null || rectObj.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create rectangle object.");
        }

        return PrepareAndInsertObject(rectObj, strokeColor, fillColor);
    }

    /// <summary>
    /// Adds a line between two points.
    /// </summary>
    /// <param name="x1">Start X coordinate.</param>
    /// <param name="y1">Start Y coordinate.</param>
    /// <param name="x2">End X coordinate.</param>
    /// <param name="y2">End Y coordinate.</param>
    /// <param name="strokeColor">Line color in ARGB format. Use 0 for no stroke.</param>
    /// <param name="lineWidth">Line width. Use 0 for default.</param>
    /// <returns>The created line object handle.</returns>
    public FpdfPageobjectT AddLine(double x1, double y1, double x2, double y2, uint strokeColor, double lineWidth)
    {
        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        // Create path object
        var pathObj = fpdf_edit.FPDFPageObjCreateNewPath((float)x1, (float)y1);
        if (pathObj is null || pathObj.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create line path object.");
        }

        try
        {
            // Add line segment
            fpdf_edit.FPDFPathLineTo(pathObj, (float)x2, (float)y2);

            return PrepareAndInsertObject(pathObj, strokeColor, 0, lineWidth);
        }
        catch
        {
            fpdf_edit.FPDFPageObjDestroy(pathObj);
            throw;
        }
    }

    /// <summary>
    /// Adds a circle to the page.
    /// </summary>
    /// <param name="centerX">Center X coordinate.</param>
    /// <param name="centerY">Center Y coordinate.</param>
    /// <param name="radius">Circle radius.</param>
    /// <param name="strokeColor">Stroke color in ARGB format. Use 0 for no stroke.</param>
    /// <param name="fillColor">Fill color in ARGB format. Use 0 for no fill.</param>
    /// <returns>The created circle object handle.</returns>
    public FpdfPageobjectT AddCircle(double centerX, double centerY, double radius, uint strokeColor, uint fillColor)
    {
        return AddEllipse(new PdfRectangle(centerX - radius, centerY - radius, radius * 2, radius * 2), strokeColor, fillColor);
    }

    /// <summary>
    /// Adds an ellipse to the page.
    /// </summary>
    /// <param name="bounds">Bounding rectangle for the ellipse.</param>
    /// <param name="strokeColor">Stroke color in ARGB format. Use 0 for no stroke.</param>
    /// <param name="fillColor">Fill color in ARGB format. Use 0 for no fill.</param>
    /// <returns>The created ellipse object handle.</returns>
    public FpdfPageobjectT AddEllipse(PdfRectangle bounds, uint strokeColor, uint fillColor)
    {
        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        // Create ellipse using Bezier curves
        var pathObj = fpdf_edit.FPDFPageObjCreateNewPath(
            (float)(bounds.X + bounds.Width / 2),
            (float)(bounds.Y + bounds.Height));

        if (pathObj is null || pathObj.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create ellipse path object.");
        }

        try
        {
            var cx = bounds.X + bounds.Width / 2;
            var cy = bounds.Y + bounds.Height / 2;
            var rx = bounds.Width / 2;
            var ry = bounds.Height / 2;

            // Magic number for Bezier curve approximation of circle: 4/3 * (sqrt(2) - 1)
            var kappa = 0.5522847498;
            var ox = rx * kappa;
            var oy = ry * kappa;

            // Top-right
            fpdf_edit.FPDFPathBezierTo(pathObj,
                (float)(cx + ox), (float)(cy + ry),
                (float)(cx + rx), (float)(cy + oy),
                (float)(cx + rx), (float)cy);

            // Bottom-right
            fpdf_edit.FPDFPathBezierTo(pathObj,
                (float)(cx + rx), (float)(cy - oy),
                (float)(cx + ox), (float)(cy - ry),
                (float)cx, (float)(cy - ry));

            // Bottom-left
            fpdf_edit.FPDFPathBezierTo(pathObj,
                (float)(cx - ox), (float)(cy - ry),
                (float)(cx - rx), (float)(cy - oy),
                (float)(cx - rx), (float)cy);

            // Top-left
            fpdf_edit.FPDFPathBezierTo(pathObj,
                (float)(cx - rx), (float)(cy + oy),
                (float)(cx - ox), (float)(cy + ry),
                (float)cx, (float)(cy + ry));

            return PrepareAndInsertObject(pathObj, strokeColor, fillColor);
        }
        catch
        {
            fpdf_edit.FPDFPageObjDestroy(pathObj);
            throw;
        }
    }

    private FpdfPageobjectT PrepareAndInsertObject(FpdfPageobjectT obj, uint strokeColor, uint fillColor, double lineWidth = 0)
    {
        try
        {
            // Set draw mode
            bool hasStroke = strokeColor != 0;
            bool hasFill = fillColor != 0;
            
            // FPDFPathSetDrawMode is for path objects. 
            // For rect objects created via FPDFPageObjCreateNewRect, it also works as they are paths.
            fpdf_edit.FPDFPathSetDrawMode(obj, hasStroke ? 1 : 0, hasFill ? 1 : 0);

            // Set stroke color if specified
            if (hasStroke)
            {
                uint a = (strokeColor >> 24) & 0xFF;
                uint r = (strokeColor >> 16) & 0xFF;
                uint g = (strokeColor >> 8) & 0xFF;
                uint b = strokeColor & 0xFF;
                if (fpdf_edit.FPDFPageObjSetStrokeColor(obj, r, g, b, a) == 0)
                {
                    throw new PdfException("Failed to set stroke color.");
                }
                
                if (lineWidth > 0)
                {
                    fpdf_edit.FPDFPageObjSetStrokeWidth(obj, (float)lineWidth);
                }
            }

            // Set fill color if specified
            if (hasFill)
            {
                uint a = (fillColor >> 24) & 0xFF;
                uint r = (fillColor >> 16) & 0xFF;
                uint g = (fillColor >> 8) & 0xFF;
                uint b = fillColor & 0xFF;

                if (fpdf_edit.FPDFPageObjSetFillColor(obj, r, g, b, a) == 0)
                {
                    throw new PdfException("Failed to set fill color.");
                }
            }

            // Insert object into page
            fpdf_edit.FPDFPageInsertObject(_page._handle!, obj);

            return obj;
        }
        catch
        {
            fpdf_edit.FPDFPageObjDestroy(obj);
            throw;
        }
    }

    /// <summary>
    /// Removes page objects at the specified indices.
    /// Indices are processed in descending order to maintain correct mapping during removal.
    /// </summary>
    /// <param name="indices">The zero-based indices of the objects to remove.</param>
    /// <exception cref="ArgumentNullException">indices is null.</exception>
    /// <exception cref="ObjectDisposedException">Editor has been disposed.</exception>
    /// <exception cref="PdfException">Failed to remove one or more objects.</exception>
    public void RemoveObjects(params int[] indices)
    {
        if (indices is null)
            throw new ArgumentNullException(nameof(indices));

        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        // Sort indices in descending order to avoid index shifts during removal
        var sortedIndices = indices.Distinct().OrderByDescending(i => i).ToArray();

        foreach (var index in sortedIndices)
        {
            if (index < 0)
                continue;

            var objectCount = _page.PageObjectCount;
            if (index >= objectCount)
                continue;

            // Get the page object at the specified index
            var pageObject = fpdf_edit.FPDFPageGetObject(_page._handle!, index);
            if (pageObject is null || pageObject.__Instance == IntPtr.Zero)
            {
                throw new PdfException($"Failed to get page object at index {index}.");
            }

            // Remove the object from the page
            var result = fpdf_edit.FPDFPageRemoveObject(_page._handle!, pageObject);
            if (result == 0)
            {
                throw new PdfException($"Failed to remove object at index {index}.");
            }
        }
    }

    /// <summary>
    /// Regenerates the page content stream to persist all changes.
    /// This must be called after all editing operations before saving the document.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Editor has been disposed.</exception>
    /// <exception cref="PdfException">Failed to generate content.</exception>
    public void GenerateContent()
    {
        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        var result = fpdf_edit.FPDFPageGenerateContent(_page._handle!);
        if (result == 0)
        {
            throw new PdfException("Failed to generate page content.");
        }
    }

    /// <summary>
    /// Adds text to the page (fluent API).
    /// </summary>
    /// <param name="text">The text to add.</param>
    /// <param name="x">X coordinate in page units.</param>
    /// <param name="y">Y coordinate in page units.</param>
    /// <param name="font">The font to use. Omit to use default font.</param>
    /// <param name="fontSize">Font size in points. Omit to use default font size.</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Font is not specified and no default font is set.</exception>
    public PdfContentEditor Text(string text, double x, double y, PdfFont? font = null, double fontSize = 0)
    {
        var targetFont = font ?? _defaultFont ?? throw new InvalidOperationException("Font is not specified and no default font is set. Call WithFont(font) first.");
        var targetSize = fontSize > 0 ? fontSize : _defaultFontSize;

        AddText(text, x, y, targetFont, targetSize);
        return this;
    }

    public PdfContentEditor Image(byte[] imageData, int width, int height, PdfRectangle bounds)
    {
        AddImage(imageData, width, height, bounds);
        return this;
    }

    /// <summary>
    /// Adds a rectangle to the page (fluent API).
    /// </summary>
    /// <param name="bounds">Rectangle position and size.</param>
    /// <param name="strokeColor">Stroke color in ARGB format. Use 0 or omit to use default.</param>
    /// <param name="fillColor">Fill color in ARGB format. Use 0 or omit to use default.</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor Rectangle(PdfRectangle bounds, uint strokeColor = 0, uint fillColor = 0)
    {
        var stroke = strokeColor == 0 ? _defaultStrokeColor : strokeColor;
        var fill = fillColor == 0 ? _defaultFillColor : fillColor;
        AddRectangle(bounds, stroke, fill);
        return this;
    }

    /// <summary>
    /// Adds a line between two points (fluent API).
    /// </summary>
    /// <param name="x1">Start X coordinate.</param>
    /// <param name="y1">Start Y coordinate.</param>
    /// <param name="x2">End X coordinate.</param>
    /// <param name="y2">End Y coordinate.</param>
    /// <param name="strokeColor">Line color in ARGB format. Use 0 or omit to use default stroke color.</param>
    /// <param name="lineWidth">Line width. Use 0 or omit to use default line width.</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor Line(double x1, double y1, double x2, double y2, uint strokeColor = 0, double lineWidth = 0)
    {
        var color = strokeColor == 0 ? _defaultStrokeColor : strokeColor;
        var width = lineWidth == 0 ? _defaultLineWidth : lineWidth;

        AddLine(x1, y1, x2, y2, color, width);
        return this;
    }

    /// <summary>
    /// Adds a circle (fluent API).
    /// </summary>
    /// <param name="centerX">Center X coordinate.</param>
    /// <param name="centerY">Center Y coordinate.</param>
    /// <param name="radius">Circle radius.</param>
    /// <param name="strokeColor">Stroke color in ARGB format. Use 0 for no stroke or omit to use default.</param>
    /// <param name="fillColor">Fill color in ARGB format. Use 0 for no fill or omit to use default.</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor Circle(double centerX, double centerY, double radius, uint strokeColor = 0, uint fillColor = 0)
    {
        var stroke = strokeColor == 0 ? _defaultStrokeColor : strokeColor;
        var fill = fillColor == 0 ? _defaultFillColor : fillColor;

        AddCircle(centerX, centerY, radius, stroke, fill);
        return this;
    }

    /// <summary>
    /// Adds an ellipse (fluent API).
    /// </summary>
    /// <param name="bounds">Bounding rectangle for the ellipse.</param>
    /// <param name="strokeColor">Stroke color in ARGB format. Use 0 for no stroke or omit to use default.</param>
    /// <param name="fillColor">Fill color in ARGB format. Use 0 for no fill or omit to use default.</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor Ellipse(PdfRectangle bounds, uint strokeColor = 0, uint fillColor = 0)
    {
        var stroke = strokeColor == 0 ? _defaultStrokeColor : strokeColor;
        var fill = fillColor == 0 ? _defaultFillColor : fillColor;

        AddEllipse(bounds, stroke, fill);
        return this;
    }

    /// <summary>
    /// Removes page objects at the specified indices (fluent API).
    /// </summary>
    /// <param name="indices">The zero-based indices of the objects to remove.</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor Remove(params int[] indices)
    {
        RemoveObjects(indices);
        return this;
    }

    #region Fluent Configuration Methods

    /// <summary>
    /// Sets the default font for subsequent text operations (fluent API).
    /// </summary>
    /// <param name="font">The font to use as default.</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor WithFont(PdfFont font)
    {
        _defaultFont = font ?? throw new ArgumentNullException(nameof(font));
        return this;
    }

    /// <summary>
    /// Sets the default font size for subsequent text operations (fluent API).
    /// </summary>
    /// <param name="fontSize">Font size in points.</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor WithFontSize(double fontSize)
    {
        if (fontSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be positive.");

        _defaultFontSize = fontSize;
        return this;
    }

    /// <summary>
    /// Sets the default text color for subsequent text operations (fluent API).
    /// </summary>
    /// <param name="color">Color in ARGB format (0xAARRGGBB).</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor WithTextColor(uint color)
    {
        _defaultTextColor = color;
        return this;
    }

    /// <summary>
    /// Sets the default stroke color for subsequent shape operations (fluent API).
    /// </summary>
    /// <param name="color">Color in ARGB format (0xAARRGGBB).</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor WithStrokeColor(uint color)
    {
        _defaultStrokeColor = color;
        return this;
    }

    /// <summary>
    /// Sets the default fill color for subsequent shape operations (fluent API).
    /// </summary>
    /// <param name="color">Color in ARGB format (0xAARRGGBB).</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor WithFillColor(uint color)
    {
        _defaultFillColor = color;
        return this;
    }

    /// <summary>
    /// Sets the default line width for subsequent line/shape operations (fluent API).
    /// </summary>
    /// <param name="width">Line width in points.</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    public PdfContentEditor WithLineWidth(double width)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Line width cannot be negative.");

        _defaultLineWidth = width;
        return this;
    }

    #endregion

    #region Table Builder

    /// <summary>
    /// Begins building a table with fluent API.
    /// </summary>
    /// <returns>A table builder instance for configuring and rendering the table.</returns>
    /// <exception cref="ObjectDisposedException">Editor has been disposed.</exception>
    /// <example>
    /// <code>
    /// editor.BeginTable()
    ///     .Columns(cols => cols.Add(100).Add().Add(150))
    ///     .Header(header => header.Cell("Name").Cell("Age").Cell("Email"))
    ///     .Row(row => row.Cell("John").Cell("30").Cell("john@example.com"))
    ///     .Row(row => row.Cell("Jane").Cell("25").Cell("jane@example.com"))
    ///     .EndTable();
    /// </code>
    /// </example>
    public PdfTableBuilder BeginTable()
    {
        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        return new PdfTableBuilder(this);
    }

    #endregion

    public void Commit()
    {
        GenerateContent();
    }

    /// <summary>
    /// Releases all resources used by the <see cref="PdfContentEditor"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var bitmap in _bitmaps)
        {
            if (bitmap is not null && bitmap.__Instance != IntPtr.Zero)
            {
                fpdfview.FPDFBitmapDestroy(bitmap);
            }
        }
        _bitmaps.Clear();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PdfContentEditor));
    }
}
