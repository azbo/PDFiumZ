using System;
using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Builder for constructing table rows with fluent API.
/// </summary>
public sealed class PdfTableRowBuilder
{
    private readonly List<string> _cells = new();
    private readonly bool _isHeader;

    /// <summary>
    /// Gets the cell contents.
    /// </summary>
    internal IReadOnlyList<string> Cells => _cells;

    /// <summary>
    /// Gets whether this is a header row.
    /// </summary>
    internal bool IsHeader => _isHeader;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfTableRowBuilder"/> class.
    /// </summary>
    /// <param name="isHeader">Whether this is a header row.</param>
    internal PdfTableRowBuilder(bool isHeader = false)
    {
        _isHeader = isHeader;
    }

    /// <summary>
    /// Adds a cell with text content.
    /// </summary>
    /// <param name="text">Cell text content.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableRowBuilder Cell(string text)
    {
        _cells.Add(text ?? string.Empty);
        return this;
    }

    /// <summary>
    /// Adds multiple cells from an array.
    /// </summary>
    /// <param name="texts">Array of cell texts.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfTableRowBuilder AddCells(params string[] texts)
    {
        if (texts != null)
        {
            foreach (var text in texts)
            {
                _cells.Add(text ?? string.Empty);
            }
        }
        return this;
    }
}
