using PDFiumZ.HighLevel;

namespace PDFiumZ.Fluent;

/// <summary>
/// Context information for measuring elements.
/// </summary>
public class MeasureContext
{
    public Size AvailableSpace { get; set; }
    public PdfDocument Document { get; set; } = null!;
    public PdfFont DefaultFont { get; set; } = null!;
    public double DefaultFontSize { get; set; } = 12;

    public MeasureContext Clone()
    {
        return new MeasureContext
        {
            AvailableSpace = AvailableSpace,
            Document = Document,
            DefaultFont = DefaultFont,
            DefaultFontSize = DefaultFontSize
        };
    }
}

/// <summary>
/// Context information for rendering elements.
/// </summary>
public class RenderContext
{
    public PdfContentEditor Editor { get; set; } = null!;
    public Position Position { get; set; }
    public Size AvailableSpace { get; set; }
    public PdfDocument Document { get; set; } = null!;
    public PdfFont DefaultFont { get; set; } = null!;
    public double DefaultFontSize { get; set; } = 12;

    public RenderContext Clone()
    {
        return new RenderContext
        {
            Editor = Editor,
            Position = Position,
            AvailableSpace = AvailableSpace,
            Document = Document,
            DefaultFont = DefaultFont,
            DefaultFontSize = DefaultFontSize
        };
    }
}
