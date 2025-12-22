using System;
namespace PDFiumZ.Fluent.Elements.Layout;

/// <summary>
/// Layers multiple elements on top of each other.
/// </summary>
public class LayersElement : MultiContainer
{
    public override SpacePlan Measure(MeasureContext context)
    {
        if (Children.Count == 0)
            return SpacePlan.FullRender(Size.Zero);

        double maxWidth = 0;
        double maxHeight = 0;

        foreach (var child in Children)
        {
            var childPlan = child.Measure(context);

            if (childPlan.Type == SpacePlanType.Wrap)
                return SpacePlan.Wrap();

            maxWidth = Math.Max(maxWidth, childPlan.Size.Width);
            maxHeight = Math.Max(maxHeight, childPlan.Size.Height);
        }

        return SpacePlan.FullRender(new Size(maxWidth, maxHeight));
    }

    public override void Render(RenderContext context)
    {
        // Render all children at the same position (layered)
        foreach (var child in Children)
        {
            child.Render(context);
        }
    }

    public void AddChild(IElement element)
    {
        Children.Add(element);
    }
}
