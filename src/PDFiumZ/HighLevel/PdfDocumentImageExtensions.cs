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
    /// <summary>
    /// Generates images from the specified page indices.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="pageIndices">Zero-based indices of pages to render.</param>
    /// <returns>An enumerable of PdfImage objects. Remember to dispose each image after use.</returns>
    /// <exception cref="ArgumentNullException">document or pageIndices is null.</exception>
    /// <exception cref="ObjectDisposedException">document has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Any index is out of range.</exception>
    public static IEnumerable<PdfImage> GenerateImages(
        this PdfDocument document,
        params int[] pageIndices)
    {
        return GenerateImages(document, pageIndices, null);
    }

    /// <summary>
    /// Generates images from a range of pages.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="startIndex">Zero-based index of the first page to render.</param>
    /// <param name="count">Number of pages to render.</param>
    /// <param name="options">Optional render options.</param>
    /// <returns>An enumerable of PdfImage objects.</returns>
    public static IEnumerable<PdfImage> GenerateImages(
        this PdfDocument document,
        int startIndex,
        int count,
        RenderOptions? options = null)
    {
        var indices = Enumerable.Range(startIndex, count).ToArray();
        return GenerateImages(document, indices, options);
    }

    /// <summary>
    /// Generates images for all pages in the document.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="options">Optional render options.</param>
    /// <returns>An enumerable of PdfImage objects.</returns>
    public static IEnumerable<PdfImage> GenerateImages(
        this PdfDocument document,
        RenderOptions? options = null)
    {
        return GenerateImages(document, 0, document.PageCount, options);
    }

    /// <summary>
    /// Generates images from the specified page indices with custom options.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="pageIndices">Zero-based indices of pages to render.</param>
    /// <param name="options">Optional render options. If null, uses default settings.</param>
    /// <returns>An enumerable of PdfImage objects. Remember to dispose each image after use.</returns>
    /// <exception cref="ArgumentNullException">document or pageIndices is null.</exception>
    /// <exception cref="ObjectDisposedException">document has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Any index is out of range.</exception>
    public static IEnumerable<PdfImage> GenerateImages(
        this PdfDocument document,
        int[] pageIndices,
        RenderOptions? options = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (pageIndices == null)
            throw new ArgumentNullException(nameof(pageIndices));

        options ??= RenderOptions.Default;

        foreach (var i in pageIndices)
        {
            if (i < 0 || i >= document.PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndices), $"Page index {i} is out of range.");

            using var page = document.GetPages(i, 1).First();
            yield return page.RenderToImage(options);
        }
    }

    /// <summary>
    /// Saves all pages as image files to a directory.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="outputDirectory">Directory path where images will be saved.</param>
    /// <param name="options">Optional render options.</param>
    /// <returns>Array of generated file paths.</returns>
    public static string[] SaveAsImages(
        this PdfDocument document,
        string outputDirectory,
        RenderOptions? options = null)
    {
        return SaveAsImages(document, outputDirectory, 0, document.PageCount, "page-{0}.png", options);
    }

    /// <summary>
    /// Saves a range of pages as image files to a directory.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="outputDirectory">Directory path where images will be saved.</param>
    /// <param name="startIndex">Zero-based index of the first page to save.</param>
    /// <param name="count">Number of pages to save.</param>
    /// <param name="fileNamePattern">Optional file name pattern. Use {0} for page index. Default: "page-{0}.png"</param>
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
        var indices = Enumerable.Range(startIndex, count).ToArray();
        return SaveAsImages(document, outputDirectory, indices, fileNamePattern, options);
    }

    /// <summary>
    /// Saves the specified pages as image files to a directory.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="outputDirectory">Directory path where images will be saved.</param>
    /// <param name="pageIndices">Zero-based indices of pages to save.</param>
    /// <returns>Array of generated file paths.</returns>
    public static string[] SaveAsImages(
        this PdfDocument document,
        string outputDirectory,
        params int[] pageIndices)
    {
        return SaveAsImages(document, outputDirectory, pageIndices, "page-{0}.png", null);
    }

    /// <summary>
    /// Saves the specified pages as image files to a directory.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="outputDirectory">Directory path where images will be saved.</param>
    /// <param name="pageIndices">Zero-based indices of pages to save.</param>
    /// <param name="fileNamePattern">Optional file name pattern. Use {0} for page index. Default: "page-{0}.png"</param>
    /// <param name="options">Optional render options.</param>
    /// <returns>Array of generated file paths.</returns>
    public static string[] SaveAsImages(
        this PdfDocument document,
        string outputDirectory,
        int[] pageIndices,
        string fileNamePattern = "page-{0}.png",
        RenderOptions? options = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (string.IsNullOrWhiteSpace(outputDirectory))
            throw new ArgumentNullException(nameof(outputDirectory));
        if (pageIndices == null)
            throw new ArgumentNullException(nameof(pageIndices));

        // Create directory if it doesn't exist
        Directory.CreateDirectory(outputDirectory);

        var filePaths = new List<string>();
        options ??= RenderOptions.Default;

        foreach (var i in pageIndices)
        {
            if (i < 0 || i >= document.PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndices), $"Page index {i} is out of range.");

            using var page = document.GetPages(i, 1).First();
            using var image = page.RenderToImage(options);

            var fileName = string.Format(fileNamePattern, i);
            var filePath = Path.Combine(outputDirectory, fileName);

            image.SaveAsPng(filePath);
            filePaths.Add(filePath);
        }

        return filePaths.ToArray();
    }

    /// <summary>
    /// Saves pages as image files using a custom path generator.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="pathGenerator">Function that generates a file path for a given page index.</param>
    /// <param name="startIndex">Zero-based index of the first page to save. Defaults to 0.</param>
    /// <param name="count">Number of pages to save. Defaults to all remaining pages.</param>
    /// <param name="options">Optional render options.</param>
    /// <returns>Array of generated file paths.</returns>
    public static string[] SaveAsImages(
        this PdfDocument document,
        Func<int, string> pathGenerator,
        int? startIndex = null,
        int? count = null,
        RenderOptions? options = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (pathGenerator == null)
            throw new ArgumentNullException(nameof(pathGenerator));

        int start = startIndex ?? 0;
        int num = count ?? (document.PageCount - start);

        var filePaths = new List<string>();
        options ??= RenderOptions.Default;

        for (int i = start; i < start + num; i++)
        {
            if (i < 0 || i >= document.PageCount)
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"Page index {i} is out of range.");

            using var page = document.GetPages(i, 1).First();
            using var image = page.RenderToImage(options);

            var filePath = pathGenerator(i);
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            image.SaveAsPng(filePath);
            filePaths.Add(filePath);
        }

        return filePaths.ToArray();
    }
}
