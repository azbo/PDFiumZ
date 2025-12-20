using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents the type of a PDF annotation.
/// Based on the PDF specification (ISO 32000) and PDFium annotation types.
/// </summary>
public enum PdfAnnotationType
{
    /// <summary>
    /// Unknown or unsupported annotation type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Text annotation (sticky note/comment).
    /// </summary>
    Text = 1,

    /// <summary>
    /// Link annotation (hyperlink).
    /// </summary>
    Link = 2,

    /// <summary>
    /// Free text annotation (text box with border).
    /// </summary>
    FreeText = 3,

    /// <summary>
    /// Line annotation.
    /// </summary>
    Line = 4,

    /// <summary>
    /// Square (rectangle) annotation.
    /// </summary>
    Square = 5,

    /// <summary>
    /// Circle (ellipse) annotation.
    /// </summary>
    Circle = 6,

    /// <summary>
    /// Polygon annotation.
    /// </summary>
    Polygon = 7,

    /// <summary>
    /// Polyline annotation.
    /// </summary>
    PolyLine = 8,

    /// <summary>
    /// Highlight annotation.
    /// </summary>
    Highlight = 9,

    /// <summary>
    /// Underline annotation.
    /// </summary>
    Underline = 10,

    /// <summary>
    /// Squiggly underline annotation.
    /// </summary>
    Squiggly = 11,

    /// <summary>
    /// Strikeout annotation.
    /// </summary>
    StrikeOut = 12,

    /// <summary>
    /// Stamp annotation (rubber stamp).
    /// </summary>
    Stamp = 13,

    /// <summary>
    /// Caret annotation.
    /// </summary>
    Caret = 14,

    /// <summary>
    /// Ink annotation (freehand drawing).
    /// </summary>
    Ink = 15,

    /// <summary>
    /// Popup annotation (associated with parent annotation).
    /// </summary>
    Popup = 16,

    /// <summary>
    /// File attachment annotation.
    /// </summary>
    FileAttachment = 17,

    /// <summary>
    /// Sound annotation.
    /// </summary>
    Sound = 18,

    /// <summary>
    /// Movie annotation.
    /// </summary>
    Movie = 19,

    /// <summary>
    /// Widget annotation (form field).
    /// </summary>
    Widget = 20,

    /// <summary>
    /// Screen annotation.
    /// </summary>
    Screen = 21,

    /// <summary>
    /// Printer mark annotation.
    /// </summary>
    PrinterMark = 22,

    /// <summary>
    /// Trap network annotation.
    /// </summary>
    TrapNet = 23,

    /// <summary>
    /// Watermark annotation.
    /// </summary>
    Watermark = 24,

    /// <summary>
    /// 3D annotation.
    /// </summary>
    ThreeD = 25,

    /// <summary>
    /// Redaction annotation.
    /// </summary>
    Redact = 26,

    /// <summary>
    /// XFA widget annotation.
    /// </summary>
    XfaWidget = 27
}
