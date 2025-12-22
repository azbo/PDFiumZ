namespace PDFiumZ.Fluent.Elements;

/// <summary>
/// Base interface for all fluent document elements.
/// </summary>
public interface IElement
{
    /// <summary>
    /// Measures the element and returns a space plan indicating how it fits in the available space.
    /// </summary>
    SpacePlan Measure(MeasureContext context);

    /// <summary>
    /// Renders the element at the specified position.
    /// </summary>
    void Render(RenderContext context);
}
