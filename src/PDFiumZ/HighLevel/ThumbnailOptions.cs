using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Options for generating thumbnail images from PDF pages.
/// Thumbnails are smaller preview images optimized for quick viewing.
/// </summary>
public sealed class ThumbnailOptions
{
    /// <summary>
    /// Default thumbnail options: 200px max width, medium quality.
    /// </summary>
    public static readonly ThumbnailOptions Default = new();

    /// <summary>
    /// Gets or sets the maximum width of each thumbnail in pixels.
    /// The height is calculated automatically to maintain aspect ratio.
    /// Default: 200
    /// </summary>
    public int MaxWidth { get; set; } = 200;

    /// <summary>
    /// Gets or sets the rendering quality level.
    /// 0 = Low quality (fastest, skips annotations)
    /// 1 = Medium quality (balanced, default settings)
    /// 2 = High quality (slowest, includes LCD text optimization)
    /// Default: 1
    /// </summary>
    public int Quality { get; set; } = 1;

    /// <summary>
    /// Gets or sets the zero-based indices of pages to generate thumbnails for.
    /// If null, generates thumbnails for all pages.
    /// </summary>
    public int[]? PageIndices { get; set; }

    /// <summary>
    /// Sets the maximum width of each thumbnail.
    /// </summary>
    /// <param name="maxWidth">The maximum width in pixels (must be positive).</param>
    /// <returns>This <see cref="ThumbnailOptions"/> instance for method chaining.</returns>
    public ThumbnailOptions WithMaxWidth(int maxWidth)
    {
        if (maxWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxWidth), "MaxWidth must be positive.");

        MaxWidth = maxWidth;
        return this;
    }

    /// <summary>
    /// Sets the rendering quality level.
    /// </summary>
    /// <param name="quality">The quality level (0=low, 1=medium, 2=high).</param>
    /// <returns>This <see cref="ThumbnailOptions"/> instance for method chaining.</returns>
    public ThumbnailOptions WithQuality(int quality)
    {
        if (quality < 0 || quality > 2)
            throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 0 (low) and 2 (high).");

        Quality = quality;
        return this;
    }

    /// <summary>
    /// Sets low quality rendering (fastest, skips annotations).
    /// </summary>
    /// <returns>This <see cref="ThumbnailOptions"/> instance for method chaining.</returns>
    public ThumbnailOptions WithLowQuality()
    {
        Quality = 0;
        return this;
    }

    /// <summary>
    /// Sets medium quality rendering (balanced, default settings).
    /// </summary>
    /// <returns>This <see cref="ThumbnailOptions"/> instance for method chaining.</returns>
    public ThumbnailOptions WithMediumQuality()
    {
        Quality = 1;
        return this;
    }

    /// <summary>
    /// Sets high quality rendering (slowest, includes LCD text optimization).
    /// </summary>
    /// <returns>This <see cref="ThumbnailOptions"/> instance for method chaining.</returns>
    public ThumbnailOptions WithHighQuality()
    {
        Quality = 2;
        return this;
    }

    /// <summary>
    /// Sets the pages to generate thumbnails for by indices.
    /// </summary>
    /// <param name="pageIndices">The zero-based page indices.</param>
    /// <returns>This <see cref="ThumbnailOptions"/> instance for method chaining.</returns>
    public ThumbnailOptions WithPages(int[] pageIndices)
    {
        PageIndices = pageIndices ?? throw new ArgumentNullException(nameof(pageIndices));
        return this;
    }

    /// <summary>
    /// Sets to generate thumbnails for all pages.
    /// </summary>
    /// <returns>This <see cref="ThumbnailOptions"/> instance for method chaining.</returns>
    public ThumbnailOptions ForAllPages()
    {
        PageIndices = null;
        return this;
    }

    /// <summary>
    /// Validates the options and throws if invalid.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">MaxWidth or Quality is invalid.</exception>
    public void Validate()
    {
        if (MaxWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxWidth), "MaxWidth must be positive.");
        if (Quality < 0 || Quality > 2)
            throw new ArgumentOutOfRangeException(nameof(Quality), "Quality must be between 0 (low) and 2 (high).");
    }
}
