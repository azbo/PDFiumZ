namespace PDFiumZ.Fluent;

/// <summary>
/// Represents a size with width and height.
/// </summary>
public readonly record struct Size(double Width, double Height)
{
    public static readonly Size Zero = new(0, 0);
    public static readonly Size Max = new(double.MaxValue, double.MaxValue);

    public bool IsZero => Width == 0 && Height == 0;
    public bool IsMax => Width == double.MaxValue && Height == double.MaxValue;
}
