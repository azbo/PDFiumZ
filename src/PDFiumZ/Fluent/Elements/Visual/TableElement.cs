using System.Collections.Generic;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Fluent.Elements.Visual;

/// <summary>
/// Renders a table with headers and rows.
/// </summary>
public class TableElement : IElement
{
    public List<TableCell> Headers { get; set; } = new();
    public List<List<TableCell>> Rows { get; set; } = new();
    public List<double> ColumnWidths { get; set; } = new();
    public PdfFont? Font { get; set; }
    public double FontSize { get; set; } = 12;
    public double BorderWidth { get; set; } = 0.5f;
    public uint BorderColor { get; set; } = PdfColor.Black;
    public uint HeaderBackgroundColor { get; set; } = 0xFF4472C4; // Blue
    public uint HeaderTextColor { get; set; } = PdfColor.White;
    public uint TextColor { get; set; } = PdfColor.Black;
    public double CellPadding { get; set; } = 5;
    public double RowSpacing { get; set; } = 0;

    public SpacePlan Measure(MeasureContext context)
    {
        var font = Font ?? context.DefaultFont;
        var fontSize = FontSize > 0 ? FontSize : context.DefaultFontSize;

        // Calculate total height
        double totalHeight = 0;
        double maxWidth = 0;

        // Measure headers
        double headerHeight = fontSize * 1.5 + CellPadding * 2;
        totalHeight += headerHeight;

        // Measure rows
        foreach (var row in Rows)
        {
            double rowHeight = fontSize * 1.2 + CellPadding * 2;
            foreach (var cell in row)
            {
                // Estimate cell height based on text wrapping
                double estimatedTextHeight = EstimateTextHeight(cell.Text, fontSize, font);
                rowHeight = System.Math.Max(rowHeight, estimatedTextHeight + CellPadding * 2);
            }
            totalHeight += rowHeight + RowSpacing;
        }

        // Calculate total width based on column widths
        double totalWidth = 0;
        if (ColumnWidths.Count > 0)
        {
            totalWidth = System.Linq.Enumerable.Sum(ColumnWidths);
        }
        else
        {
            // Auto-calculate column widths
            double colWidth = context.AvailableSpace.Width / System.Math.Max(Headers.Count, 1);
            totalWidth = colWidth * Headers.Count;
        }

        maxWidth = System.Math.Min(totalWidth, context.AvailableSpace.Width);

        var size = new Size(maxWidth, totalHeight);

        if (size.Height <= context.AvailableSpace.Height)
            return SpacePlan.FullRender(size);

        return SpacePlan.PartialRender(new Size(maxWidth, context.AvailableSpace.Height));
    }

    public void Render(RenderContext context)
    {
        var font = Font ?? context.DefaultFont;
        var fontSize = FontSize > 0 ? FontSize : context.DefaultFontSize;

        double currentY = context.Position.Y;
        double currentX = context.Position.X;

        // Calculate column widths
        List<double> colWidths = ColumnWidths;
        if (colWidths.Count == 0)
        {
            double colWidth = context.AvailableSpace.Width / System.Math.Max(Headers.Count, 1);
            colWidths = new List<double>();
            for (int i = 0; i < Headers.Count; i++)
                colWidths.Add(colWidth);
        }

        double totalWidth = System.Linq.Enumerable.Sum(colWidths);

        // Render headers
        double headerY = currentY;
        double headerHeight = fontSize * 1.5 + CellPadding * 2;

        // Draw header background
        context.Editor.Rectangle(
            new PdfRectangle(currentX, headerY - headerHeight, totalWidth, headerHeight),
            fillColor: HeaderBackgroundColor
        );

        // Draw header text
        double headerX = currentX;
        for (int i = 0; i < Headers.Count; i++)
        {
            var cell = Headers[i];
            double cellWidth = colWidths[System.Math.Min(i, colWidths.Count - 1)];

            double textX = headerX + CellPadding;
            double textY = headerY - headerHeight / 2 - fontSize / 2;

            context.Editor
                .WithFont(font)
                .WithFontSize(fontSize)
                .WithTextColor(HeaderTextColor)
                .Text(cell.Text, textX, textY);

            // Draw cell border
            context.Editor
                .WithStrokeColor(BorderColor)
                .WithLineWidth(BorderWidth)
                .Rectangle(new PdfRectangle(headerX, headerY - headerHeight, cellWidth, headerHeight));

            headerX += cellWidth;
        }

        currentY -= headerHeight;

        // Render rows
        foreach (var row in Rows)
        {
            double rowHeight = fontSize * 1.2 + CellPadding * 2;

            // Calculate row height based on cell content
            foreach (var cell in row)
            {
                double textHeight = EstimateTextHeight(cell.Text, fontSize, font);
                rowHeight = System.Math.Max(rowHeight, textHeight + CellPadding * 2);
            }

            double rowX = currentX;

            for (int i = 0; i < row.Count; i++)
            {
                var cell = row[i];
                double cellWidth = colWidths[System.Math.Min(i, colWidths.Count - 1)];

                double textX = rowX + CellPadding;
                double textY = currentY - rowHeight / 2 - fontSize / 2;

                context.Editor
                    .WithFont(font)
                    .WithFontSize(fontSize)
                    .WithTextColor(TextColor)
                    .Text(cell.Text, textX, textY);

                // Draw cell border
                context.Editor
                    .WithStrokeColor(BorderColor)
                    .WithLineWidth(BorderWidth)
                    .Rectangle(new PdfRectangle(rowX, currentY - rowHeight, cellWidth, rowHeight));

                rowX += cellWidth;
            }

            currentY -= rowHeight + RowSpacing;
        }
    }

    private double EstimateTextHeight(string text, double fontSize, PdfFont font)
    {
        if (string.IsNullOrEmpty(text))
            return fontSize;

        // Simple estimation - in real implementation would use font metrics
        int lineCount = 1;
        int estimatedCharsPerLine = 50; // Rough estimate
        if (text.Length > estimatedCharsPerLine)
        {
            lineCount = (text.Length + estimatedCharsPerLine - 1) / estimatedCharsPerLine;
        }

        return lineCount * fontSize * 1.2;
    }
}

/// <summary>
/// Represents a table cell.
/// </summary>
public class TableCell
{
    public string Text { get; set; } = string.Empty;
    public uint? Color { get; set; }
    public bool? Bold { get; set; }

    public TableCell(string text)
    {
        Text = text;
    }

    public TableCell(string text, uint color, bool bold = false)
    {
        Text = text;
        Color = color;
        Bold = bold;
    }
}
