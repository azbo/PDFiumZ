using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Provides methods for editing PDF page content by adding text, images, and shapes.
/// Must be disposed properly to free native resources.
/// </summary>
public sealed unsafe class PdfContentEditor : IDisposable
{
    private readonly PdfPage _page;
    private bool _disposed;

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
            // Create bitmap from data (FPDFBitmap format: BGRA, 4 bytes per pixel)
            fixed (byte* pData = imageData)
            {
                // Format 4 = BGRA
                bitmap = fpdfview.FPDFBitmapCreateEx(width, height, 4, (IntPtr)pData, width * 4);
                if (bitmap is null || bitmap.__Instance == IntPtr.Zero)
                {
                    throw new PdfException("Failed to create bitmap from image data.");
                }

                // Set bitmap to image object
                // Pass the current page handle in the pages array
                var result = fpdf_edit.FPDFImageObjSetBitmap(_page._handle!, 1, imageObj, bitmap);
                if (result == 0)
                {
                    throw new PdfException("Failed to set bitmap to image object.");
                }
            }

            // Calculate transformation matrix to position and scale image
            // Scale image from pixel size to page bounds size
            double scaleX = bounds.Width / width;
            double scaleY = bounds.Height / height;

            // Matrix: [a b c d e f]
            // a = scaleX, d = scaleY (scale)
            // e = bounds.X, f = bounds.Y (translation)
            fpdf_edit.FPDFPageObjTransform(imageObj, scaleX, 0, 0, scaleY, bounds.X, bounds.Y);

            // Insert object into page
            fpdf_edit.FPDFPageInsertObject(_page._handle!, imageObj);

            return imageObj;
        }
        catch
        {
            // Clean up on failure
            fpdf_edit.FPDFPageObjDestroy(imageObj);
            throw;
        }
        finally
        {
            // Destroy bitmap (no longer needed after SetBitmap)
            if (bitmap is not null && bitmap.__Instance != IntPtr.Zero)
            {
                fpdfview.FPDFBitmapDestroy(bitmap);
            }
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

        try
        {
            // Set stroke color if specified
            if (strokeColor != 0)
            {
                uint a = (strokeColor >> 24) & 0xFF;
                uint r = (strokeColor >> 16) & 0xFF;
                uint g = (strokeColor >> 8) & 0xFF;
                uint b = strokeColor & 0xFF;

                var result = fpdf_edit.FPDFPageObjSetStrokeColor(rectObj, r, g, b, a);
                if (result == 0)
                {
                    throw new PdfException("Failed to set rectangle stroke color.");
                }
            }

            // Set fill color if specified
            if (fillColor != 0)
            {
                uint a = (fillColor >> 24) & 0xFF;
                uint r = (fillColor >> 16) & 0xFF;
                uint g = (fillColor >> 8) & 0xFF;
                uint b = fillColor & 0xFF;

                var result = fpdf_edit.FPDFPageObjSetFillColor(rectObj, r, g, b, a);
                if (result == 0)
                {
                    throw new PdfException("Failed to set rectangle fill color.");
                }
            }

            // Insert object into page
            fpdf_edit.FPDFPageInsertObject(_page._handle!, rectObj);

            return rectObj;
        }
        catch
        {
            // Clean up on failure
            fpdf_edit.FPDFPageObjDestroy(rectObj);
            throw;
        }
    }

    /// <summary>
    /// Removes a page object at the specified index.
    /// </summary>
    /// <param name="index">Zero-based index of the object to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">Editor has been disposed.</exception>
    /// <exception cref="PdfException">Failed to remove object.</exception>
    public void RemoveObject(int index)
    {
        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        var objectCount = _page.GetPageObjectCount();
        if (index < 0 || index >= objectCount)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Object index {index} is out of range (0-{objectCount - 1}).");

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

    public PdfContentEditor Text(string text, double x, double y, PdfFont font, double fontSize)
    {
        AddText(text, x, y, font, fontSize);
        return this;
    }

    /// <summary>
    /// Adds text to the page using current default font and size (fluent API).
    /// </summary>
    /// <param name="text">The text to add.</param>
    /// <param name="x">X coordinate in page units.</param>
    /// <param name="y">Y coordinate in page units.</param>
    /// <returns>This editor instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Default font is not set. Call WithFont first.</exception>
    public PdfContentEditor Text(string text, double x, double y)
    {
        if (_defaultFont == null)
            throw new InvalidOperationException("Default font is not set. Call WithFont(font) first.");

        AddText(text, x, y, _defaultFont, _defaultFontSize);
        return this;
    }

    public PdfContentEditor Image(byte[] imageData, int width, int height, PdfRectangle bounds)
    {
        AddImage(imageData, width, height, bounds);
        return this;
    }

    public PdfContentEditor Rectangle(PdfRectangle bounds, uint strokeColor = 0, uint fillColor = 0)
    {
        AddRectangle(bounds, strokeColor, fillColor);
        return this;
    }

    /// <summary>
    /// Adds a rectangle using current default stroke and fill colors (fluent API).
    /// </summary>
    public PdfContentEditor Rectangle(PdfRectangle bounds)
    {
        AddRectangle(bounds, _defaultStrokeColor, _defaultFillColor);
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
        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        var color = strokeColor == 0 ? _defaultStrokeColor : strokeColor;
        var width = lineWidth == 0 ? _defaultLineWidth : lineWidth;

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

            // Set stroke color
            uint a = (color >> 24) & 0xFF;
            uint r = (color >> 16) & 0xFF;
            uint g = (color >> 8) & 0xFF;
            uint b = color & 0xFF;

            var result = fpdf_edit.FPDFPageObjSetStrokeColor(pathObj, r, g, b, a);
            if (result == 0)
            {
                throw new PdfException("Failed to set line stroke color.");
            }

            // Set stroke width
            fpdf_edit.FPDFPageObjSetStrokeWidth(pathObj, (float)width);

            // Set draw mode to stroke only
            fpdf_edit.FPDFPathSetDrawMode(pathObj, 1, 0); // stroke = 1, fill = 0

            // Insert object into page
            fpdf_edit.FPDFPageInsertObject(_page._handle!, pathObj);
        }
        catch
        {
            fpdf_edit.FPDFPageObjDestroy(pathObj);
            throw;
        }

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
        var bounds = new PdfRectangle(
            centerX - radius,
            centerY - radius,
            radius * 2,
            radius * 2);

        var stroke = strokeColor == 0 ? _defaultStrokeColor : strokeColor;
        var fill = fillColor == 0 ? _defaultFillColor : fillColor;

        return Ellipse(bounds, stroke, fill);
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
        ThrowIfDisposed();
        _page.ThrowIfDisposed();

        var stroke = strokeColor == 0 ? _defaultStrokeColor : strokeColor;
        var fill = fillColor == 0 ? _defaultFillColor : fillColor;

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

            // Top
            fpdf_edit.FPDFPathBezierTo(pathObj,
                (float)(cx + ox), (float)(cy + ry),
                (float)(cx + rx), (float)(cy + oy),
                (float)(cx + rx), (float)cy);

            // Right
            fpdf_edit.FPDFPathBezierTo(pathObj,
                (float)(cx + rx), (float)(cy - oy),
                (float)(cx + ox), (float)(cy - ry),
                (float)cx, (float)(cy - ry));

            // Bottom
            fpdf_edit.FPDFPathBezierTo(pathObj,
                (float)(cx - ox), (float)(cy - ry),
                (float)(cx - rx), (float)(cy - oy),
                (float)(cx - rx), (float)cy);

            // Left
            fpdf_edit.FPDFPathBezierTo(pathObj,
                (float)(cx - rx), (float)(cy + oy),
                (float)(cx - ox), (float)(cy + ry),
                (float)cx, (float)(cy + ry));

            fpdf_edit.FPDFPathClose(pathObj);

            // Set stroke color if specified
            if (stroke != 0)
            {
                uint a = (stroke >> 24) & 0xFF;
                uint r = (stroke >> 16) & 0xFF;
                uint g = (stroke >> 8) & 0xFF;
                uint b = stroke & 0xFF;

                var result = fpdf_edit.FPDFPageObjSetStrokeColor(pathObj, r, g, b, a);
                if (result == 0)
                {
                    throw new PdfException("Failed to set ellipse stroke color.");
                }
            }

            // Set fill color if specified
            if (fill != 0)
            {
                uint a = (fill >> 24) & 0xFF;
                uint r = (fill >> 16) & 0xFF;
                uint g = (fill >> 8) & 0xFF;
                uint b = fill & 0xFF;

                var result = fpdf_edit.FPDFPageObjSetFillColor(pathObj, r, g, b, a);
                if (result == 0)
                {
                    throw new PdfException("Failed to set ellipse fill color.");
                }
            }

            // Set draw mode
            var hasStroke = stroke != 0 ? 1 : 0;
            var hasFill = fill != 0 ? 1 : 0;
            fpdf_edit.FPDFPathSetDrawMode(pathObj, hasStroke, hasFill);

            // Insert object into page
            fpdf_edit.FPDFPageInsertObject(_page._handle!, pathObj);
        }
        catch
        {
            fpdf_edit.FPDFPageObjDestroy(pathObj);
            throw;
        }

        return this;
    }

    public PdfContentEditor Remove(int index)
    {
        RemoveObject(index);
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
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PdfContentEditor));
    }
}
