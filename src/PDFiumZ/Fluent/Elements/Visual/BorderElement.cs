using System;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Fluent.Elements.Visual;

/// <summary>
/// Applies a border around its child.
/// </summary>
public class BorderElement : Container
{
    public uint Color { get; set; } = PdfColor.Black;
    public double Width { get; set; } = 1.0;

    public BorderElement(uint color, double width = 1.0)
    {
        Color = color;
        Width = width;
    }

    public override SpacePlan Measure(MeasureContext context)
    {
        if (Child == null)
            return SpacePlan.FullRender(new Size(Width * 2, Width * 2));

        // Available space minus border width on all sides
        var innerSpace = new Size(
            Math.Max(0, context.AvailableSpace.Width - Width * 2),
            Math.Max(0, context.AvailableSpace.Height - Width * 2));

        var childContext = context.Clone();
        childContext.AvailableSpace = innerSpace;
        var childPlan = Child.Measure(childContext);

        if (childPlan.Type == SpacePlanType.Wrap)
            return SpacePlan.Wrap();

        var totalSize = new Size(
            childPlan.Size.Width + Width * 2,
            childPlan.Size.Height + Width * 2);

        return SpacePlan.FullRender(totalSize);
    }

    public override void Render(RenderContext context)
    {
        if (Child == null) return;

        // Measure child to get size
        var innerSpace = new Size(
            Math.Max(0, context.AvailableSpace.Width - Width * 2),
            Math.Max(0, context.AvailableSpace.Height - Width * 2));

        var childPlan = Child.Measure(new MeasureContext
        {
            AvailableSpace = innerSpace,
            Document = context.Document,
            DefaultFont = context.DefaultFont,
            DefaultFontSize = context.DefaultFontSize
        });

        // Draw border rectangle
        var rect = new PdfRectangle(
            context.Position.X,
            context.Position.Y,
            childPlan.Size.Width + Width * 2,
            childPlan.Size.Height + Width * 2);

        context.Editor
            .WithStrokeColor(Color)
            .WithFillColor(PdfColor.Transparent)
            .WithLineWidth(Width)
            .Rectangle(rect);

        // Render child with padding
        var childContext = context.Clone();
        childContext.Position = new Position(context.Position.X + Width, context.Position.Y + Width);
        childContext.AvailableSpace = innerSpace;

        Child.Render(childContext);
    }
}
