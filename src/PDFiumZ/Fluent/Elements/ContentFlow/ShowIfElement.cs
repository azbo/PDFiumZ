using System;

namespace PDFiumZ.Fluent.Elements.ContentFlow;

/// <summary>
/// Conditionally displays its children based on a predicate.
/// </summary>
public class ShowIfElement : Container
{
    public Func<bool> Condition { get; set; }

    public ShowIfElement(Func<bool> condition)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
    }

    public override SpacePlan Measure(MeasureContext context)
    {
        if (!Condition() || Child == null)
            return SpacePlan.FullRender(Size.Zero);

        return Child.Measure(context);
    }

    public override void Render(RenderContext context)
    {
        if (!Condition() || Child == null)
            return;

        Child.Render(context);
    }
}
