using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PDFiumZ.HighLevel.HtmlToPdf;

/// <summary>
/// Converts simple HTML fragments to PDF content.
/// Supports basic HTML tags and CSS styles.
/// </summary>
public class HtmlToPdfConverter : IDisposable
{
    private readonly PdfDocument _document;
    private readonly Dictionary<string, PdfFont> _fontCache;
    private double _currentY;
    private double _pageWidth;
    private double _pageHeight;
    private double _marginLeft = 50;
    private double _marginRight = 50;
    private double _marginTop = 50;
    private double _marginBottom = 50;

    /// <summary>
    /// Image loader delegate that converts image source to BGRA pixel data.
    /// </summary>
    /// <param name="src">Image source (file path, URL, or data URI).</param>
    /// <returns>Tuple of (bgraData, width, height) or null if image cannot be loaded.</returns>
    public delegate (byte[] bgraData, int width, int height)? ImageLoader(string src);

    private ImageLoader? _imageLoader;

    /// <summary>
    /// Gets or sets the left margin in points.
    /// </summary>
    public double MarginLeft
    {
        get => _marginLeft;
        set => _marginLeft = value;
    }

    /// <summary>
    /// Gets or sets the right margin in points.
    /// </summary>
    public double MarginRight
    {
        get => _marginRight;
        set => _marginRight = value;
    }

    /// <summary>
    /// Gets or sets the top margin in points.
    /// </summary>
    public double MarginTop
    {
        get => _marginTop;
        set => _marginTop = value;
    }

    /// <summary>
    /// Gets or sets the bottom margin in points.
    /// </summary>
    public double MarginBottom
    {
        get => _marginBottom;
        set => _marginBottom = value;
    }

    /// <summary>
    /// Gets or sets the image loader for loading and decoding images.
    /// If not set, image tags will be ignored.
    /// </summary>
    public ImageLoader? ImageLoaderFunc
    {
        get => _imageLoader;
        set => _imageLoader = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlToPdfConverter"/> class.
    /// </summary>
    /// <param name="document">The PDF document to add content to.</param>
    public HtmlToPdfConverter(PdfDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _fontCache = new Dictionary<string, PdfFont>();
    }

    /// <summary>
    /// Converts HTML content to PDF and adds it to a new page.
    /// </summary>
    /// <param name="html">The HTML content to convert.</param>
    /// <param name="pageWidth">Page width in points (default: A4 width = 595).</param>
    /// <param name="pageHeight">Page height in points (default: A4 height = 842).</param>
    /// <returns>The created PDF page.</returns>
    public PdfPage ConvertToPdf(string html, double pageWidth = 595, double pageHeight = 842)
    {
        if (string.IsNullOrWhiteSpace(html))
            throw new ArgumentException("HTML content cannot be null or empty.", nameof(html));

        _pageWidth = pageWidth;
        _pageHeight = pageHeight;
        _currentY = pageHeight - _marginTop;

        var page = _document.CreatePage(pageWidth, pageHeight);

        using (var editor = page.BeginEdit())
        {
            RenderHtml(editor, html);
            editor.Commit();
        }

        return page;
    }

    private void RenderHtml(PdfContentEditor editor, string html)
    {
        // Remove extra whitespace and normalize line breaks
        html = Regex.Replace(html, @"\r\n|\r|\n", " ");
        html = Regex.Replace(html, @"\s+", " ");

        var baseStyle = new TextStyle();
        ProcessHtmlContent(editor, html, baseStyle);
    }

    private void ProcessHtmlContent(PdfContentEditor editor, string html, TextStyle parentStyle)
    {
        int position = 0;

        while (position < html.Length)
        {
            // Find next tag
            int tagStart = html.IndexOf('<', position);

            // Process text before tag
            if (tagStart == -1 || tagStart > position)
            {
                int endPos = tagStart == -1 ? html.Length : tagStart;
                string text = html.Substring(position, endPos - position).Trim();

                if (!string.IsNullOrEmpty(text))
                {
                    text = System.Net.WebUtility.HtmlDecode(text);
                    RenderText(editor, text, parentStyle);
                }

                if (tagStart == -1) break;
                position = tagStart;
            }

            // Find tag end
            int tagEnd = html.IndexOf('>', position);
            if (tagEnd == -1) break;

            string tagContent = html.Substring(position + 1, tagEnd - position - 1);
            position = tagEnd + 1;

            // Skip comments
            if (tagContent.StartsWith("!--"))
            {
                int commentEnd = html.IndexOf("-->", position);
                if (commentEnd != -1)
                    position = commentEnd + 3;
                continue;
            }

            // Check if closing tag
            if (tagContent.StartsWith("/"))
            {
                continue; // Closing tags are handled when finding matching pairs
            }

            // Parse tag
            var (tagName, attributes, isSelfClosing) = ParseTag(tagContent);

            // Handle self-closing tags
            if (isSelfClosing || tagName == "br")
            {
                HandleSelfClosingTag(editor, tagName, attributes, parentStyle);
                continue;
            }

            // Find matching closing tag
            int closeTagPos = FindClosingTag(html, position, tagName);
            if (closeTagPos == -1)
            {
                // No closing tag found, treat as text
                continue;
            }

            string innerHtml = html.Substring(position, closeTagPos - position);
            position = closeTagPos;

            // Create style for this element
            var elementStyle = CreateStyleFromTag(tagName, attributes, parentStyle);

            // Process element
            ProcessElement(editor, tagName, innerHtml, elementStyle, attributes);
        }
    }

    private void ProcessElement(PdfContentEditor editor, string tagName, string content, TextStyle style, Dictionary<string, string> attributes)
    {
        switch (tagName.ToLower())
        {
            case "h1":
            case "h2":
            case "h3":
            case "h4":
            case "h5":
            case "h6":
                ProcessHeading(editor, content, style);
                break;

            case "p":
            case "div":
                ProcessParagraph(editor, content, style);
                break;

            case "ul":
                ProcessUnorderedList(editor, content, style);
                break;

            case "ol":
                ProcessOrderedList(editor, content, style);
                break;

            case "li":
                ProcessListItem(editor, content, style);
                break;

            case "table":
                ProcessTable(editor, content, style, attributes);
                break;

            case "b":
            case "strong":
                style.IsBold = true;
                ProcessHtmlContent(editor, content, style);
                break;

            case "i":
            case "em":
                style.IsItalic = true;
                ProcessHtmlContent(editor, content, style);
                break;

            case "u":
                style.IsUnderline = true;
                ProcessHtmlContent(editor, content, style);
                break;

            case "span":
                ProcessHtmlContent(editor, content, style);
                break;

            default:
                // Unknown tag, render as text
                ProcessHtmlContent(editor, content, style);
                break;
        }
    }

    private void ProcessHeading(PdfContentEditor editor, string content, TextStyle style)
    {
        // Add spacing before heading
        _currentY -= style.FontSize * 0.5;

        ProcessHtmlContent(editor, content, style);

        // Add spacing after heading
        _currentY -= style.FontSize * 0.3;
    }

    private void ProcessParagraph(PdfContentEditor editor, string content, TextStyle style)
    {
        ProcessHtmlContent(editor, content, style);

        // Add paragraph spacing
        _currentY -= style.FontSize * style.LineHeight;
    }

    private void HandleSelfClosingTag(PdfContentEditor editor, string tagName, Dictionary<string, string> attributes, TextStyle style)
    {
        if (tagName == "br")
        {
            _currentY -= style.FontSize * style.LineHeight;
        }
        else if (tagName == "img")
        {
            ProcessImage(editor, attributes, style);
        }
    }

    private void RenderText(PdfContentEditor editor, string text, TextStyle style)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        // Get appropriate font
        var font = GetFont(style);
        if (font == null)
            return;

        double x = CalculateXPosition(text, style, font);

        // Check if we need a new page
        if (_currentY - style.FontSize < _marginBottom)
        {
            // Would need page break handling here
            // For simplicity, just continue
        }

        editor.WithFont(font)
              .WithFontSize(style.FontSize)
              .WithTextColor(style.Color)
              .Text(text, x, _currentY);

        // Draw underline if needed
        if (style.IsUnderline)
        {
            double textWidth = EstimateTextWidth(text, style.FontSize);
            double underlineY = _currentY - 2;
            editor.Line(x, underlineY, x + textWidth, underlineY, style.Color, 0.5);
        }

        _currentY -= style.FontSize * style.LineHeight;
    }

    private double CalculateXPosition(string text, TextStyle style, PdfFont font)
    {
        double contentWidth = _pageWidth - _marginLeft - _marginRight;
        double textWidth = EstimateTextWidth(text, style.FontSize);

        switch (style.Alignment.ToLower())
        {
            case "center":
                return _marginLeft + (contentWidth - textWidth) / 2;
            case "right":
                return _pageWidth - _marginRight - textWidth;
            default: // left
                return _marginLeft;
        }
    }

    private double EstimateTextWidth(string text, double fontSize)
    {
        // Rough estimation: average character width is about 0.5 * fontSize
        return text.Length * fontSize * 0.5;
    }

    private PdfFont GetFont(TextStyle style)
    {
        string fontKey = $"{style.IsBold}_{style.IsItalic}";

        if (!_fontCache.ContainsKey(fontKey))
        {
            PdfStandardFont standardFont;

            if (style.IsBold && style.IsItalic)
                standardFont = PdfStandardFont.HelveticaBoldOblique;
            else if (style.IsBold)
                standardFont = PdfStandardFont.HelveticaBold;
            else if (style.IsItalic)
                standardFont = PdfStandardFont.HelveticaOblique;
            else
                standardFont = PdfStandardFont.Helvetica;

            _fontCache[fontKey] = PdfFont.LoadStandardFont(_document, standardFont);
        }

        return _fontCache[fontKey];
    }

    private TextStyle CreateStyleFromTag(string tagName, Dictionary<string, string> attributes, TextStyle parentStyle)
    {
        var style = parentStyle.Clone();

        // Apply default styles based on tag
        switch (tagName.ToLower())
        {
            case "h1":
                style.FontSize = PdfFontSize.Heading1;
                style.IsBold = true;
                break;
            case "h2":
                style.FontSize = PdfFontSize.Heading2;
                style.IsBold = true;
                break;
            case "h3":
                style.FontSize = PdfFontSize.Heading3;
                style.IsBold = true;
                break;
            case "h4":
                style.FontSize = PdfFontSize.Heading4;
                style.IsBold = true;
                break;
            case "h5":
                style.FontSize = 16;
                style.IsBold = true;
                break;
            case "h6":
                style.FontSize = 14;
                style.IsBold = true;
                break;
            case "b":
            case "strong":
                style.IsBold = true;
                break;
            case "i":
            case "em":
                style.IsItalic = true;
                break;
            case "u":
                style.IsUnderline = true;
                break;
        }

        // Apply inline styles from attributes
        if (attributes.ContainsKey("style"))
        {
            ApplyInlineStyle(style, attributes["style"]);
        }

        return style;
    }

    private void ApplyInlineStyle(TextStyle style, string styleText)
    {
        var declarations = styleText.Split(';');

        foreach (var declaration in declarations)
        {
            var parts = declaration.Split(':');
            if (parts.Length != 2) continue;

            string property = parts[0].Trim().ToLower();
            string value = parts[1].Trim().ToLower();

            switch (property)
            {
                case "font-size":
                    style.FontSize = ParseFontSize(value);
                    break;
                case "color":
                    style.Color = ParseColor(value);
                    break;
                case "text-align":
                    style.Alignment = value;
                    break;
                case "font-weight":
                    style.IsBold = value == "bold" || value == "bolder" || int.TryParse(value, out int weight) && weight >= 600;
                    break;
                case "font-style":
                    style.IsItalic = value == "italic" || value == "oblique";
                    break;
                case "text-decoration":
                    style.IsUnderline = value.Contains("underline");
                    break;
            }
        }
    }

    private double ParseFontSize(string value)
    {
        // Remove units and parse
        value = value.ToLower().Trim();

        if (value.EndsWith("px"))
        {
            if (double.TryParse(value.Substring(0, value.Length - 2), out double px))
                return px * 0.75; // Convert px to points (rough approximation)
        }
        else if (value.EndsWith("pt"))
        {
            if (double.TryParse(value.Substring(0, value.Length - 2), out double pt))
                return pt;
        }
        else if (value.EndsWith("em"))
        {
            if (double.TryParse(value.Substring(0, value.Length - 2), out double em))
                return em * 12; // Assume base size of 12pt
        }
        else if (double.TryParse(value, out double size))
        {
            return size;
        }

        return 12; // Default
    }

    private uint ParseColor(string value)
    {
        value = value.Trim();

        // Hex color
        if (value.StartsWith("#"))
        {
            try
            {
                return PdfColor.FromHex(value);
            }
            catch
            {
                return PdfColor.Black;
            }
        }

        // Named colors
        switch (value.ToLower())
        {
            case "black": return PdfColor.Black;
            case "white": return PdfColor.White;
            case "red": return PdfColor.Red;
            case "green": return PdfColor.Green;
            case "blue": return PdfColor.Blue;
            case "yellow": return PdfColor.Yellow;
            case "orange": return PdfColor.Orange;
            case "purple": return PdfColor.Purple;
            case "gray": case "grey": return PdfColor.Gray;
            default: return PdfColor.Black;
        }
    }

    private (string tagName, Dictionary<string, string> attributes, bool isSelfClosing) ParseTag(string tagContent)
    {
        tagContent = tagContent.Trim();
        bool isSelfClosing = tagContent.EndsWith("/");

        if (isSelfClosing)
            tagContent = tagContent.Substring(0, tagContent.Length - 1).Trim();

        var parts = tagContent.Split(new[] { ' ' }, 2);
        string tagName = parts[0].ToLower();
        var attributes = new Dictionary<string, string>();

        if (parts.Length > 1)
        {
            string attrText = parts[1];
            // Simple attribute parsing (handles both single and double quotes: style="..." or src='...')
            var attrPattern = @"(\w+)\s*=\s*[""']([^""']*)[""']";
            var matches = Regex.Matches(attrText, attrPattern);

            foreach (Match match in matches)
            {
                attributes[match.Groups[1].Value.ToLower()] = match.Groups[2].Value;
            }
        }

        return (tagName, attributes, isSelfClosing);
    }

    private int FindClosingTag(string html, int startPos, string tagName)
    {
        string openTag = "<" + tagName;
        string closeTag = "</" + tagName;
        int depth = 1;
        int pos = startPos;

        while (pos < html.Length && depth > 0)
        {
            int nextOpen = html.IndexOf(openTag, pos, StringComparison.OrdinalIgnoreCase);
            int nextClose = html.IndexOf(closeTag, pos, StringComparison.OrdinalIgnoreCase);

            if (nextClose == -1)
                return -1;

            if (nextOpen != -1 && nextOpen < nextClose)
            {
                // Check if it's a real opening tag (not part of closing tag)
                int tagEnd = html.IndexOf('>', nextOpen);
                if (tagEnd != -1 && html[nextOpen + openTag.Length] != '/')
                {
                    depth++;
                    pos = tagEnd + 1;
                    continue;
                }
            }

            depth--;
            if (depth == 0)
            {
                // Find the end of closing tag
                int closeEnd = html.IndexOf('>', nextClose);
                return closeEnd != -1 ? closeEnd + 1 : -1;
            }

            pos = nextClose + closeTag.Length;
        }

        return -1;
    }

    private void ProcessUnorderedList(PdfContentEditor editor, string content, TextStyle style)
    {
        // Increase list depth
        var listStyle = style.Clone();
        listStyle.ListDepth++;
        listStyle.ListType = "ul";

        // Process list content
        ProcessHtmlContent(editor, content, listStyle);

        // Add spacing after list
        _currentY -= style.FontSize * 0.5;
    }

    private void ProcessOrderedList(PdfContentEditor editor, string content, TextStyle style)
    {
        // Increase list depth
        var listStyle = style.Clone();
        listStyle.ListDepth++;
        listStyle.ListType = "ol";
        listStyle.ListItemIndex = 0; // Reset counter for this list

        // Process list content
        ProcessHtmlContent(editor, content, listStyle);

        // Add spacing after list
        _currentY -= style.FontSize * 0.5;
    }

    private void ProcessListItem(PdfContentEditor editor, string content, TextStyle style)
    {
        if (style.ListDepth == 0)
        {
            // Not in a list context, treat as paragraph
            ProcessParagraph(editor, content, style);
            return;
        }

        // Get appropriate font
        var font = GetFont(style);
        if (font == null)
            return;

        // Calculate indentation based on depth
        double indentPerLevel = 20;
        double indent = style.ListDepth * indentPerLevel;
        double bulletWidth = 15;

        // Increment list item index for ordered lists
        if (style.ListType == "ol")
        {
            style.ListItemIndex++;
        }

        // Calculate X position for bullet/number
        double bulletX = _marginLeft + indent;
        double contentX = bulletX + bulletWidth;

        // Render bullet or number
        string marker = "";
        if (style.ListType == "ul")
        {
            // Use bullet point based on depth
            int depthMod = style.ListDepth % 3;
            if (depthMod == 1)
                marker = "•";  // Filled circle
            else if (depthMod == 2)
                marker = "◦";  // Hollow circle
            else
                marker = "▪";  // Square
        }
        else if (style.ListType == "ol")
        {
            marker = style.ListItemIndex.ToString() + ".";
        }

        // Render the marker
        editor.WithFont(font)
              .WithFontSize(style.FontSize)
              .WithTextColor(style.Color)
              .Text(marker, bulletX, _currentY);

        // Save current Y position
        double startY = _currentY;

        // Temporarily adjust margin for list item content
        double savedMarginLeft = _marginLeft;
        _marginLeft = contentX;

        // Process list item content
        ProcessHtmlContent(editor, content, style);

        // Restore margin
        _marginLeft = savedMarginLeft;

        // Ensure minimum spacing between list items
        if (_currentY >= startY - style.FontSize * style.LineHeight)
        {
            _currentY = startY - style.FontSize * style.LineHeight;
        }

        // Add small spacing after list item
        _currentY -= style.FontSize * 0.2;
    }

    private void ProcessTable(PdfContentEditor editor, string content, TextStyle style, Dictionary<string, string> attributes)
    {
        var table = new TableInfo();

        // Parse border attribute
        if (attributes.TryGetValue("border", out string? borderValue))
        {
            if (double.TryParse(borderValue, out double borderWidth))
            {
                table.BorderWidth = borderWidth;
            }
        }

        // Parse cellpadding attribute
        if (attributes.TryGetValue("cellpadding", out string? cellpaddingValue))
        {
            if (double.TryParse(cellpaddingValue, out double cellPadding))
            {
                table.CellPadding = cellPadding;
            }
        }

        // Parse cellspacing attribute (not supported by PDFium, but we can adjust border)
        // Note: cellspacing in HTML affects spacing between cells, which we don't fully support

        // Parse table structure
        ParseTableContent(content, table, style);

        if (table.Rows.Count == 0)
            return;

        // Calculate column widths
        CalculateColumnWidths(table);

        // Render table
        RenderTable(editor, table, style);
    }

    private void ParseTableContent(string html, TableInfo table, TextStyle parentStyle)
    {
        int position = 0;

        while (position < html.Length)
        {
            // Find next tag
            int tagStart = html.IndexOf('<', position);

            if (tagStart == -1)
                break;

            position = tagStart;

            // Find tag end
            int tagEnd = html.IndexOf('>', position);
            if (tagEnd == -1) break;

            string tagContent = html.Substring(position + 1, tagEnd - position - 1);
            position = tagEnd + 1;

            // Skip comments
            if (tagContent.StartsWith("!--"))
            {
                int commentEnd = html.IndexOf("-->", position);
                if (commentEnd != -1)
                    position = commentEnd + 3;
                continue;
            }

            // Check if closing tag
            if (tagContent.StartsWith("/"))
                continue;

            // Parse tag
            var (tagName, attributes, isSelfClosing) = ParseTag(tagContent);

            if (tagName == "tr")
            {
                // Find closing tag for row
                int closeTagPos = FindClosingTag(html, position, "tr");
                if (closeTagPos == -1) continue;

                string rowHtml = html.Substring(position, closeTagPos - position);
                position = closeTagPos;

                // Parse row
                var row = ParseTableRow(rowHtml, parentStyle);
                table.Rows.Add(row);
            }
        }
    }

    private RowInfo ParseTableRow(string html, TextStyle parentStyle)
    {
        var row = new RowInfo();
        int position = 0;

        while (position < html.Length)
        {
            int tagStart = html.IndexOf('<', position);

            if (tagStart == -1)
                break;

            position = tagStart;

            int tagEnd = html.IndexOf('>', position);
            if (tagEnd == -1) break;

            string tagContent = html.Substring(position + 1, tagEnd - position - 1);
            position = tagEnd + 1;

            if (tagContent.StartsWith("/"))
                continue;

            var (tagName, attributes, isSelfClosing) = ParseTag(tagContent);

            if (tagName == "td" || tagName == "th")
            {
                // Find the closing tag start position (not end position)
                int closeTagStart = html.IndexOf("</" + tagName, position, StringComparison.OrdinalIgnoreCase);
                if (closeTagStart == -1) continue;

                string cellHtml = html.Substring(position, closeTagStart - position);

                // Move position past the closing tag
                int closeTagEnd = html.IndexOf('>', closeTagStart);
                position = closeTagEnd != -1 ? closeTagEnd + 1 : closeTagStart;

                // Parse cell
                var cell = new CellInfo
                {
                    Content = System.Net.WebUtility.HtmlDecode(cellHtml.Trim()),
                    IsHeader = tagName == "th",
                    Style = parentStyle.Clone()
                };

                // Make header bold
                if (cell.IsHeader)
                    cell.Style.IsBold = true;

                // Parse colspan and rowspan
                if (attributes.TryGetValue("colspan", out string? colspanStr))
                {
                    if (int.TryParse(colspanStr, out int colspan))
                        cell.ColSpan = colspan;
                }

                if (attributes.TryGetValue("rowspan", out string? rowspanStr))
                {
                    if (int.TryParse(rowspanStr, out int rowspan))
                        cell.RowSpan = rowspan;
                }

                row.Cells.Add(cell);
            }
        }

        return row;
    }

    private void CalculateColumnWidths(TableInfo table)
    {
        if (table.Rows.Count == 0)
            return;

        // Find maximum column count
        int maxCols = 0;
        foreach (var row in table.Rows)
        {
            int colCount = 0;
            foreach (var cell in row.Cells)
                colCount += cell.ColSpan;
            maxCols = System.Math.Max(maxCols, colCount);
        }

        if (maxCols == 0)
            return;

        // Calculate available width
        double availableWidth = _pageWidth - _marginLeft - _marginRight;
        double totalBorderWidth = (maxCols + 1) * table.BorderWidth;
        double usableWidth = availableWidth - totalBorderWidth;

        // Equal column widths for simplicity
        double columnWidth = usableWidth / maxCols;

        table.ColumnWidths = new double[maxCols];
        for (int i = 0; i < maxCols; i++)
            table.ColumnWidths[i] = columnWidth;
    }

    private void RenderTable(PdfContentEditor editor, TableInfo table, TextStyle style)
    {
        if (table.Rows.Count == 0 || table.ColumnWidths.Length == 0)
            return;

        // Add spacing before table
        _currentY -= style.FontSize;

        double tableWidth = 0;
        foreach (var width in table.ColumnWidths)
            tableWidth += width;

        double tableX = _marginLeft;
        double tableY = _currentY;

        // Calculate all row heights first
        var rowHeights = new double[table.Rows.Count];
        for (int i = 0; i < table.Rows.Count; i++)
        {
            rowHeights[i] = CalculateRowHeight(table.Rows[i], table);
        }

        // Draw cell contents and collect positions for grid lines
        double currentY = tableY;
        var horizontalLines = new System.Collections.Generic.List<double> { currentY }; // Top line

        for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            double rowHeight = rowHeights[rowIndex];
            double currentX = tableX;
            int colIndex = 0;

            foreach (var cell in row.Cells)
            {
                double cellWidth = 0;
                for (int i = 0; i < cell.ColSpan && colIndex + i < table.ColumnWidths.Length; i++)
                    cellWidth += table.ColumnWidths[colIndex + i];

                // Draw cell content
                if (!string.IsNullOrWhiteSpace(cell.Content))
                {
                    var font = GetFont(cell.Style);
                    if (font != null)
                    {
                        double textX = currentX + table.CellPadding;
                        double textY = currentY - table.CellPadding - cell.Style.FontSize;

                        editor.WithFont(font)
                              .WithFontSize(cell.Style.FontSize)
                              .WithTextColor(cell.Style.Color)
                              .Text(cell.Content, textX, textY);
                    }
                }

                currentX += cellWidth;
                colIndex += cell.ColSpan;
            }

            currentY -= rowHeight;
            horizontalLines.Add(currentY); // Bottom line of this row
        }

        // Draw borders only if border width > 0
        if (table.BorderWidth > 0)
        {
            // Draw horizontal grid lines (top and bottom of each row)
            foreach (var yPos in horizontalLines)
            {
                editor.Line(tableX, yPos, tableX + tableWidth, yPos, table.BorderColor, table.BorderWidth);
            }

            // Draw vertical grid lines (left and right of each column)
            double xPos = tableX;
            editor.Line(xPos, tableY, xPos, currentY, table.BorderColor, table.BorderWidth); // Left border

            for (int i = 0; i < table.ColumnWidths.Length; i++)
            {
                xPos += table.ColumnWidths[i];
                editor.Line(xPos, tableY, xPos, currentY, table.BorderColor, table.BorderWidth);
            }
        }

        // Update current Y position
        _currentY = currentY - style.FontSize;
    }

    private double CalculateRowHeight(RowInfo row, TableInfo table)
    {
        double maxHeight = 0;

        foreach (var cell in row.Cells)
        {
            double cellHeight = cell.Style.FontSize + 2 * table.CellPadding;
            maxHeight = System.Math.Max(maxHeight, cellHeight);
        }

        return maxHeight;
    }

    private void ProcessImage(PdfContentEditor editor, Dictionary<string, string> attributes, TextStyle style)
    {
        // Check if image loader is available
        if (_imageLoader == null)
        {
            // No image loader provided, skip image
            return;
        }

        // Get src attribute
        if (!attributes.TryGetValue("src", out string? src) || string.IsNullOrWhiteSpace(src))
        {
            return;
        }

        // Load image using the provided loader
        var imageData = _imageLoader(src);
        if (imageData == null)
        {
            return;
        }

        var (bgraData, imgWidth, imgHeight) = imageData.Value;

        // Parse width and height attributes (in pixels or points)
        double width = imgWidth;
        double height = imgHeight;

        if (attributes.TryGetValue("width", out string? widthStr))
        {
            if (double.TryParse(widthStr.Replace("px", "").Replace("pt", ""), out double w))
            {
                width = w;
                // Maintain aspect ratio if height not specified
                if (!attributes.ContainsKey("height"))
                {
                    height = imgHeight * (width / imgWidth);
                }
            }
        }

        if (attributes.TryGetValue("height", out string? heightStr))
        {
            if (double.TryParse(heightStr.Replace("px", "").Replace("pt", ""), out double h))
            {
                height = h;
                // Maintain aspect ratio if width not specified
                if (!attributes.ContainsKey("width"))
                {
                    width = imgWidth * (height / imgHeight);
                }
            }
        }

        // Parse style attribute for dimensions
        if (attributes.TryGetValue("style", out string? styleStr))
        {
            var declarations = styleStr.Split(';');
            foreach (var declaration in declarations)
            {
                var parts = declaration.Split(':');
                if (parts.Length != 2) continue;

                string property = parts[0].Trim().ToLower();
                string value = parts[1].Trim();

                if (property == "width")
                {
                    if (double.TryParse(value.Replace("px", "").Replace("pt", ""), out double w))
                    {
                        width = w;
                    }
                }
                else if (property == "height")
                {
                    if (double.TryParse(value.Replace("px", "").Replace("pt", ""), out double h))
                    {
                        height = h;
                    }
                }
            }
        }

        // Calculate X position based on alignment
        double x = _marginLeft;
        double contentWidth = _pageWidth - _marginLeft - _marginRight;

        if (style.Alignment.ToLower() == "center")
        {
            x = _marginLeft + (contentWidth - width) / 2;
        }
        else if (style.Alignment.ToLower() == "right")
        {
            x = _pageWidth - _marginRight - width;
        }

        // Check if we need a new page
        if (_currentY - height < _marginBottom)
        {
            // Would need page break handling here
            // For simplicity, just continue
        }

        // Calculate Y position (PDF coordinates are bottom-up)
        double y = _currentY - height;

        // Add image to page
        var bounds = new PdfRectangle(x, y, width, height);
        editor.AddImage(bgraData, imgWidth, imgHeight, bounds);

        // Move Y position down
        _currentY -= height + style.FontSize * 0.5; // Add some spacing after image
    }

    /// <summary>
    /// Cleans up resources used by the converter.
    /// </summary>
    public void Dispose()
    {
        foreach (var font in _fontCache.Values)
        {
            font?.Dispose();
        }
        _fontCache.Clear();
    }
}
