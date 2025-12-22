using System;
using System.IO;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a PDF font that can be used for text rendering.
/// Must be disposed properly to free native resources.
/// </summary>
public sealed unsafe class PdfFont : IDisposable
{
    private FpdfFontT? _handle;
    private readonly PdfDocument _document;
    private bool _disposed;

    /// <summary>
    /// Gets the base font name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfFont"/> class.
    /// Internal constructor - use factory methods to create fonts.
    /// </summary>
    internal PdfFont(FpdfFontT handle, PdfDocument document, string name)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        _document = document ?? throw new ArgumentNullException(nameof(document));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Loads a standard PDF font (14 built-in fonts).
    /// </summary>
    /// <param name="document">The document to load the font into.</param>
    /// <param name="fontName">Standard font name (e.g., "Helvetica", "Times-Roman", "Courier").</param>
    /// <returns>A new <see cref="PdfFont"/> instance.</returns>
    /// <exception cref="ArgumentNullException">document or fontName is null.</exception>
    /// <exception cref="PdfException">Failed to load the font.</exception>
    public static PdfFont Load(PdfDocument document, string fontName)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));
        if (fontName is null)
            throw new ArgumentNullException(nameof(fontName));

        document.ThrowIfDisposed();

        var handle = fpdf_edit.FPDFTextLoadStandardFont(document._handle!, fontName);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException($"Failed to load standard font '{fontName}'.");
        }

        return new PdfFont(handle, document, fontName);
    }

    /// <summary>
    /// Loads a standard PDF font using the predefined enum values.
    /// </summary>
    /// <param name="document">The document to load the font into.</param>
    /// <param name="font">The standard font to load.</param>
    /// <returns>A new <see cref="PdfFont"/> instance.</returns>
    /// <exception cref="ArgumentNullException">document is null.</exception>
    /// <exception cref="PdfException">Failed to load the font.</exception>
    public static PdfFont Load(PdfDocument document, PdfStandardFont font)
    {
        var fontName = font switch
        {
            PdfStandardFont.Helvetica => "Helvetica",
            PdfStandardFont.HelveticaBold => "Helvetica-Bold",
            PdfStandardFont.HelveticaOblique => "Helvetica-Oblique",
            PdfStandardFont.HelveticaBoldOblique => "Helvetica-BoldOblique",
            PdfStandardFont.TimesRoman => "Times-Roman",
            PdfStandardFont.TimesBold => "Times-Bold",
            PdfStandardFont.TimesItalic => "Times-Italic",
            PdfStandardFont.TimesBoldItalic => "Times-BoldItalic",
            PdfStandardFont.Courier => "Courier",
            PdfStandardFont.CourierBold => "Courier-Bold",
            PdfStandardFont.CourierOblique => "Courier-Oblique",
            PdfStandardFont.CourierBoldOblique => "Courier-BoldOblique",
            PdfStandardFont.Symbol => "Symbol",
            PdfStandardFont.ZapfDingbats => "ZapfDingbats",
            _ => throw new ArgumentException($"Unknown font type: {font}", nameof(font))
        };

        return Load(document, fontName);
    }

    /// <summary>
    /// Loads a TrueType font from byte data.
    /// </summary>
    /// <param name="document">The document to load the font into.</param>
    /// <param name="fontData">The TrueType font file data.</param>
    /// <param name="isCidFont">True if this is a CID font (for Asian languages), false for standard TrueType.</param>
    /// <returns>A new <see cref="PdfFont"/> instance.</returns>
    /// <exception cref="ArgumentNullException">document or fontData is null.</exception>
    /// <exception cref="ArgumentException">fontData is empty.</exception>
    /// <exception cref="PdfException">Failed to load the font.</exception>
    public static PdfFont Load(PdfDocument document, byte[] fontData, bool isCidFont = false)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));
        if (fontData is null)
            throw new ArgumentNullException(nameof(fontData));
        if (fontData.Length == 0)
            throw new ArgumentException("Font data cannot be empty.", nameof(fontData));

        document.ThrowIfDisposed();

        FpdfFontT handle;
        fixed (byte* pData = fontData)
        {
            // font_type: 1 = TrueType, 2 = Type1
            int fontType = 1; // TrueType
            int cid = isCidFont ? 1 : 0;
            handle = fpdf_edit.FPDFTextLoadFont(document._handle!, pData, (uint)fontData.Length, fontType, cid);
        }

        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to load TrueType font from byte data.");
        }

        return new PdfFont(handle, document, "CustomFont");
    }

    /// <summary>
    /// Loads a TrueType font from a file path.
    /// </summary>
    /// <param name="document">The document to load the font into.</param>
    /// <param name="filePath">Path to the TrueType font file (.ttf, .otf).</param>
    /// <param name="isCidFont">True if this is a CID font (for Asian languages), false for standard TrueType.</param>
    /// <returns>A new <see cref="PdfFont"/> instance.</returns>
    /// <exception cref="ArgumentNullException">document or filePath is null.</exception>
    /// <exception cref="FileNotFoundException">Font file does not exist.</exception>
    /// <exception cref="PdfException">Failed to load the font.</exception>
    public static PdfFont Load(PdfDocument document, string filePath, bool isCidFont)
    {
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Font file not found: {filePath}", filePath);

        var fontData = File.ReadAllBytes(filePath);
        return Load(document, fontData, isCidFont);
    }

    /// <summary>
    /// Gets the internal font handle.
    /// </summary>
    internal FpdfFontT Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle!;
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="PdfFont"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        if (_handle is not null)
        {
            fpdf_edit.FPDFFontClose(_handle);
            _handle = null;
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PdfFont));
    }

    /// <summary>
    /// Returns a string representation of the font.
    /// </summary>
    public override string ToString()
    {
        return $"PdfFont: {Name}";
    }
}

/// <summary>
/// Standard PDF fonts (14 built-in fonts that don't require embedding).
/// </summary>
public enum PdfStandardFont
{
    /// <summary>Helvetica (sans-serif, normal weight)</summary>
    Helvetica,

    /// <summary>Helvetica Bold</summary>
    HelveticaBold,

    /// <summary>Helvetica Oblique (italic)</summary>
    HelveticaOblique,

    /// <summary>Helvetica Bold Oblique</summary>
    HelveticaBoldOblique,

    /// <summary>Times-Roman (serif, normal weight)</summary>
    TimesRoman,

    /// <summary>Times Bold</summary>
    TimesBold,

    /// <summary>Times Italic</summary>
    TimesItalic,

    /// <summary>Times Bold Italic</summary>
    TimesBoldItalic,

    /// <summary>Courier (monospace, normal weight)</summary>
    Courier,

    /// <summary>Courier Bold</summary>
    CourierBold,

    /// <summary>Courier Oblique</summary>
    CourierOblique,

    /// <summary>Courier Bold Oblique</summary>
    CourierBoldOblique,

    /// <summary>Symbol font</summary>
    Symbol,

    /// <summary>ZapfDingbats (decorative symbols)</summary>
    ZapfDingbats
}
