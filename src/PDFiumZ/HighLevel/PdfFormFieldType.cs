using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents the type of a form field.
/// </summary>
public enum PdfFormFieldType
{
    /// <summary>
    /// Unknown field type.
    /// </summary>
    Unknown = -1,

    /// <summary>
    /// Push button (action trigger).
    /// </summary>
    PushButton = 0,

    /// <summary>
    /// Checkbox (toggle on/off).
    /// </summary>
    CheckBox = 1,

    /// <summary>
    /// Radio button (select one from group).
    /// </summary>
    RadioButton = 2,

    /// <summary>
    /// Combo box (dropdown selection).
    /// </summary>
    ComboBox = 3,

    /// <summary>
    /// List box (list selection).
    /// </summary>
    ListBox = 4,

    /// <summary>
    /// Text field (single or multi-line text input).
    /// </summary>
    TextField = 5,

    /// <summary>
    /// Signature field (digital signature).
    /// </summary>
    Signature = 6
}
