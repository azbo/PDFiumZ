using System;
using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Fluent API builder for creating PDF tables.
/// </summary>
public sealed class PdfTableBuilder
{
    private readonly PdfContentEditor _editor;
    private PdfColumnDefinition? _columnDefinition;
    private PdfTableRowBuilder? _headerRow;
    private readonly List<PdfTableRowBuilder> _dataRows = new();

    // Table styling
    private double _cellPadding = 5;
    private double _borderWidth = 1;
    private uint _borderColor = PdfColor.Black;
    private uint _headerBackgroundColor = PdfColor.Transparent;
    private uint _headerTextColor = PdfColor.Black;
    private double _headerFontSize = 12;
    private bool _headerBold = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfTableBuilder"/> class.
    /// </summary>
    /// <param name="editor">The content editor to render the table on.</param>
    internal PdfTableBuilder(PdfContentEditor editor)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
    }

    /// <summary>
    /// Defines table columns using a configuration action.
    /// </summary>
    /// <param name="configureColumns">Action to configure columns.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableBuilder Columns(Action<PdfColumnDefinition> configureColumns)
    {
        if (configureColumns is null)
            throw new ArgumentNullException(nameof(configureColumns));

        _columnDefinition = new PdfColumnDefinition();
        configureColumns(_columnDefinition);
        return this;
    }

    /// <summary>
    /// Defines the table header row.
    /// </summary>
    /// <param name="configureHeader">Action to configure header cells.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableBuilder Header(Action<PdfTableRowBuilder> configureHeader)
    {
        if (configureHeader is null)
            throw new ArgumentNullException(nameof(configureHeader));

        _headerRow = new PdfTableRowBuilder(isHeader: true);
        configureHeader(_headerRow);
        return this;
    }

    /// <summary>
    /// Adds a data row to the table.
    /// </summary>
    /// <param name="configureRow">Action to configure row cells.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableBuilder Row(Action<PdfTableRowBuilder> configureRow)
    {
        if (configureRow is null)
            throw new ArgumentNullException(nameof(configureRow));

        var row = new PdfTableRowBuilder(isHeader: false);
        configureRow(row);
        _dataRows.Add(row);
        return this;
    }

    /// <summary>
    /// Sets the cell padding in points.
    /// </summary>
    /// <param name="padding">Padding in points (default: 5).</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableBuilder CellPadding(double padding)
    {
        if (padding < 0)
            throw new ArgumentOutOfRangeException(nameof(padding), "Cell padding cannot be negative.");

        _cellPadding = padding;
        return this;
    }

    /// <summary>
    /// Sets the border width in points.
    /// </summary>
    /// <param name="width">Border width in points (default: 1).</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableBuilder BorderWidth(double width)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Border width cannot be negative.");

        _borderWidth = width;
        return this;
    }

    /// <summary>
    /// Sets the border color.
    /// </summary>
    /// <param name="color">Border color in ARGB format (default: Black).</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableBuilder BorderColor(uint color)
    {
        _borderColor = color;
        return this;
    }

    /// <summary>
    /// Sets the header background color.
    /// </summary>
    /// <param name="color">Header background color in ARGB format.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableBuilder HeaderBackgroundColor(uint color)
    {
        _headerBackgroundColor = color;
        return this;
    }

    /// <summary>
    /// Sets the header text color.
    /// </summary>
    /// <param name="color">Header text color in ARGB format (default: Black).</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableBuilder HeaderTextColor(uint color)
    {
        _headerTextColor = color;
        return this;
    }

    /// <summary>
    /// Sets the header font size.
    /// </summary>
    /// <param name="fontSize">Font size in points (default: 12).</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableBuilder HeaderFontSize(double fontSize)
    {
        if (fontSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be positive.");

        _headerFontSize = fontSize;
        return this;
    }

    /// <summary>
    /// Sets whether header text should be bold.
    /// </summary>
    /// <param name="bold">True for bold header text (default: true).</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableBuilder HeaderBold(bool bold = true)
    {
        _headerBold = bold;
        return this;
    }

    /// <summary>
    /// Renders the table and returns the content editor for further operations.
    /// </summary>
    /// <returns>The content editor instance for fluent chaining.</returns>
    public PdfContentEditor EndTable()
    {
        // Validate table configuration
        if (_columnDefinition == null || _columnDefinition.Count == 0)
            throw new InvalidOperationException("Table must have at least one column. Call Columns() first.");

        // Get page dimensions for width calculation
        var page = _editor.Page;
        double availableWidth = page.Width - 100; // Assume 50pt margins on each side

        // Calculate column widths
        var columnWidths = _columnDefinition.CalculateWidths(availableWidth);

        // Calculate table dimensions
        double tableWidth = 0;
        foreach (var width in columnWidths)
            tableWidth += width;
        tableWidth += (columnWidths.Length + 1) * _borderWidth;

        double tableX = 50; // Left margin
        double currentY = page.Height - 100; // Start position (with top margin)

        // Render header row if exists
        if (_headerRow != null)
        {
            currentY = RenderRow(_headerRow, columnWidths, tableX, currentY, isHeader: true);
        }

        // Render data rows
        foreach (var row in _dataRows)
        {
            currentY = RenderRow(row, columnWidths, tableX, currentY, isHeader: false);
        }

        // Draw bottom border
        _editor.Line(tableX, currentY, tableX + tableWidth, currentY, _borderColor, _borderWidth);

        return _editor;
    }

    private double RenderRow(PdfTableRowBuilder row, double[] columnWidths, double tableX, double currentY, bool isHeader)
    {
        // Get font for this row
        var font = isHeader && _headerBold
            ? PdfFont.LoadStandardFont(_editor.Page._document, PdfStandardFont.HelveticaBold)
            : PdfFont.LoadStandardFont(_editor.Page._document, PdfStandardFont.Helvetica);

        var fontSize = isHeader ? _headerFontSize : 12;
        var textColor = isHeader ? _headerTextColor : PdfColor.Black;

        // Calculate row height
        double rowHeight = fontSize + 2 * _cellPadding;

        // Draw cells
        double currentX = tableX;
        int cellIndex = 0;

        foreach (var cellText in row.Cells)
        {
            if (cellIndex >= columnWidths.Length)
                break; // Skip extra cells

            double cellWidth = columnWidths[cellIndex];

            // Draw cell border
            var cellBounds = new PdfRectangle(
                currentX,
                currentY - rowHeight,
                cellWidth + _borderWidth,
                rowHeight
            );

            // Draw background for header
            if (isHeader && _headerBackgroundColor != PdfColor.Transparent)
            {
                _editor.Rectangle(cellBounds, _borderColor, _headerBackgroundColor);
            }
            else
            {
                _editor.Rectangle(cellBounds, _borderColor, PdfColor.Transparent);
            }

            // Draw cell text
            if (!string.IsNullOrWhiteSpace(cellText))
            {
                double textX = currentX + _borderWidth + _cellPadding;
                double textY = currentY - _cellPadding - fontSize;

                _editor.WithFont(font)
                      .WithFontSize(fontSize)
                      .WithTextColor(textColor)
                      .Text(cellText, textX, textY);
            }

            currentX += cellWidth + _borderWidth;
            cellIndex++;
        }

        // Dispose font
        font.Dispose();

        return currentY - rowHeight;
    }
}
