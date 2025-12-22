namespace PDFiumZ.Fluent;

/// <summary>
/// Represents a position with X and Y coordinates.
/// </summary>
public readonly record struct Position(double X, double Y)
{
    public static readonly Position Zero = new(0, 0);
}
