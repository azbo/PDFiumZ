using System;
namespace PDFiumZ.Fluent.Elements.Positional;

/// <summary>
/// Horizontal alignment options.
/// </summary>
public enum HorizontalAlignment
{
    Left,
    Center,
    Right
}

/// <summary>
/// Vertical alignment options.
/// </summary>
public enum VerticalAlignment
{
    Top,
    Middle,
    Bottom
}

/// <summary>
/// Aligns its child within the available space.
/// </summary>
public class AlignmentElement : Container
{
    public HorizontalAlignment Horizontal { get; set; }
    public VerticalAlignment Vertical { get; set; }

    public AlignmentElement(HorizontalAlignment horizontal, VerticalAlignment vertical)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public override void Render(RenderContext context)
    {
        if (Child == null) return;

        var childPlan = Child.Measure(new MeasureContext
        {
            AvailableSpace = context.AvailableSpace,
            Document = context.Document,
            DefaultFont = context.DefaultFont,
            DefaultFontSize = context.DefaultFontSize
        });

        double x = context.Position.X;
        double y = context.Position.Y;

        // Horizontal alignment
        switch (Horizontal)
        {
            case HorizontalAlignment.Center:
                x += (context.AvailableSpace.Width - childPlan.Size.Width) / 2;
                break;
            case HorizontalAlignment.Right:
                x += context.AvailableSpace.Width - childPlan.Size.Width;
                break;
        }

        // Vertical alignment
        switch (Vertical)
        {
            case VerticalAlignment.Middle:
                y += (context.AvailableSpace.Height - childPlan.Size.Height) / 2;
                break;
            case VerticalAlignment.Bottom:
                y += context.AvailableSpace.Height - childPlan.Size.Height;
                break;
        }

        var childContext = context.Clone();
        childContext.Position = new Position(x, y);

        Child.Render(childContext);
    }
}
