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
