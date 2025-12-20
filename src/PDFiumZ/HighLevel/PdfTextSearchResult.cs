using System;
using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a single text search result with position and bounding rectangles.
/// </summary>
public sealed class PdfTextSearchResult
{
    /// <summary>
    /// Gets the zero-based character index of the first character in the search result.
    /// </summary>
    public int CharIndex { get; }

    /// <summary>
    /// Gets the number of characters in the search result.
    /// </summary>
    public int CharCount { get; }

    /// <summary>
    /// Gets the matched text content.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the bounding rectangles for the search result.
    /// Multiple rectangles occur when text spans multiple lines.
    /// </summary>
    public IReadOnlyList<PdfRectangle> BoundingRectangles { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfTextSearchResult"/> class.
    /// </summary>
    /// <param name="charIndex">Character index of the match.</param>
    /// <param name="charCount">Number of characters in the match.</param>
    /// <param name="text">Matched text content.</param>
    /// <param name="boundingRectangles">Bounding rectangles for the match.</param>
    internal PdfTextSearchResult(int charIndex, int charCount, string text, IReadOnlyList<PdfRectangle> boundingRectangles)
    {
        CharIndex = charIndex;
        CharCount = charCount;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        BoundingRectangles = boundingRectangles ?? throw new ArgumentNullException(nameof(boundingRectangles));
    }
}
