namespace PDFiumZ.Fluent;

/// <summary>
/// Represents the type of space plan result.
/// </summary>
public enum SpacePlanType
{
    /// <summary>
    /// The element can fit in the available space.
    /// </summary>
    FullRender,

    /// <summary>
    /// The element can partially fit in the available space.
    /// </summary>
    PartialRender,

    /// <summary>
    /// The element needs to wrap to the next container (e.g., next page).
    /// </summary>
    Wrap
}

/// <summary>
/// Represents the result of measuring an element in available space.
/// </summary>
public class SpacePlan
{
    public SpacePlanType Type { get; init; }
    public Size Size { get; init; }

    public static SpacePlan FullRender(Size size) => new() { Type = SpacePlanType.FullRender, Size = size };
    public static SpacePlan PartialRender(Size size) => new() { Type = SpacePlanType.PartialRender, Size = size };
    public static SpacePlan Wrap() => new() { Type = SpacePlanType.Wrap, Size = Size.Zero };
}
