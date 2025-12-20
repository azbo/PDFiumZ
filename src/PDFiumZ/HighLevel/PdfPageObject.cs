namespace PDFiumCore.HighLevel;

/// <summary>
/// Represents the type of a PDF page object.
/// </summary>
public enum PdfPageObjectType
{
    /// <summary>
    /// Unknown object type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Text object.
    /// </summary>
    Text = 1,

    /// <summary>
    /// Path (vector graphics) object.
    /// </summary>
    Path = 2,

    /// <summary>
    /// Image object.
    /// </summary>
    Image = 3,

    /// <summary>
    /// Shading object.
    /// </summary>
    Shading = 4,

    /// <summary>
    /// Form XObject.
    /// </summary>
    Form = 5
}

/// <summary>
/// Represents basic information about a page object.
/// </summary>
public sealed class PdfPageObject
{
    /// <summary>
    /// Gets the zero-based index of this object on the page.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the type of this page object.
    /// </summary>
    public PdfPageObjectType ObjectType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfPageObject"/> class.
    /// </summary>
    internal PdfPageObject(int index, PdfPageObjectType objectType)
    {
        Index = index;
        ObjectType = objectType;
    }
}
