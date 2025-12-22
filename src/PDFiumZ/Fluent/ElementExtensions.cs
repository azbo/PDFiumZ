using System;
using PDFiumZ.HighLevel;
using PDFiumZ.Fluent.Elements;
using PDFiumZ.Fluent.Elements.Visual;
using PDFiumZ.Fluent.Elements.Layout;
using PDFiumZ.Fluent.Elements.Positional;
using PDFiumZ.Fluent.Elements.ContentFlow;

namespace PDFiumZ.Fluent;

/// <summary>
/// Extension methods for fluent API composition.
/// </summary>
public static class ElementExtensions
{
    // Visual Elements

    public static TextElement Text(this string content)
    {
        return new TextElement(content);
    }

    public static TextElement WithFont(this TextElement element, PdfFont font)
    {
        element.Font = font;
        return element;
    }

    public static TextElement WithFontSize(this TextElement element, double fontSize)
    {
        element.FontSize = fontSize;
        return element;
    }

    public static TextElement WithColor(this TextElement element, uint color)
    {
        element.Color = color;
        return element;
    }

    public static BackgroundElement Background(this IElement element, uint color)
    {
        return new BackgroundElement(color) { Child = element };
    }

    public static BorderElement Border(this IElement element, uint color, double width = 1.0)
    {
        return new BorderElement(color, width) { Child = element };
    }

    // Positional Elements

    public static PaddingElement Padding(this IElement element, double all)
    {
        return new PaddingElement(all) { Child = element };
    }

    public static PaddingElement Padding(this IElement element, double horizontal, double vertical)
    {
        return new PaddingElement(horizontal, vertical) { Child = element };
    }

    public static PaddingElement Padding(this IElement element, double left, double top, double right, double bottom)
    {
        return new PaddingElement(left, top, right, bottom) { Child = element };
    }

    public static WidthElement Width(this IElement element, double? minWidth = null, double? maxWidth = null)
    {
        return new WidthElement(minWidth, maxWidth) { Child = element };
    }

    public static HeightElement Height(this IElement element, double? minHeight = null, double? maxHeight = null)
    {
        return new HeightElement(minHeight, maxHeight) { Child = element };
    }

    public static AlignmentElement AlignCenter(this IElement element)
    {
        return new AlignmentElement(HorizontalAlignment.Center, VerticalAlignment.Middle) { Child = element };
    }

    public static AlignmentElement AlignLeft(this IElement element)
    {
        return new AlignmentElement(HorizontalAlignment.Left, VerticalAlignment.Top) { Child = element };
    }

    public static AlignmentElement AlignRight(this IElement element)
    {
        return new AlignmentElement(HorizontalAlignment.Right, VerticalAlignment.Top) { Child = element };
    }

    public static AspectRatioElement AspectRatio(this IElement element, double ratio)
    {
        return new AspectRatioElement(ratio) { Child = element };
    }

    // Layout Elements

    public static ColumnElement Column(params IElement[] children)
    {
        var column = new ColumnElement();
        foreach (var child in children)
        {
            column.AddChild(child);
        }
        return column;
    }

    public static ColumnElement Column(double spacing, params IElement[] children)
    {
        var column = new ColumnElement { Spacing = spacing };
        foreach (var child in children)
        {
            column.AddChild(child);
        }
        return column;
    }

    public static RowElement Row(params IElement[] children)
    {
        var row = new RowElement();
        foreach (var child in children)
        {
            row.AddChild(child);
        }
        return row;
    }

    public static RowElement Row(double spacing, params IElement[] children)
    {
        var row = new RowElement { Spacing = spacing };
        foreach (var child in children)
        {
            row.AddChild(child);
        }
        return row;
    }

    public static LayersElement Layers(params IElement[] children)
    {
        var layers = new LayersElement();
        foreach (var child in children)
        {
            layers.AddChild(child);
        }
        return layers;
    }

    // Content Flow Elements

    public static ShowIfElement ShowIf(this IElement element, bool condition)
    {
        return new ShowIfElement(() => condition) { Child = element };
    }

    public static ShowIfElement ShowIf(this IElement element, Func<bool> condition)
    {
        return new ShowIfElement(condition) { Child = element };
    }

    public static PageBreakElement PageBreak()
    {
        return new PageBreakElement();
    }
}
