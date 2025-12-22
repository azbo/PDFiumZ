using System;
namespace PDFiumZ.Fluent.Elements.Positional;

/// <summary>
/// Extends child size to fit the entire content area.
/// </summary>
public class ExtendElement : Container
{
    public bool ExtendHorizontal { get; set; } = true;
    public bool ExtendVertical { get; set; } = true;

    public override SpacePlan Measure(MeasureContext context)
    {
        if (Child == null)
            return SpacePlan.FullRender(context.AvailableSpace);

        var childPlan = Child.Measure(context);

        if (childPlan.Type == SpacePlanType.Wrap)
            return SpacePlan.Wrap();

        var width = ExtendHorizontal ? context.AvailableSpace.Width : childPlan.Size.Width;
        var height = ExtendVertical ? context.AvailableSpace.Height : childPlan.Size.Height;

        return new SpacePlan
        {
            Type = childPlan.Type,
            Size = new Size(width, height)
        };
    }
}
