using System;

namespace PDFiumCore.HighLevel;

/// <summary>
/// Represents a destination that a link points to.
/// Can be either an internal page destination or an external URI.
/// </summary>
public sealed class PdfLinkDestination
{
    /// <summary>
    /// Gets the destination type.
    /// </summary>
    public PdfLinkDestinationType Type { get; }

    /// <summary>
    /// Gets the target page index for internal destinations.
    /// Null for external URI destinations.
    /// </summary>
    public int? PageIndex { get; }

    /// <summary>
    /// Gets the URI for external destinations.
    /// Null for internal page destinations.
    /// </summary>
    public string? Uri { get; }

    /// <summary>
    /// Gets whether this destination has a valid page index.
    /// </summary>
    public bool HasValidPage => Type == PdfLinkDestinationType.InternalPage && PageIndex.HasValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLinkDestination"/> class for an internal page.
    /// Internal constructor - created by PdfLink only.
    /// </summary>
    internal PdfLinkDestination(int pageIndex)
    {
        Type = PdfLinkDestinationType.InternalPage;
        PageIndex = pageIndex;
        Uri = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLinkDestination"/> class for an external URI.
    /// Internal constructor - created by PdfLink only.
    /// </summary>
    internal PdfLinkDestination(string uri)
    {
        Type = PdfLinkDestinationType.ExternalUri;
        PageIndex = null;
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLinkDestination"/> class for unknown destination.
    /// Internal constructor - created by PdfLink only.
    /// </summary>
    internal PdfLinkDestination()
    {
        Type = PdfLinkDestinationType.Unknown;
        PageIndex = null;
        Uri = null;
    }
}

/// <summary>
/// Specifies the type of link destination.
/// </summary>
public enum PdfLinkDestinationType
{
    /// <summary>
    /// Unknown or unsupported destination type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Destination is an internal page within the document.
    /// </summary>
    InternalPage = 1,

    /// <summary>
    /// Destination is an external URI (URL).
    /// </summary>
    ExternalUri = 2
}
