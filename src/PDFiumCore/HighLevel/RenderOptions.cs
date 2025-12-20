using System;

namespace PDFiumCore.HighLevel;

/// <summary>
/// Fluent builder for configuring PDF page rendering.
/// Immutable - each method returns a new instance.
/// </summary>
public sealed record RenderOptions
{
    /// <summary>
    /// Default rendering options: 1x scale, white background, annotations enabled.
    /// </summary>
    public static readonly RenderOptions Default = new();

    /// <summary>
    /// Gets the scale factor (default: 1.0).
    /// </summary>
    public double Scale { get; init; } = 1.0;

    /// <summary>
    /// Gets the viewport rectangle (null = entire page).
    /// </summary>
    public PdfRectangle? Viewport { get; init; }

    /// <summary>
    /// Gets the background color (ARGB format, default: white).
    /// </summary>
    public uint BackgroundColor { get; init; } = 0xFFFFFFFF;

    /// <summary>
    /// Gets whether to render with transparent background.
    /// </summary>
    public bool HasTransparency { get; init; } = false;

    /// <summary>
    /// Gets the rendering flags.
    /// </summary>
    public RenderFlags Flags { get; init; } = RenderFlags.RenderAnnotations;

    /// <summary>
    /// Gets the DPI for rendering (default: 72 - PDF native).
    /// </summary>
    public int Dpi { get; init; } = 72;

    /// <summary>
    /// Sets the scale factor.
    /// </summary>
    /// <param name="scale">The scale factor (must be positive).</param>
    /// <returns>A new <see cref="RenderOptions"/> instance with the specified scale.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Scale must be positive.</exception>
    public RenderOptions WithScale(double scale)
    {
        if (scale <= 0)
            throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be positive.");
        return this with { Scale = scale };
    }

    /// <summary>
    /// Sets the DPI and calculates corresponding scale.
    /// </summary>
    /// <param name="dpi">The DPI value (must be positive).</param>
    /// <returns>A new <see cref="RenderOptions"/> instance with the specified DPI.</returns>
    /// <exception cref="ArgumentOutOfRangeException">DPI must be positive.</exception>
    public RenderOptions WithDpi(int dpi)
    {
        if (dpi <= 0)
            throw new ArgumentOutOfRangeException(nameof(dpi), "DPI must be positive.");
        return this with { Dpi = dpi, Scale = dpi / 72.0 };
    }

    /// <summary>
    /// Sets a viewport to render only a portion of the page.
    /// </summary>
    /// <param name="x">The X coordinate of the viewport.</param>
    /// <param name="y">The Y coordinate of the viewport.</param>
    /// <param name="width">The width of the viewport.</param>
    /// <param name="height">The height of the viewport.</param>
    /// <returns>A new <see cref="RenderOptions"/> instance with the specified viewport.</returns>
    public RenderOptions WithViewport(double x, double y, double width, double height)
    {
        return this with { Viewport = new PdfRectangle(x, y, width, height) };
    }

    /// <summary>
    /// Sets the viewport.
    /// </summary>
    /// <param name="viewport">The viewport rectangle.</param>
    /// <returns>A new <see cref="RenderOptions"/> instance with the specified viewport.</returns>
    public RenderOptions WithViewport(PdfRectangle viewport)
    {
        return this with { Viewport = viewport };
    }

    /// <summary>
    /// Enables transparent background (BGRA format).
    /// </summary>
    /// <param name="enabled">Whether to enable transparency.</param>
    /// <returns>A new <see cref="RenderOptions"/> instance with transparency setting.</returns>
    public RenderOptions WithTransparency(bool enabled = true)
    {
        return this with { HasTransparency = enabled };
    }

    /// <summary>
    /// Sets the background color (ARGB format).
    /// </summary>
    /// <param name="color">The background color in ARGB format.</param>
    /// <returns>A new <see cref="RenderOptions"/> instance with the specified background color.</returns>
    public RenderOptions WithBackgroundColor(uint color)
    {
        return this with { BackgroundColor = color, HasTransparency = false };
    }

    /// <summary>
    /// Sets rendering flags.
    /// </summary>
    /// <param name="flags">The rendering flags.</param>
    /// <returns>A new <see cref="RenderOptions"/> instance with the specified flags.</returns>
    public RenderOptions WithFlags(RenderFlags flags)
    {
        return this with { Flags = flags };
    }

    /// <summary>
    /// Adds rendering flags to existing flags.
    /// </summary>
    /// <param name="flags">The flags to add.</param>
    /// <returns>A new <see cref="RenderOptions"/> instance with the flags added.</returns>
    public RenderOptions AddFlags(RenderFlags flags)
    {
        return this with { Flags = Flags | flags };
    }

    /// <summary>
    /// Removes rendering flags from existing flags.
    /// </summary>
    /// <param name="flags">The flags to remove.</param>
    /// <returns>A new <see cref="RenderOptions"/> instance with the flags removed.</returns>
    public RenderOptions RemoveFlags(RenderFlags flags)
    {
        return this with { Flags = Flags & ~flags };
    }

    /// <summary>
    /// Calculates final bitmap dimensions based on page size and options.
    /// </summary>
    internal (double Width, double Height) CalculateDimensions(double pageWidth, double pageHeight)
    {
        var viewport = Viewport ?? new PdfRectangle(0, 0, pageWidth, pageHeight);
        return (viewport.Width * Scale, viewport.Height * Scale);
    }

    /// <summary>
    /// Creates the transformation matrix for rendering.
    /// </summary>
    internal FS_MATRIX_ CreateMatrix(double pageWidth, double pageHeight)
    {
        var viewport = Viewport ?? new PdfRectangle(0, 0, pageWidth, pageHeight);

        var matrix = new FS_MATRIX_
        {
            A = (float)Scale,
            B = 0,
            C = 0,
            D = (float)Scale,
            E = (float)(-viewport.X * Scale),
            F = (float)(-viewport.Y * Scale)
        };

        return matrix;
    }

    /// <summary>
    /// Creates the clipping rectangle for rendering.
    /// </summary>
    internal FS_RECTF_ CreateClipping(double width, double height)
    {
        return new FS_RECTF_
        {
            Left = 0,
            Top = (float)height,
            Right = (float)width,
            Bottom = 0
        };
    }
}
