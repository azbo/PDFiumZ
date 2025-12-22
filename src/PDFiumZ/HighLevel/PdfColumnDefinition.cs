using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Defines column widths for a PDF table.
/// </summary>
public sealed class PdfColumnDefinition
{
    private readonly List<double?> _columns = new();

    /// <summary>
    /// Gets the number of columns defined.
    /// </summary>
    public int Count => _columns.Count;

    /// <summary>
    /// Adds a column with automatic width (equal distribution of remaining space).
    /// </summary>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfColumnDefinition Add()
    {
        _columns.Add(null);
        return this;
    }

    /// <summary>
    /// Adds a column with fixed width in points.
    /// </summary>
    /// <param name="width">Column width in points.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public PdfColumnDefinition Add(double width)
    {
        if (width <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(width), "Column width must be positive.");

        _columns.Add(width);
        return this;
    }

    /// <summary>
    /// Calculates actual column widths based on available space.
    /// </summary>
    /// <param name="availableWidth">Total available width in points.</param>
    /// <returns>Array of actual column widths.</returns>
    internal double[] CalculateWidths(double availableWidth)
    {
        if (_columns.Count == 0)
            return System.Array.Empty<double>();

        // Calculate fixed width total and auto-width count
        double fixedTotal = 0;
        int autoCount = 0;

        foreach (var width in _columns)
        {
            if (width.HasValue)
                fixedTotal += width.Value;
            else
                autoCount++;
        }

        // Calculate auto-width
        double autoWidth = autoCount > 0 ? (availableWidth - fixedTotal) / autoCount : 0;

        // Build result array
        var result = new double[_columns.Count];
        for (int i = 0; i < _columns.Count; i++)
        {
            result[i] = _columns[i] ?? autoWidth;
        }

        return result;
    }
}
