using PDFiumZ.HighLevel;

namespace PDFiumZ.Fluent.Elements.Visual;

/// <summary>
/// Renders text content.
/// </summary>
public class TextElement : IElement
{
    public string Content { get; set; } = string.Empty;
    public PdfFont? Font { get; set; }
    public double FontSize { get; set; } = 12;
    public uint Color { get; set; } = PdfColor.Black;

    public TextElement(string content)
    {
        Content = content;
    }

    public SpacePlan Measure(MeasureContext context)
    {
        var font = Font ?? context.DefaultFont;
        var fontSize = FontSize > 0 ? FontSize : context.DefaultFontSize;

        // Simple text measurement - estimate width based on character count
        // In a real implementation, you'd use font metrics
        double estimatedWidth = Content.Length * fontSize * 0.6;
        double height = fontSize * 1.2; // Line height

        var size = new Size(estimatedWidth, height);

        if (size.Width <= context.AvailableSpace.Width && size.Height <= context.AvailableSpace.Height)
            return SpacePlan.FullRender(size);

        if (size.Height <= context.AvailableSpace.Height)
            return SpacePlan.PartialRender(new Size(context.AvailableSpace.Width, height));

        return SpacePlan.Wrap();
    }

    public void Render(RenderContext context)
    {
        var font = Font ?? context.DefaultFont;
        var fontSize = FontSize > 0 ? FontSize : context.DefaultFontSize;

        context.Editor
            .WithFont(font)
            .WithFontSize(fontSize)
            .WithTextColor(Color)
            .Text(Content, context.Position.X, context.Position.Y);
    }
}
