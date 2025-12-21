namespace PDFiumZ.HighLevel;

/// <summary>
/// Specifies the rotation angle for a PDF page.
/// </summary>
public enum PdfRotation
{
    /// <summary>
    /// No rotation (0 degrees).
    /// </summary>
    None = 0,

    /// <summary>
    /// Rotated 90 degrees clockwise.
    /// </summary>
    Rotate90 = 1,

    /// <summary>
    /// Rotated 180 degrees clockwise.
    /// </summary>
    Rotate180 = 2,

    /// <summary>
    /// Rotated 270 degrees clockwise (or 90 degrees counter-clockwise).
    /// </summary>
    Rotate270 = 3
}
