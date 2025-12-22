using System.Collections.Generic;

namespace PDFiumZ.HighLevel.HtmlToPdf;

/// <summary>
/// Represents a table cell.
/// </summary>
internal class CellInfo
{
    public string Content { get; set; } = string.Empty;
    public bool IsHeader { get; set; }
    public int ColSpan { get; set; } = 1;
    public int RowSpan { get; set; } = 1;
    public TextStyle Style { get; set; } = new TextStyle();
}

/// <summary>
/// Represents a table row.
/// </summary>
internal class RowInfo
{
    public List<CellInfo> Cells { get; set; } = new List<CellInfo>();
}

/// <summary>
/// Represents a table with layout information.
/// </summary>
internal class TableInfo
{
    public List<RowInfo> Rows { get; set; } = new List<RowInfo>();
    public double[] ColumnWidths { get; set; } = System.Array.Empty<double>();
    public double CellPadding { get; set; } = 5;
    public double BorderWidth { get; set; } = 1;
    public uint BorderColor { get; set; } = PdfColor.Black;
}
