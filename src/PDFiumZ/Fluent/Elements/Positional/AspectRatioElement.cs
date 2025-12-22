using System;
namespace PDFiumZ.Fluent.Elements.Positional;

/// <summary>
/// Reserves an area with a specified aspect ratio.
/// </summary>
public class AspectRatioElement : Container
{
    public double AspectRatio { get; set; } // width / height

    public AspectRatioElement(double aspectRatio)
    {
        AspectRatio = aspectRatio;
    }

    public override SpacePlan Measure(MeasureContext context)
    {
        double width = context.AvailableSpace.Width;
        double height = width / AspectRatio;

        if (height > context.AvailableSpace.Height)
        {
            height = context.AvailableSpace.Height;
            width = height * AspectRatio;
        }

        if (Child != null)
        {
            var childContext = context.Clone();
            childContext.AvailableSpace = new Size(width, height);

            var childPlan = Child.Measure(childContext);
            if (childPlan.Type == SpacePlanType.Wrap)
                return SpacePlan.Wrap();
        }

        return SpacePlan.FullRender(new Size(width, height));
    }

    public override void Render(RenderContext context)
    {
        if (Child == null) return;

        double width = context.AvailableSpace.Width;
        double height = width / AspectRatio;

        if (height > context.AvailableSpace.Height)
        {
            height = context.AvailableSpace.Height;
            width = height * AspectRatio;
        }

        var childContext = context.Clone();
        childContext.AvailableSpace = new Size(width, height);

        Child.Render(childContext);
    }
}
