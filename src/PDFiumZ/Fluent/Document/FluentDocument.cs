using System;
using System.Collections.Generic;
using PDFiumZ.HighLevel;
using PDFiumZ.Fluent.Elements;

namespace PDFiumZ.Fluent.Document;

/// <summary>
/// Fluent document builder for creating PDFs using QuestPDF-style API.
/// </summary>
public class FluentDocument : IDisposable
{
    private readonly PdfDocument _document;
    private readonly List<IElement> _content = new();
    private PdfFont? _defaultFont;
    private double _defaultFontSize = 12;
    private bool _disposed;

    public PdfDocument UnderlyingDocument => _document;

    public FluentDocument()
    {
        _document = PdfDocument.CreateNew();
    }

    /// <summary>
    /// Sets the default font for the document.
    /// </summary>
    public FluentDocument WithFont(PdfFont font)
    {
        _defaultFont = font;
        return this;
    }

    /// <summary>
    /// Sets the default font size for the document.
    /// </summary>
    public FluentDocument WithFontSize(double fontSize)
    {
        _defaultFontSize = fontSize;
        return this;
    }

    /// <summary>
    /// Adds content to the document using a builder pattern.
    /// </summary>
    public FluentDocument Content(Action<IContainer> builder)
    {
        var container = new DocumentContainer();
        builder(container);
        _content.Add(container.GetElement());
        return this;
    }

    /// <summary>
    /// Generates the PDF document.
    /// </summary>
    public void Generate(double pageWidth = 595, double pageHeight = 842)
    {
        if (_defaultFont == null)
        {
            _defaultFont = PdfFont.Load(_document, PdfStandardFont.Helvetica);
        }

        var pageSize = new Size(pageWidth, pageHeight);

        foreach (var element in _content)
        {
            RenderElement(element, pageSize);
        }
    }

    private void RenderElement(IElement element, Size pageSize)
    {
        // Create a new page
        using var page = _document.CreatePage(pageSize.Width, pageSize.Height);
        using var editor = page.BeginEdit();

        var measureContext = new MeasureContext
        {
            AvailableSpace = pageSize,
            Document = _document,
            DefaultFont = _defaultFont!,
            DefaultFontSize = _defaultFontSize
        };

        var spacePlan = element.Measure(measureContext);

        if (spacePlan.Type != SpacePlanType.Wrap)
        {
            var renderContext = new RenderContext
            {
                Editor = editor,
                Position = new Position(0, pageSize.Height), // PDF coordinates start from bottom
                AvailableSpace = pageSize,
                Document = _document,
                DefaultFont = _defaultFont!,
                DefaultFontSize = _defaultFontSize
            };

            element.Render(renderContext);
        }

        editor.GenerateContent();
    }

    /// <summary>
    /// Saves the generated document to a file.
    /// </summary>
    public void Save(string filePath)
    {
        _document.Save(filePath);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _defaultFont?.Dispose();
        _document?.Dispose();
        _disposed = true;
    }
}

/// <summary>
/// Container for building document content.
/// </summary>
public interface IContainer
{
    void Text(string text);
    void Column(Action<IColumnContainer> builder);
    void Row(Action<IRowContainer> builder);
    void Padding(double all, Action<IContainer> content);
    void Background(uint color, Action<IContainer> content);
    void Border(uint color, double width, Action<IContainer> content);
}

public interface IColumnContainer
{
    void Item(Action<IContainer> content);
}

public interface IRowContainer
{
    void Item(Action<IContainer> content);
}

internal class DocumentContainer : IContainer
{
    private IElement? _element;

    public IElement GetElement() => _element ?? new Elements.EmptyElement();

    public void Text(string text)
    {
        _element = new Elements.Visual.TextElement(text);
    }

    public void Column(Action<IColumnContainer> builder)
    {
        var column = new ColumnContainer();
        builder(column);
        _element = column.GetElement();
    }

    public void Row(Action<IRowContainer> builder)
    {
        var row = new RowContainer();
        builder(row);
        _element = row.GetElement();
    }

    public void Padding(double all, Action<IContainer> content)
    {
        var innerContainer = new DocumentContainer();
        content(innerContainer);

        var padding = new Elements.Positional.PaddingElement(all);
        padding.Child = innerContainer.GetElement();
        _element = padding;
    }

    public void Background(uint color, Action<IContainer> content)
    {
        var innerContainer = new DocumentContainer();
        content(innerContainer);

        var background = new Elements.Visual.BackgroundElement(color);
        background.Child = innerContainer.GetElement();
        _element = background;
    }

    public void Border(uint color, double width, Action<IContainer> content)
    {
        var innerContainer = new DocumentContainer();
        content(innerContainer);

        var border = new Elements.Visual.BorderElement(color, width);
        border.Child = innerContainer.GetElement();
        _element = border;
    }
}

internal class ColumnContainer : IColumnContainer
{
    private readonly Elements.Layout.ColumnElement _column = new();

    public IElement GetElement() => _column;

    public void Item(Action<IContainer> content)
    {
        var container = new DocumentContainer();
        content(container);
        _column.AddChild(container.GetElement());
    }
}

internal class RowContainer : IRowContainer
{
    private readonly Elements.Layout.RowElement _row = new();

    public IElement GetElement() => _row;

    public void Item(Action<IContainer> content)
    {
        var container = new DocumentContainer();
        content(container);
        _row.AddChild(container.GetElement());
    }
}
