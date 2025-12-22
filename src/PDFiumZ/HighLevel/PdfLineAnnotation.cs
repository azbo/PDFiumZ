using System;

namespace PDFiumZ.HighLevel;

public sealed class PdfLineAnnotation : PdfAnnotation
{
    internal PdfLineAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Line, index)
    {
    }

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

    public (double X, double Y) Start
    {
        get
        {
            if (!TryGetLine(out var start, out _))
                throw new PdfException("Failed to get line start/end points.");
            return start;
        }
    }

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

