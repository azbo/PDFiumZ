namespace PDFiumZ.Fluent.Elements.ContentFlow;

/// <summary>
/// Forces all subsequent content to be moved to the next page.
/// </summary>
public class PageBreakElement : IElement
{
    public SpacePlan Measure(MeasureContext context)
    {
        return SpacePlan.Wrap();
    }

    public void Render(RenderContext context)
    {
        // PageBreak doesn't render anything, it just signals a wrap
    }
}
