using PDFiumZ.HighLevel;

namespace PDFiumZ.Fluent.Elements.Visual;

/// <summary>
/// Footer element that displays page numbers.
/// </summary>
public class FooterElement : IElement
{
    public string Template { get; set; } = "第 {page} 页 / 共 {total} 页";
    public PdfFont? Font { get; set; }
    public double FontSize { get; set; } = 10;
    public uint Color { get; set; } = 0xFF666666; // Gray
    public double Margin { get; set; } = 20;

    public SpacePlan Measure(MeasureContext context)
    {
        var font = Font ?? context.DefaultFont;
        var fontSize = FontSize > 0 ? FontSize : context.DefaultFontSize;

        // Estimate footer height
        double height = fontSize * 1.5 + Margin;
        var size = new Size(context.AvailableSpace.Width, height);

        return SpacePlan.FullRender(size);
    }

    public void Render(RenderContext context)
    {
        var font = Font ?? context.DefaultFont;
        var fontSize = FontSize > 0 ? FontSize : context.DefaultFontSize;

        // Calculate footer position at bottom of page
        double y = context.Position.Y - context.AvailableSpace.Height + Margin;
        double x = context.Position.X + context.AvailableSpace.Width / 2;

        // Note: In real implementation, you'd need to track current page number
        // For now, render a template
        var text = Template
            .Replace("{page}", "1")
            .Replace("{total}", "1");

        context.Editor
            .WithFont(font)
            .WithFontSize(fontSize)
            .WithTextColor(Color)
            .Text(text, x - text.Length * fontSize * 0.3, y); // Center-align
    }
}
