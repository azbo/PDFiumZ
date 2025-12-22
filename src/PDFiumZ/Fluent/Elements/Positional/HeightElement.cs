using System;
namespace PDFiumZ.Fluent.Elements.Positional;

/// <summary>
/// Applies height constraints to its child.
/// </summary>
public class HeightElement : Container
{
    public double? MinHeight { get; set; }
    public double? MaxHeight { get; set; }

    public HeightElement(double? minHeight, double? maxHeight)
    {
        MinHeight = minHeight;
        MaxHeight = maxHeight;
    }

    public override SpacePlan Measure(MeasureContext context)
    {
        if (Child == null)
            return SpacePlan.FullRender(Size.Zero);

        double availableHeight = context.AvailableSpace.Height;

        if (MaxHeight.HasValue)
            availableHeight = Math.Min(availableHeight, MaxHeight.Value);

        var childContext = context.Clone();
        childContext.AvailableSpace = new Size(context.AvailableSpace.Width, availableHeight);

        var childPlan = Child.Measure(childContext);

        if (childPlan.Type == SpacePlanType.Wrap)
            return SpacePlan.Wrap();

        double finalHeight = childPlan.Size.Height;

        if (MinHeight.HasValue)
            finalHeight = Math.Max(finalHeight, MinHeight.Value);
        if (MaxHeight.HasValue)
            finalHeight = Math.Min(finalHeight, MaxHeight.Value);

        return new SpacePlan
        {
            Type = childPlan.Type,
            Size = new Size(childPlan.Size.Width, finalHeight)
        };
    }
}
