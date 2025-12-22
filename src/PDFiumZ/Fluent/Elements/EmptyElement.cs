namespace PDFiumZ.Fluent.Elements;

/// <summary>
/// Represents an empty element that takes no space.
/// </summary>
public class EmptyElement : IElement
{
    public SpacePlan Measure(MeasureContext context)
    {
        return SpacePlan.FullRender(Size.Zero);
    }

    public void Render(RenderContext context)
    {
        // Do nothing
    }
}
