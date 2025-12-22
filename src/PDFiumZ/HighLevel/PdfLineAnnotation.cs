using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a line annotation.
/// </summary>
public sealed class PdfLineAnnotation : PdfAnnotation
{
    internal PdfLineAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Line, index)
    {
    }

    /// <summary>
    /// Tries to get the start and end points of the line in page coordinates.
    /// </summary>
    public bool TryGetLine(out (double X, double Y) start, out (double X, double Y) end)
    {
        ThrowIfDisposed();

        using var startPoint = new FS_POINTF_();
        using var endPoint = new FS_POINTF_();

        var result = fpdf_annot.FPDFAnnotGetLine(_handle!, startPoint, endPoint);
        if (result == 0)
        {
            start = default;
            end = default;
            return false;
        }

        start = (startPoint.X, startPoint.Y);
        end = (endPoint.X, endPoint.Y);
        return true;
    }

    /// <summary>
    /// Gets the start point of the line in page coordinates.
    /// </summary>
    public (double X, double Y) Start
    {
        get
        {
            if (!TryGetLine(out var start, out _))
                throw new PdfException("Failed to get line start/end points.");
            return start;
        }
    }

    /// <summary>
    /// Gets the end point of the line in page coordinates.
    /// </summary>
    public (double X, double Y) End
    {
        get
        {
            if (!TryGetLine(out _, out var end))
                throw new PdfException("Failed to get line start/end points.");
            return end;
        }
    }
}

