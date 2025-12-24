using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Async extension methods for generating images from PdfDocument.
/// Provides asynchronous streaming support for large-scale operations.
/// </summary>
public static class PdfDocumentAsyncExtensions
{
#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER || NET9_0_OR_GREATER || NET10_0_OR_GREATER
    /// <summary>
    /// Asynchronously generates images from the document using the specified options.
    /// Uses async streaming for efficient memory usage with large documents.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="options">Options specifying which pages to render and how.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>An async enumerable of <see cref="PdfImage"/> objects. Remember to dispose each image after use.</returns>
    /// <exception cref="ArgumentNullException">document or options is null.</exception>
    /// <exception cref="ObjectDisposedException">document has been disposed.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Any page index is out of range.</exception>
    /// <example>
    /// <code>
    /// await foreach (var image in document.GenerateImagesAsync(options))
    /// {
    ///     // Process image
    ///     image.Dispose();
    /// }
    /// </code>
    /// </example>
    public static async IAsyncEnumerable<PdfImage> GenerateImagesAsync(
        this PdfDocument document,
        ImageGenerationOptions options,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
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

        // Generate images asynchronously
        foreach (var i in indices)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Run page loading and rendering in background thread
            var image = await Task.Run(() =>
            {
                using var page = document.GetPages(i, 1).First();
                return page.RenderToImage(renderOptions);
            }, cancellationToken);

            yield return image;
        }
    }

    /// <summary>
    /// Asynchronously generates thumbnail images for the document.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="options">Options specifying thumbnail size, quality, and which pages to generate.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>An async enumerable collection of <see cref="PdfImage"/> thumbnails.</returns>
    /// <exception cref="ArgumentNullException">document or options is null.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    /// <exception cref="ObjectDisposedException">Document has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">options.MaxWidth or options.Quality is invalid, or any page index is out of range.</exception>
    /// <example>
    /// <code>
    /// var options = new ThumbnailOptions { MaxWidth = 400, Quality = 2 };
    /// await foreach (var thumb in document.GenerateThumbnailsAsync(options))
    /// {
    ///     thumb.Dispose();
    /// }
    /// </code>
    /// </example>
    public static async IAsyncEnumerable<PdfImage> GenerateThumbnailsAsync(
        this PdfDocument document,
        ThumbnailOptions options,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        options.Validate();

        // Determine which pages to process
        int[]? indices = options.PageIndices;
        if (indices == null)
        {
            indices = new int[document.PageCount];
            for (int i = 0; i < document.PageCount; i++)
                indices[i] = i;
        }

        // Validate indices
        foreach (var index in indices)
        {
            if (index < 0 || index >= document.PageCount)
                throw new ArgumentOutOfRangeException(nameof(options.PageIndices),
                    $"Page index {index} is out of range (0-{document.PageCount - 1}).");
        }

        // Generate thumbnails asynchronously
        foreach (var index in indices)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var thumb = await Task.Run(() =>
            {
                using var page = document.GetPage(index);
                return page.GenerateThumbnail(options.MaxWidth, options.Quality);
            }, cancellationToken);

            yield return thumb;
        }
    }
#else
    /// <summary>
    /// Asynchronously generates all images from the document.
    /// Note: IAsyncEnumerable is not available in .NET Standard 2.0.
    /// Use .NET Standard 2.1 or later for async streaming support.
    /// </summary>
    public static Task<PdfImage[]> GenerateAllImagesAsync(
        this PdfDocument document,
        CancellationToken cancellationToken = default)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return Task.Run(() =>
        {
            return document.GenerateImages(ImageGenerationOptions.Default).ToArray();
        }, cancellationToken);
    }
#endif
}
