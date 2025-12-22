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
            ProcessElement(editor, tagName, innerHtml, elementStyle);
        }
    }

    private void ProcessElement(PdfContentEditor editor, string tagName, string content, TextStyle style)
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
            // Simple attribute parsing (handles style="..." pattern)
            var attrPattern = @"(\w+)\s*=\s*""([^""]*)""";
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
