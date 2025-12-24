using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Options for saving PDF pages as image files.
/// Provides a unified way to specify which pages to save, output location, and file naming.
/// </summary>
public sealed class ImageSaveOptions
{
    /// <summary>
    /// Default options: save all pages to directory with default naming.
    /// </summary>
    public static readonly ImageSaveOptions Default = new();

    /// <summary>
    /// Gets or sets the zero-based indices of pages to save.
    /// If specified, <see cref="StartIndex"/> and <see cref="Count"/> are ignored.
    /// </summary>
    public int[]? PageIndices { get; set; }

    /// <summary>
    /// Gets or sets the zero-based index of the first page to save.
    /// Used only when <see cref="PageIndices"/> is null.
    /// Default: 0
    /// </summary>
    public int StartIndex { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of pages to save.
    /// Used only when <see cref="PageIndices"/> is null.
    /// Default: null (save all remaining pages from <see cref="StartIndex"/>)
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// Gets or sets the output directory path where images will be saved.
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Gets or sets the custom path generator function for each page.
    /// If set, <see cref="OutputDirectory"/> and <see cref="FileNamePattern"/> are ignored.
    /// </summary>
    public Func<int, string>? PathGenerator { get; set; }

    /// <summary>
    /// Gets or sets the file name pattern.
    /// Use {0} as placeholder for page index.
    /// Default: "page-{0}.png"
    /// </summary>
    public string FileNamePattern { get; set; } = "page-{0}.png";

    /// <summary>
    /// Gets or sets the render options for controlling image quality and appearance.
    /// If null, uses <see cref="RenderOptions.Default"/>.
    /// </summary>
    public RenderOptions? RenderOptions { get; set; }

    /// <summary>
    /// Creates options for saving specific pages by indices to a directory.
    /// </summary>
    /// <param name="outputDirectory">The output directory path.</param>
    /// <param name="pageIndices">The zero-based page indices to save.</param>
    /// <param name="renderOptions">Optional render options.</param>
    /// <returns>A new <see cref="ImageSaveOptions"/> instance.</returns>
    public static ImageSaveOptions ForPages(string outputDirectory, int[] pageIndices, RenderOptions? renderOptions = null)
    {
        return new ImageSaveOptions
        {
            OutputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory)),
            PageIndices = pageIndices ?? throw new ArgumentNullException(nameof(pageIndices)),
            RenderOptions = renderOptions
        };
    }

    /// <summary>
    /// Creates options for saving a range of pages to a directory.
    /// </summary>
    /// <param name="outputDirectory">The output directory path.</param>
    /// <param name="startIndex">The zero-based index of the first page.</param>
    /// <param name="count">The number of pages to save.</param>
    /// <param name="renderOptions">Optional render options.</param>
    /// <returns>A new <see cref="ImageSaveOptions"/> instance.</returns>
    public static ImageSaveOptions ForRange(string outputDirectory, int startIndex, int count, RenderOptions? renderOptions = null)
    {
        return new ImageSaveOptions
        {
            OutputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory)),
            StartIndex = startIndex,
            Count = count,
            RenderOptions = renderOptions
        };
    }

    /// <summary>
    /// Creates options for saving all pages to a directory.
    /// </summary>
    /// <param name="outputDirectory">The output directory path.</param>
    /// <param name="renderOptions">Optional render options.</param>
    /// <returns>A new <see cref="ImageSaveOptions"/> instance.</returns>
    public static ImageSaveOptions ForAllPages(string outputDirectory, RenderOptions? renderOptions = null)
    {
        return new ImageSaveOptions
        {
            OutputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory)),
            RenderOptions = renderOptions
        };
    }

    /// <summary>
    /// Creates options for saving pages using a custom path generator.
    /// </summary>
    /// <param name="pathGenerator">Function that generates a file path for a given page index.</param>
    /// <param name="startIndex">Optional start index. Default: 0</param>
    /// <param name="count">Optional number of pages. Default: all remaining pages.</param>
    /// <param name="renderOptions">Optional render options.</param>
    /// <returns>A new <see cref="ImageSaveOptions"/> instance.</returns>
    public static ImageSaveOptions WithPathGenerator(Func<int, string> pathGenerator, int? startIndex = null, int? count = null, RenderOptions? renderOptions = null)
    {
        return new ImageSaveOptions
        {
            PathGenerator = pathGenerator ?? throw new ArgumentNullException(nameof(pathGenerator)),
            StartIndex = startIndex ?? 0,
            Count = count,
            RenderOptions = renderOptions
        };
    }
}
