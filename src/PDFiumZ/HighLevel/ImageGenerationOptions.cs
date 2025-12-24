using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Options for generating images from PDF pages.
/// Provides a unified way to specify which pages to render and how.
/// </summary>
public sealed class ImageGenerationOptions
{
    /// <summary>
    /// Default options: render all pages with default render settings.
    /// </summary>
    public static readonly ImageGenerationOptions Default = new();

    /// <summary>
    /// Gets or sets the zero-based indices of pages to render.
    /// If specified, <see cref="StartIndex"/> and <see cref="Count"/> are ignored.
    /// </summary>
    public int[]? PageIndices { get; set; }

    /// <summary>
    /// Gets or sets the zero-based index of the first page to render.
    /// Used only when <see cref="PageIndices"/> is null.
    /// Default: 0
    /// </summary>
    public int StartIndex { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of pages to render.
    /// Used only when <see cref="PageIndices"/> is null.
    /// Default: null (render all remaining pages from <see cref="StartIndex"/>)
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// Gets or sets the render options for controlling image quality and appearance.
    /// If null, uses <see cref="RenderOptions.Default"/>.
    /// </summary>
    public RenderOptions? RenderOptions { get; set; }

    /// <summary>
    /// Sets the pages to render by indices.
    /// </summary>
    /// <param name="pageIndices">The zero-based page indices to render.</param>
    /// <returns>This <see cref="ImageGenerationOptions"/> instance for method chaining.</returns>
    public ImageGenerationOptions WithPages(int[] pageIndices)
    {
        PageIndices = pageIndices ?? throw new ArgumentNullException(nameof(pageIndices));
        return this;
    }

    /// <summary>
    /// Sets the start index and count for rendering a range of pages.
    /// </summary>
    /// <param name="startIndex">The zero-based index of the first page.</param>
    /// <param name="count">The number of pages to render.</param>
    /// <returns>This <see cref="ImageGenerationOptions"/> instance for method chaining.</returns>
    public ImageGenerationOptions WithRange(int startIndex, int count)
    {
        StartIndex = startIndex;
        Count = count;
        PageIndices = null;
        return this;
    }

    /// <summary>
    /// Sets the render options for controlling image quality and appearance.
    /// </summary>
    /// <param name="renderOptions">The render options.</param>
    /// <returns>This <see cref="ImageGenerationOptions"/> instance for method chaining.</returns>
    public ImageGenerationOptions WithRenderOptions(RenderOptions renderOptions)
    {
        RenderOptions = renderOptions ?? throw new ArgumentNullException(nameof(renderOptions));
        return this;
    }

    /// <summary>
    /// Sets the DPI (dots per inch) for rendering, which automatically calculates the scale factor.
    /// </summary>
    /// <param name="dpi">The DPI value (must be positive).</param>
    /// <returns>This <see cref="ImageGenerationOptions"/> instance for method chaining.</returns>
    public ImageGenerationOptions WithDpi(int dpi)
    {
        if (dpi <= 0)
            throw new ArgumentOutOfRangeException(nameof(dpi), "DPI must be positive.");

        RenderOptions = (RenderOptions ?? RenderOptions.Default).WithDpi(dpi);
        return this;
    }

    /// <summary>
    /// Sets the scale factor for rendering.
    /// </summary>
    /// <param name="scale">The scale factor (must be positive).</param>
    /// <returns>This <see cref="ImageGenerationOptions"/> instance for method chaining.</returns>
    public ImageGenerationOptions WithScale(double scale)
    {
        if (scale <= 0)
            throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be positive.");

        RenderOptions = (RenderOptions ?? RenderOptions.Default).WithScale(scale);
        return this;
    }

    /// <summary>
    /// Enables transparent background rendering.
    /// </summary>
    /// <param name="enabled">Whether to enable transparency (default: true).</param>
    /// <returns>This <see cref="ImageGenerationOptions"/> instance for method chaining.</returns>
    public ImageGenerationOptions WithTransparency(bool enabled = true)
    {
        RenderOptions = (RenderOptions ?? RenderOptions.Default).WithTransparency(enabled);
        return this;
    }

    /// <summary>
    /// Sets the background color for rendering (disables transparency).
    /// </summary>
    /// <param name="color">The background color in ARGB format.</param>
    /// <returns>This <see cref="ImageGenerationOptions"/> instance for method chaining.</returns>
    public ImageGenerationOptions WithBackgroundColor(uint color)
    {
        RenderOptions = (RenderOptions ?? RenderOptions.Default).WithBackgroundColor(color);
        return this;
    }

    /// <summary>
    /// Creates options for rendering specific pages by indices.
    /// </summary>
    /// <param name="pageIndices">The zero-based page indices to render.</param>
    /// <param name="renderOptions">Optional render options.</param>
    /// <returns>A new <see cref="ImageGenerationOptions"/> instance.</returns>
    public static ImageGenerationOptions ForPages(int[] pageIndices, RenderOptions? renderOptions = null)
    {
        return new ImageGenerationOptions
        {
            PageIndices = pageIndices ?? throw new ArgumentNullException(nameof(pageIndices)),
            RenderOptions = renderOptions
        };
    }

    /// <summary>
    /// Creates options for rendering a range of pages.
    /// </summary>
    /// <param name="startIndex">The zero-based index of the first page.</param>
    /// <param name="count">The number of pages to render.</param>
    /// <param name="renderOptions">Optional render options.</param>
    /// <returns>A new <see cref="ImageGenerationOptions"/> instance.</returns>
    public static ImageGenerationOptions ForRange(int startIndex, int count, RenderOptions? renderOptions = null)
    {
        return new ImageGenerationOptions
        {
            StartIndex = startIndex,
            Count = count,
            RenderOptions = renderOptions
        };
    }

    /// <summary>
    /// Creates options for rendering all pages.
    /// </summary>
    /// <param name="renderOptions">Optional render options.</param>
    /// <returns>A new <see cref="ImageGenerationOptions"/> instance.</returns>
    public static ImageGenerationOptions ForAllPages(RenderOptions? renderOptions = null)
    {
        return new ImageGenerationOptions
        {
            RenderOptions = renderOptions
        };
    }

#if NET8_0_OR_GREATER || NET9_0_OR_GREATER || NET10_0_OR_GREATER
    /// <summary>
    /// Creates options for rendering pages specified by a <see cref="Range"/> expression.
    /// </summary>
    /// <param name="range">The range of pages to render.</param>
    /// <param name="pageCount">Total number of pages in the document (for calculating the range).</param>
    /// <param name="renderOptions">Optional render options.</param>
    /// <returns>A new <see cref="ImageGenerationOptions"/> instance.</returns>
    /// <example>
    /// <code>
    /// // Render first 10 pages
    /// var options = ImageGenerationOptions.ForRange(..10, document.PageCount);
    ///
    /// // Render pages 5-15
    /// var options = ImageGenerationOptions.ForRange(5..15, document.PageCount);
    ///
    /// // Render last 5 pages
    /// var options = ImageGenerationOptions.ForRange(^5.., document.PageCount);
    ///
    /// // Render all pages except first 5
    /// var options = ImageGenerationOptions.ForRange(5.., document.PageCount);
    /// </code>
    /// </example>
    public static ImageGenerationOptions ForRange(Range range, int pageCount, RenderOptions? renderOptions = null)
    {
        var (offset, length) = range.GetOffsetAndLength(pageCount);
        return new ImageGenerationOptions
        {
            StartIndex = offset,
            Count = length,
            RenderOptions = renderOptions
        };
    }
#endif
}
