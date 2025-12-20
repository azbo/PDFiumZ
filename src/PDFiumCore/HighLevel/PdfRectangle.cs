namespace PDFiumCore.HighLevel;

/// <summary>
/// Represents a rectangle in PDF coordinate space.
/// </summary>
public readonly record struct PdfRectangle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfRectangle"/> struct.
    /// </summary>
    /// <param name="x">The X coordinate of the rectangle's origin.</param>
    /// <param name="y">The Y coordinate of the rectangle's origin.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public PdfRectangle(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the X coordinate of the rectangle's origin.
    /// </summary>
    public double X { get; init; }

    /// <summary>
    /// Gets the Y coordinate of the rectangle's origin.
    /// </summary>
    public double Y { get; init; }

    /// <summary>
    /// Gets the width of the rectangle.
    /// </summary>
    public double Width { get; init; }

    /// <summary>
    /// Gets the height of the rectangle.
    /// </summary>
    public double Height { get; init; }

    /// <summary>
    /// Gets the right edge coordinate.
    /// </summary>
    public double Right => X + Width;

    /// <summary>
    /// Gets the bottom edge coordinate.
    /// </summary>
    public double Bottom => Y + Height;
}
