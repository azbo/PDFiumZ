using System;

namespace PDFiumZ.HighLevel.HtmlToPdf;

/// <summary>
/// Represents text styling information for HTML to PDF conversion.
/// </summary>
internal class TextStyle
{
    /// <summary>
    /// Font to use for text rendering.
    /// </summary>
    public PdfFont? Font { get; set; }

    /// <summary>
    /// Font size in points.
    /// </summary>
    public double FontSize { get; set; } = 12;

    /// <summary>
    /// Text color in ARGB format.
    /// </summary>
    public uint Color { get; set; } = PdfColor.Black;

    /// <summary>
    /// Whether text is bold.
    /// </summary>
    public bool IsBold { get; set; }

    /// <summary>
    /// Whether text is italic.
    /// </summary>
    public bool IsItalic { get; set; }

    /// <summary>
    /// Whether text is underlined.
    /// </summary>
    public bool IsUnderline { get; set; }

    /// <summary>
    /// Text alignment (left, center, right).
    /// </summary>
    public string Alignment { get; set; } = "left";

    /// <summary>
    /// Line height multiplier (1.0 = single spacing).
    /// </summary>
    public double LineHeight { get; set; } = 1.2;

    /// <summary>
    /// Current list depth level (0 = not in list, 1+ = nesting level).
    /// </summary>
    public int ListDepth { get; set; } = 0;

    /// <summary>
    /// Current list type (ul, ol, none).
    /// </summary>
    public string ListType { get; set; } = "none";

    /// <summary>
    /// Current list item index (for ordered lists).
    /// </summary>
    public int ListItemIndex { get; set; } = 0;

    /// <summary>
    /// Creates a copy of this style.
    /// </summary>
    public TextStyle Clone()
    {
        return new TextStyle
        {
            Font = Font,
            FontSize = FontSize,
            Color = Color,
            IsBold = IsBold,
            IsItalic = IsItalic,
            IsUnderline = IsUnderline,
            Alignment = Alignment,
            LineHeight = LineHeight,
            ListDepth = ListDepth,
            ListType = ListType,
            ListItemIndex = ListItemIndex
        };
    }
}
