using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a link annotation in a PDF document.
/// Supports external URI links (web URLs, email links, etc.).
/// </summary>
/// <remarks>
/// Link annotations create clickable areas on PDF pages that can open external URIs.
/// Note: PDFium 145 does not support creating internal page destination links programmatically.
/// </remarks>
public sealed class PdfLinkAnnotation : PdfAnnotation
{
    /// <summary>
    /// Creates an external link annotation that opens a URI when clicked.
    /// </summary>
    /// <param name="page">The page to add the link to.</param>
    /// <param name="bounds">The clickable area of the link in page coordinates.</param>
    /// <param name="uri">The URI to open (e.g., "https://example.com", "mailto:user@example.com").</param>
    /// <param name="color">The link border color in ARGB format (default: transparent blue 0x400000FF).</param>
    /// <returns>A new link annotation.</returns>
    /// <exception cref="ArgumentNullException">page or uri is null.</exception>
    /// <exception cref="ObjectDisposedException">page has been disposed.</exception>
    /// <exception cref="PdfException">Failed to create link annotation or set URI.</exception>
    /// <example>
    /// <code>
    /// using var doc = PdfDocument.CreateNew();
    /// using var page = doc.CreatePage();
    ///
    /// // Create a clickable link to a website
    /// var bounds = new PdfRectangle(50, 700, 200, 30);
    /// using var link = PdfLinkAnnotation.CreateExternal(page, bounds, "https://github.com/casbin-net/pdfiumz");
    /// </code>
    /// </example>
    public static PdfLinkAnnotation CreateExternal(PdfPage page, PdfRectangle bounds, string uri, uint color = 0x400000FF)
    {
        if (page is null)
            throw new ArgumentNullException(nameof(page));
        if (uri is null)
            throw new ArgumentNullException(nameof(uri));

        page.ThrowIfDisposed();

        // Create link annotation
        var handle = fpdf_annot.FPDFPageCreateAnnot(page._handle!, (int)PdfAnnotationType.Link);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create link annotation.");
        }

        var annotation = new PdfLinkAnnotation(handle, page, -1);

        try
        {
            // Set bounds
            annotation.Bounds = bounds;

            // Set border color
            annotation.Color = color;

            // Set URI target
            var result = fpdf_annot.FPDFAnnotSetURI(handle, uri);
            if (result == 0)
            {
                throw new PdfException($"Failed to set link URI: {uri}");
            }

            return annotation;
        }
        catch
        {
            annotation.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Sets the URI target of this link annotation.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Value is null when setting.</exception>
    /// <exception cref="PdfException">Failed to set URI.</exception>
    /// <remarks>
    /// This property is write-only. PDFium does not provide a direct API to read the URI back from a link annotation.
    /// To read existing link URIs, use the PdfLink class and its Destination property instead.
    /// </remarks>
    public string Uri
    {
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            ThrowIfDisposed();

            var result = fpdf_annot.FPDFAnnotSetURI(_handle!, value);
            if (result == 0)
            {
                throw new PdfException($"Failed to set link URI: {value}");
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLinkAnnotation"/> class.
    /// Internal constructor - created by factory methods or PdfPage only.
    /// </summary>
    internal PdfLinkAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Link, index)
    {
    }
}
