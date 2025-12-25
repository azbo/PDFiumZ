using System;
using PDFiumZ.Utilities;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a stamp annotation (rubber stamp) in a PDF document.
/// Stamp annotations display a predefined icon or graphic representing a rubber stamp.
/// </summary>
public sealed unsafe class PdfStampAnnotation : PdfAnnotation
{
    private ushort _dummyBuffer;

    /// <summary>
    /// Creates a new stamp annotation at the specified location.
    /// </summary>
    /// <param name="page">The page to add the annotation to.</param>
    /// <param name="bounds">The bounding rectangle of the stamp.</param>
    /// <param name="stampType">The type of stamp to create.</param>
    /// <returns>A new <see cref="PdfStampAnnotation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">page is null.</exception>
    /// <exception cref="PdfException">Failed to create annotation.</exception>
    public static PdfStampAnnotation Create(PdfPage page, PdfRectangle bounds, PdfStampType stampType = PdfStampType.Draft)
    {
        if (page is null)
            throw new ArgumentNullException(nameof(page));

        page.ThrowIfDisposed();

        // Create stamp annotation (type 13)
        var handle = fpdf_annot.FPDFPageCreateAnnot(page._handle!, (int)PdfAnnotationType.Stamp);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create stamp annotation.");
        }

        var annotation = new PdfStampAnnotation(handle, page, -1, stampType);

        // Set bounds
        annotation.Bounds = bounds;

        // Set the stamp icon name based on type
        annotation.SetStampIcon(stampType);

        return annotation;
    }

    /// <summary>
    /// Initializes a new instance from an existing annotation handle.
    /// Internal constructor - used when loading existing annotations.
    /// </summary>
    internal PdfStampAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Stamp, index)
    {
        // Try to read the stamp type from the annotation
        StampType = ReadStampType();
    }

    /// <summary>
    /// Initializes a new instance for a newly created annotation.
    /// Internal constructor - used when creating new annotations.
    /// </summary>
    private PdfStampAnnotation(FpdfAnnotationT handle, PdfPage page, int index, PdfStampType stampType)
        : base(handle, page, PdfAnnotationType.Stamp, index)
    {
        StampType = stampType;
    }

    /// <summary>
    /// Gets the type of stamp for this annotation.
    /// </summary>
    public PdfStampType StampType { get; }

    /// <summary>
    /// Reads the stamp type from an existing annotation.
    /// </summary>
    private PdfStampType ReadStampType()
    {
        ThrowIfDisposed();

        // Get the icon name from the annotation
        var length = fpdf_annot.FPDFAnnotGetStringValue(_handle!, "Name", ref _dummyBuffer, 0);
        if (length <= 2) // Empty or just null terminator
            return PdfStampType.Draft; // Default

        // Allocate buffer for UTF-16 string
        var buffer = new ushort[length / 2];
        fpdf_annot.FPDFAnnotGetStringValue(_handle!, "Name", ref buffer[0], length);

        // Convert from UTF-16LE to string (exclude null terminator)
        string iconName;
        fixed (ushort* pBuffer = buffer)
        {
            iconName = new string((char*)pBuffer, 0, (int)(length / 2) - 1);
        }

        // Map icon name back to stamp type
        return iconName switch
        {
            "Approved" => PdfStampType.Approved,
            "Experimental" => PdfStampType.Experimental,
            "NotApproved" => PdfStampType.NotApproved,
            "AsIs" => PdfStampType.AsIs,
            "Expired" => PdfStampType.Expired,
            "NotForPublicRelease" => PdfStampType.NotForPublicRelease,
            "Confidential" => PdfStampType.Confidential,
            "Final" => PdfStampType.Final,
            "Sold" => PdfStampType.Sold,
            "Departmental" => PdfStampType.Departmental,
            "ForComment" => PdfStampType.ForComment,
            "TopSecret" => PdfStampType.TopSecret,
            "ForPublicRelease" => PdfStampType.ForPublicRelease,
            "Draft" => PdfStampType.Draft,
            _ => PdfStampType.Draft
        };
    }

    /// <summary>
    /// Sets the stamp icon name based on the stamp type.
    /// </summary>
    private void SetStampIcon(PdfStampType stampType)
    {
        // Map stamp type to PDFium icon name
        var iconName = stampType switch
        {
            PdfStampType.Approved => "Approved",
            PdfStampType.Experimental => "Experimental",
            PdfStampType.NotApproved => "NotApproved",
            PdfStampType.AsIs => "AsIs",
            PdfStampType.Expired => "Expired",
            PdfStampType.NotForPublicRelease => "NotForPublicRelease",
            PdfStampType.Confidential => "Confidential",
            PdfStampType.Final => "Final",
            PdfStampType.Sold => "Sold",
            PdfStampType.Departmental => "Departmental",
            PdfStampType.ForComment => "ForComment",
            PdfStampType.TopSecret => "TopSecret",
            PdfStampType.ForPublicRelease => "ForPublicRelease",
            PdfStampType.Draft => "Draft",
            _ => "Draft"
        };

        // Convert to UTF-16LE (ushort array)
        var utf16Array = iconName.ToNullTerminatedUtf16Array();

        // Set the icon name using the "Name" key
        fpdf_annot.FPDFAnnotSetStringValue(_handle!, "Name", ref utf16Array[0]);
    }
}
