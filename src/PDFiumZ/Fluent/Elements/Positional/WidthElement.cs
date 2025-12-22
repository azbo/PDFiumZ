using System;
namespace PDFiumZ.Fluent.Elements.Positional;

/// <summary>
/// Applies width constraints to its child.
/// </summary>
public class WidthElement : Container
{
    public double? MinWidth { get; set; }
    public double? MaxWidth { get; set; }

    public WidthElement(double? minWidth, double? maxWidth)
    {
        MinWidth = minWidth;
        MaxWidth = maxWidth;
    }

    public override SpacePlan Measure(MeasureContext context)
    {
        if (Child == null)
            return SpacePlan.FullRender(Size.Zero);

        double availableWidth = context.AvailableSpace.Width;

        if (MaxWidth.HasValue)
            availableWidth = Math.Min(availableWidth, MaxWidth.Value);

        var childContext = context.Clone();
        childContext.AvailableSpace = new Size(availableWidth, context.AvailableSpace.Height);

        var childPlan = Child.Measure(childContext);

        if (childPlan.Type == SpacePlanType.Wrap)
            return SpacePlan.Wrap();

        double finalWidth = childPlan.Size.Width;

        if (MinWidth.HasValue)
            finalWidth = Math.Max(finalWidth, MinWidth.Value);
        if (MaxWidth.HasValue)
            finalWidth = Math.Min(finalWidth, MaxWidth.Value);

        return new SpacePlan
        {
            Type = childPlan.Type,
            Size = new Size(finalWidth, childPlan.Size.Height)
        };
    }
}
