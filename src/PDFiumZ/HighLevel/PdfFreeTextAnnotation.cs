using System;
using System.Runtime.InteropServices;
using PDFiumZ.Utilities;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a free text annotation (text box) in a PDF document.
/// </summary>
public sealed unsafe class PdfFreeTextAnnotation : PdfAnnotation
{
    /// <summary>
    /// Creates a new free text annotation on the specified page.
    /// </summary>
    /// <param name="page">The page to add the annotation to.</param>
    /// <param name="rect">The bounding rectangle of the text box.</param>
    /// <param name="text">The text content.</param>
    /// <returns>A new <see cref="PdfFreeTextAnnotation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">page is null.</exception>
    /// <exception cref="PdfException">Failed to create annotation.</exception>
    public static PdfFreeTextAnnotation Create(
        PdfPage page,
        PdfRectangle rect,
        string text)
    {
        if (page is null) throw new ArgumentNullException(nameof(page));
        page.ThrowIfDisposed();

        var handle = fpdf_annot.FPDFPageCreateAnnot(page._handle!, (int)PdfAnnotationType.FreeText);
        if (handle is null || handle.__Instance == IntPtr.Zero)
            throw new PdfException("Failed to create FreeText annotation.");

        var annotation = new PdfFreeTextAnnotation(handle, page, -1);

        try
        {
            annotation.Bounds = rect;
            annotation.Contents = text;
            // Default appearance is often required for the text to show up.
            // We set a basic DA string: "0 0 0 rg /Helv 12 Tf" (Black, Helvetica, 12pt)
            annotation.DefaultAppearance = "0 0 0 rg /Helv 12 Tf";
        }
        catch
        {
            annotation.Dispose();
            throw;
        }

        return annotation;
    }

    internal PdfFreeTextAnnotation(global::PDFiumZ.FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.FreeText, index)
    {
    }

    /// <summary>
    /// Gets or sets the text content of the annotation.
    /// </summary>
    public string Contents
    {
        get
        {
            ThrowIfDisposed();
            ushort dummy = 0;
            // Get length in bytes (including terminator)
            uint len = fpdf_annot.FPDFAnnotGetStringValue(_handle!, "Contents", ref dummy, 0);
            if (len <= 2) return string.Empty;

            var buffer = new ushort[len / 2];
            fpdf_annot.FPDFAnnotGetStringValue(_handle!, "Contents", ref buffer[0], len);
            
            fixed (ushort* p = buffer)
            {
                return new string((char*)p, 0, (int)(len / 2) - 1);
            }
        }
        set
        {
            ThrowIfDisposed();
            var str = value ?? string.Empty;

            var utf16Array = str.ToNullTerminatedUtf16Array();

            fpdf_annot.FPDFAnnotSetStringValue(_handle!, "Contents", ref utf16Array[0]);
        }
    }

    /// <summary>
    /// Gets or sets the Default Appearance (DA) string for the annotation.
    /// This controls font, size, and color of the text.
    /// </summary>
    public string DefaultAppearance
    {
        get
        {
            ThrowIfDisposed();
            ushort dummy = 0;
            uint len = fpdf_annot.FPDFAnnotGetStringValue(_handle!, "DA", ref dummy, 0);
            if (len <= 2) return string.Empty;

            var buffer = new ushort[len / 2];
            fpdf_annot.FPDFAnnotGetStringValue(_handle!, "DA", ref buffer[0], len);
            
            fixed (ushort* p = buffer)
            {
                return new string((char*)p, 0, (int)(len / 2) - 1);
            }
        }
        set
        {
            ThrowIfDisposed();
            var str = value ?? string.Empty;

            var utf16Array = str.ToNullTerminatedUtf16Array();

            fpdf_annot.FPDFAnnotSetStringValue(_handle!, "DA", ref utf16Array[0]);
        }
    }
}
