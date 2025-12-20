using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a generic PDF annotation for types that don't have specialized implementations yet.
/// This class provides basic annotation functionality while specific annotation types are being implemented.
/// </summary>
internal sealed class GenericAnnotation : PdfAnnotation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericAnnotation"/> class.
    /// </summary>
    internal GenericAnnotation(FpdfAnnotationT handle, PdfPage page, PdfAnnotationType type, int index)
        : base(handle, page, type, index)
    {
    }
}
