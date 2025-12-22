using System.Collections.Generic;

namespace PDFiumZ.Fluent.Elements;

/// <summary>
/// Base class for container elements that hold a single child.
/// </summary>
public abstract class Container : IElement
{
    public IElement? Child { get; set; }

    public virtual SpacePlan Measure(MeasureContext context)
    {
        if (Child == null)
            return SpacePlan.FullRender(Size.Zero);

        return Child.Measure(context);
    }

    public virtual void Render(RenderContext context)
    {
        Child?.Render(context);
    }
}

/// <summary>
/// Base class for container elements that hold multiple children.
/// </summary>
public abstract class MultiContainer : IElement
{
    protected List<IElement> Children { get; } = new();

    public abstract SpacePlan Measure(MeasureContext context);
    public abstract void Render(RenderContext context);
}
