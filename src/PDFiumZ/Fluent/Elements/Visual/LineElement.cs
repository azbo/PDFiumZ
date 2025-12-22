using PDFiumZ.HighLevel;

namespace PDFiumZ.Fluent.Elements.Visual;

/// <summary>
/// Draws a horizontal or vertical line.
/// </summary>
public class LineElement : IElement
{
    public bool IsHorizontal { get; set; } = true;
    public double Length { get; set; }
    public uint Color { get; set; } = PdfColor.Black;
    public double Width { get; set; } = 1.0;

    public LineElement(double length, bool horizontal = true)
    {
        Length = length;
        IsHorizontal = horizontal;
    }

    public SpacePlan Measure(MeasureContext context)
    {
        var size = IsHorizontal
            ? new Size(Length, Width)
            : new Size(Width, Length);

        if (size.Width <= context.AvailableSpace.Width && size.Height <= context.AvailableSpace.Height)
            return SpacePlan.FullRender(size);

        return SpacePlan.Wrap();
    }

    public void Render(RenderContext context)
    {
        double x1 = context.Position.X;
        double y1 = context.Position.Y;
        double x2 = IsHorizontal ? x1 + Length : x1;
        double y2 = IsHorizontal ? y1 : y1 + Length;

        context.Editor
            .WithLineWidth(Width)
            .Line(x1, y1, x2, y2, Color);
    }
}
