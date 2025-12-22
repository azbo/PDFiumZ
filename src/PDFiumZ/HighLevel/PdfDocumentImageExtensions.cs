using System;
using System.Collections.Generic;
using System.IO;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Extension methods for generating images from PDF documents.
/// </summary>
public static class PdfDocumentImageExtensions
{
    /// <summary>
    /// Generates images from all pages in the document.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="options">Optional render options. If null, uses default settings.</param>
    /// <returns>An enumerable of PdfImage objects. Remember to dispose each image after use.</returns>
    /// <exception cref="ArgumentNullException">document is null.</exception>
    /// <exception cref="ObjectDisposedException">document has been disposed.</exception>
    /// <example>
    /// <code>
    /// using var document = PdfDocument.Open("sample.pdf");
    /// foreach (var image in document.GenerateImages())
    /// {
    ///     using (image)
    ///     {
    ///         // Process image...
    ///         image.SaveAsSkiaPng($"page-{i}.png");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IEnumerable<PdfImage> GenerateImages(this PdfDocument document, RenderOptions? options = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        options ??= RenderOptions.Default;

        for (int i = 0; i < document.PageCount; i++)
        {
            using var page = document.GetPage(i);
            yield return page.RenderToImage(options);
        }
    }

    /// <summary>
    /// Generates images from a range of pages.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="startIndex">The zero-based starting page index.</param>
    /// <param name="count">Number of pages to render.</param>
    /// <param name="options">Optional render options.</param>
    /// <returns>An enumerable of PdfImage objects.</returns>
    public static IEnumerable<PdfImage> GenerateImages(
        this PdfDocument document,
        int startIndex,
        int count,
        RenderOptions? options = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (startIndex < 0 || startIndex >= document.PageCount)
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (count < 0 || startIndex + count > document.PageCount)
            throw new ArgumentOutOfRangeException(nameof(count));

        options ??= RenderOptions.Default;

        for (int i = startIndex; i < startIndex + count; i++)
        {
            using var page = document.GetPage(i);
            yield return page.RenderToImage(options);
        }
    }

    /// <summary>
    /// Saves all pages as image files to a directory with a specified format.
    /// File names are automatically generated as "page-0.png", "page-1.png", etc.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="outputDirectory">Directory path where images will be saved.</param>
    /// <param name="fileNamePattern">Optional file name pattern. Use {0} for page index. Default: "page-{0}.png"</param>
    /// <param name="options">Optional render options.</param>
    /// <returns>Array of generated file paths.</returns>
    /// <exception cref="ArgumentNullException">document or outputDirectory is null.</exception>
    /// <exception cref="ObjectDisposedException">document has been disposed.</exception>
    /// <example>
    /// <code>
    /// // Save to output directory with default naming
    /// document.SaveAsImages("output/");
    ///
    /// // Save with custom naming pattern
    /// document.SaveAsImages("output/", "document-page-{0}.png");
    ///
    /// // Save with custom options
    /// var options = RenderOptions.Default.WithDpi(150);
    /// document.SaveAsImages("output/", options: options);
    /// </code>
    /// </example>
    public static string[] SaveAsImages(
        this PdfDocument document,
        string outputDirectory,
        string fileNamePattern = "page-{0}.png",
        RenderOptions? options = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (string.IsNullOrWhiteSpace(outputDirectory))
            throw new ArgumentNullException(nameof(outputDirectory));

        // Create directory if it doesn't exist
        Directory.CreateDirectory(outputDirectory);

        var filePaths = new List<string>();
        options ??= RenderOptions.Default;

        for (int i = 0; i < document.PageCount; i++)
        {
            using var page = document.GetPage(i);
            using var image = page.RenderToImage(options);

            var fileName = string.Format(fileNamePattern, i);
            var filePath = Path.Combine(outputDirectory, fileName);

            // Note: This will throw NotSupportedException if SkiaSharp extensions are not available
            image.SaveAsPng(filePath);
            filePaths.Add(filePath);
        }

        return filePaths.ToArray();
    }

    /// <summary>
    /// Saves a range of pages as image files.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="outputDirectory">Directory path where images will be saved.</param>
    /// <param name="startIndex">The zero-based starting page index.</param>
    /// <param name="count">Number of pages to save.</param>
    /// <param name="fileNamePattern">Optional file name pattern. Default: "page-{0}.png"</param>
    /// <param name="options">Optional render options.</param>
    /// <returns>Array of generated file paths.</returns>
    public static string[] SaveAsImages(
        this PdfDocument document,
        string outputDirectory,
        int startIndex,
        int count,
        string fileNamePattern = "page-{0}.png",
        RenderOptions? options = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (string.IsNullOrWhiteSpace(outputDirectory))
            throw new ArgumentNullException(nameof(outputDirectory));
        if (startIndex < 0 || startIndex >= document.PageCount)
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (count < 0 || startIndex + count > document.PageCount)
            throw new ArgumentOutOfRangeException(nameof(count));

        Directory.CreateDirectory(outputDirectory);

        var filePaths = new List<string>();
        options ??= RenderOptions.Default;

        for (int i = startIndex; i < startIndex + count; i++)
        {
            using var page = document.GetPage(i);
            using var image = page.RenderToImage(options);

            var fileName = string.Format(fileNamePattern, i);
            var filePath = Path.Combine(outputDirectory, fileName);

            image.SaveAsPng(filePath);
            filePaths.Add(filePath);
        }

        return filePaths.ToArray();
    }

    /// <summary>
    /// Saves all pages as image files using a custom path generator function.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="filePathGenerator">Function that generates file path for each page index.</param>
    /// <param name="options">Optional render options.</param>
    /// <returns>Array of generated file paths.</returns>
    /// <exception cref="ArgumentNullException">document or filePathGenerator is null.</exception>
    /// <example>
    /// <code>
    /// // Generate custom file names
    /// document.SaveAsImages(pageIndex => $"output/doc-{pageIndex:D3}.png");
    ///
    /// // With custom options
    /// var options = RenderOptions.Default.WithDpi(300).WithTransparency();
    /// document.SaveAsImages(
    ///     pageIndex => $"highres/page-{pageIndex}.png",
    ///     options);
    /// </code>
    /// </example>
    public static string[] SaveAsImages(
        this PdfDocument document,
        Func<int, string> filePathGenerator,
        RenderOptions? options = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (filePathGenerator == null)
            throw new ArgumentNullException(nameof(filePathGenerator));

        var filePaths = new List<string>();
        options ??= RenderOptions.Default;

        for (int i = 0; i < document.PageCount; i++)
        {
            using var page = document.GetPage(i);
            using var image = page.RenderToImage(options);

            var filePath = filePathGenerator(i);

            // Create directory if needed
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            image.SaveAsPng(filePath);
            filePaths.Add(filePath);
        }

        return filePaths.ToArray();
    }
}
