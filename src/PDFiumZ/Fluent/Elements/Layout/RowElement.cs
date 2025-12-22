using System;
namespace PDFiumZ.Fluent.Elements.Layout;

/// <summary>
/// Places children horizontally, one alongside another.
/// </summary>
public class RowElement : MultiContainer
{
    public double Spacing { get; set; } = 0;

    public override SpacePlan Measure(MeasureContext context)
    {
        if (Children.Count == 0)
            return SpacePlan.FullRender(Size.Zero);

        double totalWidth = 0;
        double maxHeight = 0;
        double availableWidth = context.AvailableSpace.Width;

        for (int i = 0; i < Children.Count; i++)
        {
            if (i > 0)
                totalWidth += Spacing;

            var childContext = context.Clone();
            childContext.AvailableSpace = new Size(availableWidth - totalWidth, context.AvailableSpace.Height);

            var childPlan = Children[i].Measure(childContext);

            if (childPlan.Type == SpacePlanType.Wrap && i == 0)
                return SpacePlan.Wrap();

            if (childPlan.Type == SpacePlanType.Wrap)
                break; // Stop at first wrapping child

            totalWidth += childPlan.Size.Width;
            maxHeight = Math.Max(maxHeight, childPlan.Size.Height);

            if (totalWidth > availableWidth)
                return SpacePlan.PartialRender(new Size(availableWidth, maxHeight));
        }

        return SpacePlan.FullRender(new Size(totalWidth, maxHeight));
    }

    public override void Render(RenderContext context)
    {
        double currentX = context.Position.X;
        double availableWidth = context.AvailableSpace.Width;

        for (int i = 0; i < Children.Count; i++)
        {
            if (i > 0)
                currentX += Spacing;

            if (currentX >= context.Position.X + availableWidth)
                break;

            var childContext = context.Clone();
            childContext.Position = new Position(currentX, context.Position.Y);
            childContext.AvailableSpace = new Size(availableWidth - (currentX - context.Position.X), context.AvailableSpace.Height);

            var childPlan = Children[i].Measure(new MeasureContext
            {
                AvailableSpace = childContext.AvailableSpace,
                Document = context.Document,
                DefaultFont = context.DefaultFont,
                DefaultFontSize = context.DefaultFontSize
            });

            if (childPlan.Type == SpacePlanType.Wrap)
                break;

            Children[i].Render(childContext);
            currentX += childPlan.Size.Width;
        }
    }

    public void AddChild(IElement element)
    {
        Children.Add(element);
    }
}
