using PDFiumZ.HighLevel;

namespace PDFiumZ.Fluent.Elements.Visual;

/// <summary>
/// Applies a background color to its child.
/// </summary>
public class BackgroundElement : Container
{
    public uint Color { get; set; } = PdfColor.White;

    public BackgroundElement(uint color)
    {
        Color = color;
    }

    public override void Render(RenderContext context)
    {
        if (Child == null) return;

        // Measure child to get size
        var childPlan = Child.Measure(new MeasureContext
        {
            AvailableSpace = context.AvailableSpace,
            Document = context.Document,
            DefaultFont = context.DefaultFont,
            DefaultFontSize = context.DefaultFontSize
        });

        // Draw background rectangle
        var rect = new PdfRectangle(
            context.Position.X,
            context.Position.Y,
            childPlan.Size.Width,
            childPlan.Size.Height);

        context.Editor
            .WithFillColor(Color)
            .WithStrokeColor(PdfColor.Transparent)
            .Rectangle(rect);

        // Render child on top
        base.Render(context);
    }
}
