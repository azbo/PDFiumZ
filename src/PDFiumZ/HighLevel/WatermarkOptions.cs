namespace PDFiumZ.HighLevel;

/// <summary>
/// Options for configuring watermark appearance.
/// </summary>
public sealed class WatermarkOptions
{
    /// <summary>
    /// Gets or sets the opacity of the watermark (0.0 = transparent, 1.0 = opaque).
    /// Default is 0.5.
    /// </summary>
    public double Opacity { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the rotation angle of the watermark in degrees (clockwise).
    /// Default is 0 (no rotation).
    /// </summary>
    public double Rotation { get; set; } = 0;

    /// <summary>
    /// Gets or sets the font size for text watermarks in points.
    /// Default is 48.
    /// </summary>
    public double FontSize { get; set; } = 48;

    /// <summary>
    /// Gets or sets the color of the watermark in ARGB format.
    /// Default is gray (0xFF808080).
    /// </summary>
    public uint Color { get; set; } = 0xFF808080;

    /// <summary>
    /// Gets or sets the font for text watermarks.
    /// Default is Helvetica Bold.
    /// </summary>
    public PdfStandardFont Font { get; set; } = PdfStandardFont.HelveticaBold;
}
