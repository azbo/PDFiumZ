using System;
namespace PDFiumZ.Fluent.Elements.Positional;

/// <summary>
/// Applies padding around its child.
/// </summary>
public class PaddingElement : Container
{
    public double Left { get; set; }
    public double Right { get; set; }
    public double Top { get; set; }
    public double Bottom { get; set; }

    public PaddingElement(double all)
    {
        Left = Right = Top = Bottom = all;
    }

    public PaddingElement(double horizontal, double vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;
    }

    public PaddingElement(double left, double top, double right, double bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public override SpacePlan Measure(MeasureContext context)
    {
        if (Child == null)
            return SpacePlan.FullRender(new Size(Left + Right, Top + Bottom));

        var innerSpace = new Size(
            Math.Max(0, context.AvailableSpace.Width - Left - Right),
            Math.Max(0, context.AvailableSpace.Height - Top - Bottom));

        var childContext = context.Clone();
        childContext.AvailableSpace = innerSpace;
        var childPlan = Child.Measure(childContext);

        if (childPlan.Type == SpacePlanType.Wrap)
            return SpacePlan.Wrap();

        var totalSize = new Size(
            childPlan.Size.Width + Left + Right,
            childPlan.Size.Height + Top + Bottom);

        return new SpacePlan
        {
            Type = childPlan.Type,
            Size = totalSize
        };
    }

    public override void Render(RenderContext context)
    {
        if (Child == null) return;

        var childContext = context.Clone();
        childContext.Position = new Position(context.Position.X + Left, context.Position.Y + Top);
        childContext.AvailableSpace = new Size(
            Math.Max(0, context.AvailableSpace.Width - Left - Right),
            Math.Max(0, context.AvailableSpace.Height - Top - Bottom));

        Child.Render(childContext);
    }
}
