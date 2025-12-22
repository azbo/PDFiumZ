using System;
namespace PDFiumZ.Fluent.Elements.Layout;

/// <summary>
/// Places children vertically, one under another.
/// </summary>
public class ColumnElement : MultiContainer
{
    public double Spacing { get; set; } = 0;

    public override SpacePlan Measure(MeasureContext context)
    {
        if (Children.Count == 0)
            return SpacePlan.FullRender(Size.Zero);

        double totalHeight = 0;
        double maxWidth = 0;
        double availableHeight = context.AvailableSpace.Height;

        for (int i = 0; i < Children.Count; i++)
        {
            if (i > 0)
                totalHeight += Spacing;

            var childContext = context.Clone();
            childContext.AvailableSpace = new Size(context.AvailableSpace.Width, availableHeight - totalHeight);

            var childPlan = Children[i].Measure(childContext);

            if (childPlan.Type == SpacePlanType.Wrap && i == 0)
                return SpacePlan.Wrap();

            if (childPlan.Type == SpacePlanType.Wrap)
                break; // Stop at first wrapping child

            totalHeight += childPlan.Size.Height;
            maxWidth = Math.Max(maxWidth, childPlan.Size.Width);

            if (totalHeight > availableHeight)
                return SpacePlan.PartialRender(new Size(maxWidth, availableHeight));
        }

        return SpacePlan.FullRender(new Size(maxWidth, totalHeight));
    }

    public override void Render(RenderContext context)
    {
        double currentY = context.Position.Y;
        double availableHeight = context.AvailableSpace.Height;

        for (int i = 0; i < Children.Count; i++)
        {
            if (i > 0)
                currentY += Spacing;

            if (currentY >= context.Position.Y + availableHeight)
                break;

            var childContext = context.Clone();
            childContext.Position = new Position(context.Position.X, currentY);
            childContext.AvailableSpace = new Size(context.AvailableSpace.Width, availableHeight - (currentY - context.Position.Y));

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
            currentY += childPlan.Size.Height;
        }
    }

    public void AddChild(IElement element)
    {
        Children.Add(element);
    }
}
