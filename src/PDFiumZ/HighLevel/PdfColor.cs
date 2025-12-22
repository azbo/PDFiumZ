using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Provides color utilities and common predefined colors for PDF operations.
/// Colors are represented in ARGB format (0xAARRGGBB).
/// </summary>
public static class PdfColor
{
    /// <summary>
    /// Creates an ARGB color from individual components.
    /// </summary>
    /// <param name="r">Red component (0-255)</param>
    /// <param name="g">Green component (0-255)</param>
    /// <param name="b">Blue component (0-255)</param>
    /// <param name="a">Alpha component (0-255). Default is 255 (fully opaque).</param>
    /// <returns>ARGB color value</returns>
    public static uint FromArgb(byte r, byte g, byte b, byte a = 255)
    {
        return (uint)((a << 24) | (r << 16) | (g << 8) | b);
    }

    /// <summary>
    /// Creates an ARGB color with specified opacity percentage.
    /// </summary>
    /// <param name="r">Red component (0-255)</param>
    /// <param name="g">Green component (0-255)</param>
    /// <param name="b">Blue component (0-255)</param>
    /// <param name="opacity">Opacity as percentage (0.0 = transparent, 1.0 = opaque)</param>
    /// <returns>ARGB color value</returns>
    public static uint FromRgb(byte r, byte g, byte b, double opacity = 1.0)
    {
        var alpha = (byte)Math.Max(0, Math.Min(255, opacity * 255));
        return FromArgb(r, g, b, alpha);
    }

    /// <summary>
    /// Creates a color from hex string (e.g., "#FF0000" or "FF0000" for red).
    /// </summary>
    /// <param name="hex">Hex color string (with or without # prefix)</param>
    /// <param name="opacity">Opacity as percentage (0.0 = transparent, 1.0 = opaque)</param>
    /// <returns>ARGB color value</returns>
    public static uint FromHex(string hex, double opacity = 1.0)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Hex string cannot be null or empty", nameof(hex));

        hex = hex.TrimStart('#');

        if (hex.Length != 6)
            throw new ArgumentException("Hex string must be 6 characters (RGB)", nameof(hex));

        var r = Convert.ToByte(hex.Substring(0, 2), 16);
        var g = Convert.ToByte(hex.Substring(2, 2), 16);
        var b = Convert.ToByte(hex.Substring(4, 2), 16);

        return FromRgb(r, g, b, opacity);
    }

    /// <summary>
    /// Adjusts the opacity of an existing ARGB color.
    /// </summary>
    /// <param name="color">Original ARGB color</param>
    /// <param name="opacity">New opacity as percentage (0.0 = transparent, 1.0 = opaque)</param>
    /// <returns>ARGB color with adjusted opacity</returns>
    public static uint WithOpacity(uint color, double opacity)
    {
        var alpha = (byte)Math.Max(0, Math.Min(255, opacity * 255));
        return (color & 0x00FFFFFF) | ((uint)alpha << 24);
    }

    #region Basic Colors
    /// <summary>Transparent (0x00000000)</summary>
    public static readonly uint Transparent = 0x00000000;

    /// <summary>Black (0xFF000000)</summary>
    public static readonly uint Black = 0xFF000000;

    /// <summary>White (0xFFFFFFFF)</summary>
    public static readonly uint White = 0xFFFFFFFF;

    /// <summary>Red (0xFFFF0000)</summary>
    public static readonly uint Red = 0xFFFF0000;

    /// <summary>Green (0xFF00FF00)</summary>
    public static readonly uint Green = 0xFF00FF00;

    /// <summary>Blue (0xFF0000FF)</summary>
    public static readonly uint Blue = 0xFF0000FF;

    /// <summary>Yellow (0xFFFFFF00)</summary>
    public static readonly uint Yellow = 0xFFFFFF00;

    /// <summary>Cyan (0xFF00FFFF)</summary>
    public static readonly uint Cyan = 0xFF00FFFF;

    /// <summary>Magenta (0xFFFF00FF)</summary>
    public static readonly uint Magenta = 0xFFFF00FF;
    #endregion

    #region Gray Scale
    /// <summary>Dark Gray (0xFF404040)</summary>
    public static readonly uint DarkGray = 0xFF404040;

    /// <summary>Gray (0xFF808080)</summary>
    public static readonly uint Gray = 0xFF808080;

    /// <summary>Light Gray (0xFFC0C0C0)</summary>
    public static readonly uint LightGray = 0xFFC0C0C0;

    /// <summary>Silver (0xFFC0C0C0)</summary>
    public static readonly uint Silver = 0xFFC0C0C0;
    #endregion

    #region Extended Colors
    /// <summary>Orange (0xFFFFA500)</summary>
    public static readonly uint Orange = 0xFFFFA500;

    /// <summary>Purple (0xFF800080)</summary>
    public static readonly uint Purple = 0xFF800080;

    /// <summary>Pink (0xFFFFC0CB)</summary>
    public static readonly uint Pink = 0xFFFFC0CB;

    /// <summary>Brown (0xFFA52A2A)</summary>
    public static readonly uint Brown = 0xFFA52A2A;

    /// <summary>Gold (0xFFFFD700)</summary>
    public static readonly uint Gold = 0xFFFFD700;

    /// <summary>Navy (0xFF000080)</summary>
    public static readonly uint Navy = 0xFF000080;

    /// <summary>Teal (0xFF008080)</summary>
    public static readonly uint Teal = 0xFF008080;

    /// <summary>Olive (0xFF808000)</summary>
    public static readonly uint Olive = 0xFF808000;

    /// <summary>Maroon (0xFF800000)</summary>
    public static readonly uint Maroon = 0xFF800000;

    /// <summary>Lime (0xFF00FF00)</summary>
    public static readonly uint Lime = 0xFF00FF00;

    /// <summary>Aqua (0xFF00FFFF)</summary>
    public static readonly uint Aqua = 0xFF00FFFF;

    /// <summary>Fuchsia (0xFFFF00FF)</summary>
    public static readonly uint Fuchsia = 0xFFFF00FF;
    #endregion

    #region Common PDF Colors
    /// <summary>Light Yellow for highlighting (0x80FFFF00)</summary>
    public static readonly uint HighlightYellow = 0x80FFFF00;

    /// <summary>Light Green for highlighting (0x8000FF00)</summary>
    public static readonly uint HighlightGreen = 0x8000FF00;

    /// <summary>Light Blue for highlighting (0x8000FFFF)</summary>
    public static readonly uint HighlightBlue = 0x8000FFFF;

    /// <summary>Light Pink for highlighting (0x80FFCCE5)</summary>
    public static readonly uint HighlightPink = 0x80FFCCE5;
    #endregion

    #region Shades
    /// <summary>Dark Red (0xFF8B0000)</summary>
    public static readonly uint DarkRed = 0xFF8B0000;

    /// <summary>Light Red (0xFFFFCCCC)</summary>
    public static readonly uint LightRed = 0xFFFFCCCC;

    /// <summary>Dark Green (0xFF006400)</summary>
    public static readonly uint DarkGreen = 0xFF006400;

    /// <summary>Light Green (0xFF90EE90)</summary>
    public static readonly uint LightGreen = 0xFF90EE90;

    /// <summary>Dark Blue (0xFF00008B)</summary>
    public static readonly uint DarkBlue = 0xFF00008B;

    /// <summary>Light Blue (0xFFADD8E6)</summary>
    public static readonly uint LightBlue = 0xFFADD8E6;
    #endregion
}
