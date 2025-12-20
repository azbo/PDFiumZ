using System;

namespace PDFiumCore.HighLevel;

/// <summary>
/// Represents a destination in a PDF document (where a bookmark or link points to).
/// </summary>
public sealed class PdfDestination
{
    private readonly FpdfDestT? _handle;
    private readonly PdfDocument _document;

    /// <summary>
    /// Gets the zero-based page index of the destination.
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// Gets whether the destination has a valid page index.
    /// </summary>
    public bool HasValidPage => PageIndex >= 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfDestination"/> class.
    /// Internal constructor - created by PdfBookmark only.
    /// </summary>
    internal PdfDestination(FpdfDestT handle, PdfDocument document)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        _document = document ?? throw new ArgumentNullException(nameof(document));

        // Get page index (returns -1 if invalid)
        PageIndex = fpdf_doc.FPDFDestGetDestPageIndex(_document._handle!, _handle);
    }

    /// <summary>
    /// Returns a string representation of the destination.
    /// </summary>
    public override string ToString()
    {
        return HasValidPage ? $"Page {PageIndex}" : "Invalid destination";
    }
}
