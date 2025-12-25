using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Extension methods for generating images from PdfDocument.
/// </summary>
public static class PdfDocumentImageExtensions
{
    #region New Core Methods (Recommended)

    /// <summary>
    /// Generates images from the document using the specified options.
    /// This is the recommended method for image generation as it provides a unified interface.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="options">Options specifying which pages to render and how.</param>
    /// <returns>An enumerable of PdfImage objects. Remember to dispose each image after use.</returns>
    /// <exception cref="ArgumentNullException">document or options is null.</exception>
    /// <exception cref="ObjectDisposedException">document has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Any page index is out of range.</exception>
    /// <example>
    /// <code>
    /// // Generate images for specific pages
    /// var options = ImageGenerationOptions.ForPages([0, 2, 4]);
    /// foreach (var image in document.GenerateImages(options))
    /// {
    ///     // Use image
    ///     image.Dispose();
    /// }
    ///
    /// // Generate images for a range with custom render options
    /// var rangeOptions = ImageGenerationOptions.ForRange(0, 10, RenderOptions.Default.WithDpi(150));
    /// </code>
    /// </example>
    public static IEnumerable<PdfImage> GenerateImages(
        this PdfDocument document,
        ImageGenerationOptions options)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var renderOptions = options.RenderOptions ?? RenderOptions.Default;

        // Determine which pages to render
        int[]? indices = null;
        if (options.PageIndices != null)
        {
            indices = options.PageIndices;
        }
        else
        {
            int start = options.StartIndex;
            int count = options.Count ?? (document.PageCount - start);
            indices = Enumerable.Range(start, count).ToArray();
        }

        // Validate indices
        foreach (var index in indices)
        {
            if (index < 0 || index >= document.PageCount)
                throw new ArgumentOutOfRangeException(nameof(options.PageIndices),
                    $"Page index {index} is out of range (0-{document.PageCount - 1}).");
        }

        // Generate images
        foreach (var i in indices)
        {
            using var page = document.GetPages(i, 1).First();
            yield return page.RenderToImage(renderOptions);
        }
    }

    /// <summary>
    /// Saves pages as image files using the specified options.
    /// This is the recommended method for saving images as it provides a unified interface.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="options">Options specifying which pages to save and where.</param>
    /// <returns>Array of generated file paths.</returns>
    /// <exception cref="ArgumentNullException">document or options is null.</exception>
    /// <exception cref="ObjectDisposedException">document has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Any page index is out of range.</exception>
    /// <example>
    /// <code>
    /// // Save specific pages to a directory
    /// var options = ImageSaveOptions.ForPages("output", [0, 2, 4]);
    /// var paths = document.SaveAsImages(options);
    ///
    /// // Save all pages with custom naming
    /// var customOptions = new ImageSaveOptions
    /// {
    ///     OutputDirectory = "output",
    ///     FileNamePattern = "scan-{0:D3}.png",
    ///     RenderOptions = RenderOptions.Default.WithDpi(200)
    /// };
    /// </code>
    /// </example>
    public static string[] SaveAsImages(
        this PdfDocument document,
        ImageSaveOptions options)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var renderOptions = options.RenderOptions ?? RenderOptions.Default;
        var filePaths = new List<string>();

        // Use custom path generator if provided
        if (options.PathGenerator != null)
        {
            int start = options.StartIndex;
            int count = options.Count ?? (document.PageCount - start);

            for (int i = start; i < start + count; i++)
            {
                if (i < 0 || i >= document.PageCount)
                    throw new ArgumentOutOfRangeException(nameof(options.StartIndex),
                        $"Page index {i} is out of range (0-{document.PageCount - 1}).");

                using var page = document.GetPages(i, 1).First();
                using var image = page.RenderToImage(renderOptions);

                var filePath = options.PathGenerator(i);
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                image.SaveAsPng(filePath);
                filePaths.Add(filePath);
            }
        }
        else
        {
            // Use output directory
            if (string.IsNullOrWhiteSpace(options.OutputDirectory))
                throw new ArgumentException("OutputDirectory must be specified when PathGenerator is not set.", nameof(options));

            Directory.CreateDirectory(options.OutputDirectory);

            // Determine which pages to save
            int[]? indices = null;
            if (options.PageIndices != null)
            {
                indices = options.PageIndices;
            }
            else
            {
                int start = options.StartIndex;
                int count = options.Count ?? (document.PageCount - start);
                indices = Enumerable.Range(start, count).ToArray();
            }

            // Validate and save
            foreach (var i in indices)
            {
                if (i < 0 || i >= document.PageCount)
                    throw new ArgumentOutOfRangeException(nameof(options.PageIndices),
                        $"Page index {i} is out of range (0-{document.PageCount - 1}).");

                using var page = document.GetPages(i, 1).First();
                using var image = page.RenderToImage(renderOptions);

                var fileName = string.Format(options.FileNamePattern, i);
                var filePath = Path.Combine(options.OutputDirectory, fileName);

                image.SaveAsPng(filePath);
                filePaths.Add(filePath);
            }
        }

        return filePaths.ToArray();
    }

    #endregion
}
