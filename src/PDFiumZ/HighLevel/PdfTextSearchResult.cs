using System;
using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a single text search result with position and bounding rectangles.
/// </summary>
public sealed class PdfTextSearchResult(int charIndex, int charCount, string text, IReadOnlyList<PdfRectangle> boundingRectangles)
{
    /// <summary>
    /// Gets the zero-based character index of the first character in the search result.
    /// </summary>
    public int CharIndex { get; } = charIndex;

    /// <summary>
    /// Gets the number of characters in the search result.
    /// </summary>
    public int CharCount { get; } = charCount;

    /// <summary>
    /// Gets the matched text content.
    /// </summary>
    public string Text { get; } = text ?? throw new ArgumentNullException(nameof(text));

    /// <summary>
    /// Gets the bounding rectangles for the search result.
    /// Multiple rectangles occur when text spans multiple lines.
    /// </summary>
    public IReadOnlyList<PdfRectangle> BoundingRectangles { get; } = boundingRectangles ?? throw new ArgumentNullException(nameof(boundingRectangles));
}
