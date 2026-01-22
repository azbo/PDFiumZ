using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Fluent.Elements.Visual;

/// <summary>
/// Renders text with inline spans that have different styles (colors, bold, etc.).
/// Similar to HTML span tags within a text block.
/// </summary>
public class SpanTextElement : IElement
{
    public List<TextSpan> Spans { get; set; } = new();
    public PdfFont? Font { get; set; }
    public double BaseFontSize { get; set; } = 12;
    public double LineHeight { get; set; } = 1.2;
    public uint BaseColor { get; set; } = PdfColor.Black;

    public SpacePlan Measure(MeasureContext context)
    {
        var font = Font ?? context.DefaultFont;
        var fontSize = BaseFontSize > 0 ? BaseFontSize : context.DefaultFontSize;

        double totalWidth = 0;
        double maxHeight = fontSize * LineHeight;

        foreach (var span in Spans)
        {
            double spanFontSize = span.FontSize > 0 ? span.FontSize : fontSize;
            double estimatedWidth = span.Text.Length * spanFontSize * 0.6;
            totalWidth += estimatedWidth;
            maxHeight = System.Math.Max(maxHeight, spanFontSize * LineHeight);
        }

        var size = new Size(totalWidth, maxHeight);

        if (size.Width <= context.AvailableSpace.Width && size.Height <= context.AvailableSpace.Height)
            return SpacePlan.FullRender(size);

        if (size.Height <= context.AvailableSpace.Height)
            return SpacePlan.PartialRender(new Size(context.AvailableSpace.Width, maxHeight));

        return SpacePlan.Wrap();
    }

    public void Render(RenderContext context)
    {
        var font = Font ?? context.DefaultFont;
        var fontSize = BaseFontSize > 0 ? BaseFontSize : context.DefaultFontSize;

        double currentX = context.Position.X;
        double currentY = context.Position.Y;

        foreach (var span in Spans)
        {
            double spanFontSize = span.FontSize > 0 ? span.FontSize : fontSize;
            uint spanColor = span.Color != 0 ? span.Color : BaseColor;

            context.Editor
                .WithFont(font)
                .WithFontSize(spanFontSize)
                .WithTextColor(spanColor)
                .Text(span.Text, currentX, currentY);

            // Move cursor forward
            double estimatedWidth = span.Text.Length * spanFontSize * 0.6;
            currentX += estimatedWidth;
        }
    }
}

/// <summary>
/// Represents a text span with specific styling.
/// </summary>
public class TextSpan
{
    public string Text { get; set; } = string.Empty;
    public uint Color { get; set; } = 0;
    public double FontSize { get; set; } = 0;
    public bool Bold { get; set; } = false;

    public TextSpan(string text)
    {
        Text = text;
    }

    public TextSpan(string text, uint color, double fontSize = 0, bool bold = false)
    {
        Text = text;
        Color = color;
        FontSize = fontSize;
        Bold = bold;
    }
}

/// <summary>
/// Renders HTML-like text with basic formatting support.
/// Supports: &lt;br&gt;, &lt;b&gt;, &lt;span color="#"&gt;, &amp;nbsp;, &amp;emsp;, &amp;mdash;, etc.
/// </summary>
public class HtmlTextElement : IElement
{
    public string HtmlContent { get; set; } = string.Empty;
    public PdfFont? Font { get; set; }
    public double FontSize { get; set; } = 12;
    public uint TextColor { get; set; } = PdfColor.Black;
    public uint HighlightColor { get; set; } = 0xFF0000; // Red for highlights
    public List<string> HighlightWords { get; set; } = new();
    public double Indent { get; set; } = 0;
    public double LineHeight { get; set; } = 1.2;

    public SpacePlan Measure(MeasureContext context)
    {
        var font = Font ?? context.DefaultFont;
        var fontSize = FontSize > 0 ? FontSize : context.DefaultFontSize;

        // Parse HTML and calculate dimensions
        var lines = ParseHtmlToLines(HtmlContent);
        double lineHeight = fontSize * LineHeight;
        double totalHeight = lines.Count * lineHeight;

        double maxWidth = context.AvailableSpace.Width;
        foreach (var line in lines)
        {
            double lineWidth = Indent;
            foreach (var span in line)
            {
                double spanFontSize = span.FontSize > 0 ? span.FontSize : fontSize;
                lineWidth += span.Text.Length * spanFontSize * 0.6;
            }
            maxWidth = System.Math.Min(System.Math.Max(maxWidth, lineWidth), context.AvailableSpace.Width);
        }

        var size = new Size(maxWidth, totalHeight);

        if (size.Height <= context.AvailableSpace.Height)
            return SpacePlan.FullRender(size);

        return SpacePlan.PartialRender(new Size(maxWidth, context.AvailableSpace.Height));
    }

    public void Render(RenderContext context)
    {
        var font = Font ?? context.DefaultFont;
        var fontSize = FontSize > 0 ? FontSize : context.DefaultFontSize;

        var lines = ParseHtmlToLines(HtmlContent);
        double lineHeight = fontSize * LineHeight;
        double currentY = context.Position.Y;

        foreach (var line in lines)
        {
            double currentX = context.Position.X + Indent;
            double currentYLine = currentY;

            foreach (var span in line)
            {
                double spanFontSize = span.FontSize > 0 ? span.FontSize : fontSize;
                uint spanColor = span.Color != 0 ? span.Color : TextColor;

                // Apply highlights if needed
                RenderTextWithHighlights(context, font, spanFontSize, spanColor, span.Text, ref currentX, currentYLine);
            }

            currentY -= lineHeight;
        }
    }

    private void RenderTextWithHighlights(RenderContext context, PdfFont font, double fontSize, uint color,
        string text, ref double x, double y)
    {
        if (HighlightWords == null || HighlightWords.Count == 0)
        {
            // No highlights, render normally
            context.Editor
                .WithFont(font)
                .WithFontSize(fontSize)
                .WithTextColor(color)
                .Text(text, x, y);
            x += text.Length * fontSize * 0.6;
            return;
        }

        // Simple highlight implementation - split by highlighted words
        int currentIndex = 0;
        foreach (var word in HighlightWords)
        {
            int index = text.IndexOf(word, currentIndex, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                // Render text before highlight
                if (index > currentIndex)
                {
                    var beforeText = text.Substring(currentIndex, index - currentIndex);
                    context.Editor
                        .WithFont(font)
                        .WithFontSize(fontSize)
                        .WithTextColor(color)
                        .Text(beforeText, x, y);
                    x += beforeText.Length * fontSize * 0.6;
                }

                // Render highlighted word
                context.Editor
                    .WithFont(font)
                    .WithFontSize(fontSize)
                    .WithTextColor(HighlightColor)
                    .Text(word, x, y);
                x += word.Length * fontSize * 0.6;

                currentIndex = index + word.Length;
            }
        }

        // Render remaining text
        if (currentIndex < text.Length)
        {
            var remainingText = text.Substring(currentIndex);
            context.Editor
                .WithFont(font)
                .WithFontSize(fontSize)
                .WithTextColor(color)
                .Text(remainingText, x, y);
            x += remainingText.Length * fontSize * 0.6;
        }
    }

    private List<List<TextSpan>> ParseHtmlToLines(string html)
    {
        var lines = new List<List<TextSpan>>();

        // Decode HTML entities
        html = DecodeHtmlEntities(html);

        // Split by <br> tags
        var parts = html.Split(new[] { "<br>", "<br/>", "<br />" }, StringSplitOptions.None);

        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            var line = new List<TextSpan>();

            // Parse HTML tags in this line
            var currentText = part;
            var boldStack = new Stack<bool>();
            var colorStack = new Stack<uint>();

            // Simple regex to find tags
            var tagPattern = @"<(?<tag>/?\w+)(?:\s+[^>]*)?>?";
            var matches = Regex.Matches(currentText, tagPattern);

            int lastIndex = 0;
            foreach (Match match in matches)
            {
                // Add text before tag
                if (match.Index > lastIndex)
                {
                    var text = currentText.Substring(lastIndex, match.Index - lastIndex);
                    if (!string.IsNullOrEmpty(text))
                    {
                        line.Add(new TextSpan(text,
                            colorStack.Count > 0 ? colorStack.Peek() : 0,
                            FontSize,
                            boldStack.Count > 0 && boldStack.Peek()));
                    }
                }

                // Process tag
                var tag = match.Groups["tag"].Value;
                if (tag.StartsWith("/"))
                {
                    // Closing tag
                    if (tag == "/b" && boldStack.Count > 0) boldStack.Pop();
                    if (tag == "/span" && colorStack.Count > 0) colorStack.Pop();
                }
                else
                {
                    // Opening tag
                    if (tag == "b") boldStack.Push(true);

                    if (tag == "span")
                    {
                        // Extract color attribute
                        var colorMatch = Regex.Match(match.Value, @"color=""?#?([0-9A-Fa-f]+)""?");
                        if (colorMatch.Success)
                        {
                            var hexColor = colorMatch.Groups[1].Value;
                            if (hexColor.Length == 6)
                            {
                                uint color = Convert.ToUInt32(hexColor, 16);
                                colorStack.Push(0xFF000000 | color); // Add full alpha
                            }
                        }
                    }
                }

                lastIndex = match.Index + match.Length;
            }

            // Add remaining text
            if (lastIndex < currentText.Length)
            {
                var text = currentText.Substring(lastIndex);
                if (!string.IsNullOrEmpty(text))
                {
                    line.Add(new TextSpan(text,
                        colorStack.Count > 0 ? colorStack.Peek() : 0,
                        FontSize,
                        boldStack.Count > 0 && boldStack.Peek()));
                }
            }

            if (line.Count > 0)
                lines.Add(line);
        }

        return lines;
    }

    private string DecodeHtmlEntities(string html)
    {
        return html
            .Replace("&nbsp;", " ")
            .Replace("&emsp;", "   ")
            .Replace("&mdash;", "—")
            .Replace("&rsquo;", "'")
            .Replace("&ldquo;", "\"")
            .Replace("&rdquo;", "\"")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&amp;", "&");
    }
}
