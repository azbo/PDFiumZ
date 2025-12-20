using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a bookmark (outline item) in a PDF document.
/// Bookmarks form a tree structure with parent-child relationships.
/// </summary>
public sealed unsafe class PdfBookmark
{
    private readonly FpdfBookmarkT? _handle;
    private readonly PdfDocument _document;
    private string? _title;

    /// <summary>
    /// Gets the bookmark title text.
    /// </summary>
    public string Title
    {
        get
        {
            if (_title is not null)
                return _title;

            // Get title length first (returns bytes needed including null terminator)
            var length = fpdf_doc.FPDFBookmarkGetTitle(_handle!, IntPtr.Zero, 0);
            if (length <= 2) // Empty or just null terminator
            {
                _title = string.Empty;
                return _title;
            }

            // Allocate buffer and get title (UTF-16)
            var buffer = new ushort[length / 2];
            fixed (ushort* pBuffer = buffer)
            {
                fpdf_doc.FPDFBookmarkGetTitle(_handle!, (IntPtr)pBuffer, length);

                // Convert to string (remove null terminator)
                _title = new string((char*)pBuffer, 0, (int)(length / 2) - 1);
            }

            return _title;
        }
    }

    /// <summary>
    /// Gets the number of child bookmarks.
    /// </summary>
    public int ChildCount => fpdf_doc.FPDFBookmarkGetCount(_handle!);

    /// <summary>
    /// Gets the destination this bookmark points to, or null if none.
    /// </summary>
    public PdfDestination? Destination
    {
        get
        {
            var destHandle = fpdf_doc.FPDFBookmarkGetDest(_document._handle!, _handle!);
            if (destHandle is null || destHandle.__Instance == IntPtr.Zero)
                return null;

            return new PdfDestination(destHandle, _document);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfBookmark"/> class.
    /// Internal constructor - created by PdfDocument only.
    /// </summary>
    internal PdfBookmark(FpdfBookmarkT handle, PdfDocument document)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <summary>
    /// Gets the first child bookmark, or null if none.
    /// </summary>
    public PdfBookmark? GetFirstChild()
    {
        var childHandle = fpdf_doc.FPDFBookmarkGetFirstChild(_document._handle!, _handle!);
        if (childHandle is null || childHandle.__Instance == IntPtr.Zero)
            return null;

        return new PdfBookmark(childHandle, _document);
    }

    /// <summary>
    /// Gets the next sibling bookmark, or null if none.
    /// </summary>
    public PdfBookmark? GetNextSibling()
    {
        var siblingHandle = fpdf_doc.FPDFBookmarkGetNextSibling(_document._handle!, _handle!);
        if (siblingHandle is null || siblingHandle.__Instance == IntPtr.Zero)
            return null;

        return new PdfBookmark(siblingHandle, _document);
    }

    /// <summary>
    /// Gets all child bookmarks as an enumerable.
    /// </summary>
    public IEnumerable<PdfBookmark> GetChildren()
    {
        var child = GetFirstChild();
        while (child is not null)
        {
            yield return child;
            child = child.GetNextSibling();
        }
    }

    /// <summary>
    /// Recursively gets all descendant bookmarks (children, grandchildren, etc.).
    /// </summary>
    public IEnumerable<PdfBookmark> GetAllDescendants()
    {
        foreach (var child in GetChildren())
        {
            yield return child;

            // Recursively get descendants
            foreach (var descendant in child.GetAllDescendants())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the bookmark.
    /// </summary>
    public override string ToString()
    {
        var dest = Destination;
        return dest is not null
            ? $"{Title} -> {dest}"
            : Title;
    }
}
