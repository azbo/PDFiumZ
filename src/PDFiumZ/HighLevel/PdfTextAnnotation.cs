using System;
using PDFiumZ.Utilities;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a text annotation (sticky note/comment) in a PDF document.
/// Text annotations appear as small icons that display a popup with text content when clicked.
/// </summary>
public sealed unsafe class PdfTextAnnotation : PdfAnnotation
{
    private ushort _dummyBuffer;
    /// <summary>
    /// Creates a new text annotation at the specified position.
    /// </summary>
    /// <param name="page">The page to add the annotation to.</param>
    /// <param name="x">The X coordinate of the annotation icon.</param>
    /// <param name="y">The Y coordinate of the annotation icon.</param>
    /// <param name="contents">The text content of the annotation.</param>
    /// <returns>A new <see cref="PdfTextAnnotation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">page is null.</exception>
    /// <exception cref="PdfException">Failed to create annotation.</exception>
    public static PdfTextAnnotation Create(PdfPage page, double x, double y, string contents = "")
    {
        if (page is null)
            throw new ArgumentNullException(nameof(page));

        page.ThrowIfDisposed();

        // Create text annotation (type 1)
        var handle = fpdf_annot.FPDFPageCreateAnnot(page._handle!, (int)PdfAnnotationType.Text);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create text annotation.");
        }

        var annotation = new PdfTextAnnotation(handle, page, -1);

        // Set position (text annotations typically have a small fixed size, like 20x20)
        annotation.Bounds = new PdfRectangle(x, y, 20, 20);

        // Set content if provided
        if (!string.IsNullOrEmpty(contents))
        {
            annotation.Contents = contents;
        }

        return annotation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfTextAnnotation"/> class.
    /// Internal constructor - use factory methods to create annotations.
    /// </summary>
    internal PdfTextAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Text, index)
    {
    }

    /// <summary>
    /// Gets or sets the text content of the annotation.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    public string Contents
    {
        get
        {
            ThrowIfDisposed();

            // Get the length first
            var length = fpdf_annot.FPDFAnnotGetStringValue(_handle!, "Contents", ref _dummyBuffer, 0);
            if (length <= 2) // Empty or just null terminator
                return string.Empty;

            // Allocate buffer for UTF-16 string
            var buffer = new ushort[length / 2];
            fpdf_annot.FPDFAnnotGetStringValue(_handle!, "Contents", ref buffer[0], length);

            // Convert from UTF-16LE to string (exclude null terminator)
            fixed (ushort* pBuffer = buffer)
            {
                return new string((char*)pBuffer, 0, (int)(length / 2) - 1);
            }
        }
        set
        {
            ThrowIfDisposed();

            if (value is null)
                value = string.Empty;

            // Convert to UTF-16LE (ushort array)
            var utf16Array = value.ToNullTerminatedUtf16Array();

            var result = fpdf_annot.FPDFAnnotSetStringValue(_handle!, "Contents", ref utf16Array[0]);
            if (result == 0)
            {
                throw new PdfException("Failed to set annotation contents.");
            }
        }
    }

    /// <summary>
    /// Gets or sets the author of the annotation.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The annotation has been disposed.</exception>
    public string Author
    {
        get
        {
            ThrowIfDisposed();

            // Get the length first
            var length = fpdf_annot.FPDFAnnotGetStringValue(_handle!, "T", ref _dummyBuffer, 0);
            if (length <= 2) // Empty or just null terminator
                return string.Empty;

            // Allocate buffer for UTF-16 string
            var buffer = new ushort[length / 2];
            fpdf_annot.FPDFAnnotGetStringValue(_handle!, "T", ref buffer[0], length);

            // Convert from UTF-16LE to string (exclude null terminator)
            fixed (ushort* pBuffer = buffer)
            {
                return new string((char*)pBuffer, 0, (int)(length / 2) - 1);
            }
        }
        set
        {
            ThrowIfDisposed();

            if (value is null)
                value = string.Empty;

            // Convert to UTF-16LE (ushort array)
            var utf16Array = value.ToNullTerminatedUtf16Array();

            var result = fpdf_annot.FPDFAnnotSetStringValue(_handle!, "T", ref utf16Array[0]);
            if (result == 0)
            {
                throw new PdfException("Failed to set annotation author.");
            }
        }
    }
}
