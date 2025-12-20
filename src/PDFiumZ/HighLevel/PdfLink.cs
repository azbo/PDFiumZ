using System;

namespace PDFiumCore.HighLevel;

/// <summary>
/// Represents a hyperlink on a PDF page.
/// Must not outlive its parent PdfPage.
/// </summary>
public sealed unsafe class PdfLink : IDisposable
{
    private FpdfLinkT? _linkHandle;
    private readonly PdfPage _page;
    private bool _disposed;
    private PdfLinkDestination? _destination;

    /// <summary>
    /// Gets the link destination.
    /// Cached after first access.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The link has been disposed.</exception>
    public PdfLinkDestination Destination
    {
        get
        {
            ThrowIfDisposed();

            if (_destination == null)
            {
                _destination = GetDestination();
            }

            return _destination;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLink"/> class.
    /// Internal constructor - created by PdfPage only.
    /// </summary>
    internal PdfLink(FpdfLinkT linkHandle, PdfPage page)
    {
        _linkHandle = linkHandle ?? throw new ArgumentNullException(nameof(linkHandle));
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }

    private PdfLinkDestination GetDestination()
    {
        ThrowIfDisposed();

        // Try to get action first (for URI links)
        var action = fpdf_doc.FPDFLinkGetAction(_linkHandle!);
        if (action != null && action.__Instance != IntPtr.Zero)
        {
            // Get action type
            var actionType = fpdf_doc.FPDFActionGetType(action);

            // Type 3 = URI
            if (actionType == 3)
            {
                // Get URI string (in bytes/ASCII format)
                var bufferSize = fpdf_doc.FPDFActionGetURIPath(_page._document._handle!, action, IntPtr.Zero, 0);

                if (bufferSize > 1)
                {
                    var buffer = new byte[bufferSize];
                    fixed (byte* pBuffer = buffer)
                    {
                        fpdf_doc.FPDFActionGetURIPath(_page._document._handle!, action, (IntPtr)pBuffer, bufferSize);

                        // Find null terminator
                        int length = 0;
                        while (length < bufferSize && buffer[length] != 0)
                            length++;

                        if (length > 0)
                        {
                            var uri = System.Text.Encoding.ASCII.GetString(buffer, 0, length);
                            return new PdfLinkDestination(uri);
                        }
                    }
                }
            }
        }

        // Try to get destination (for internal page links)
        var dest = fpdf_doc.FPDFLinkGetDest(_page._document._handle!, _linkHandle!);
        if (dest != null && dest.__Instance != IntPtr.Zero)
        {
            var pageIndex = fpdf_doc.FPDFDestGetDestPageIndex(_page._document._handle!, dest);
            if (pageIndex >= 0)
            {
                return new PdfLinkDestination(pageIndex);
            }
        }

        // Unknown destination
        return new PdfLinkDestination();
    }

    /// <summary>
    /// Releases all resources used by the <see cref="PdfLink"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // Note: Link handles are not explicitly freed in PDFium
        // They are owned by the page and freed when the page is closed
        _linkHandle = null;

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PdfLink));
    }
}
