using System;
using System.Collections.Generic;
using PDFiumZ.Utilities;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents a form field annotation in a PDF page.
/// Provides read and write access to form field properties.
/// Supports setting values, checking/unchecking boxes, and selecting options.
/// </summary>
public sealed unsafe class PdfFormField : PdfAnnotation
{
    private FpdfFormHandleT? _formHandle;
    private string? _name;
    private string? _alternateName;
    private string? _value;

    /// <summary>
    /// Gets the type of the form field.
    /// </summary>
    public PdfFormFieldType FieldType { get; }

    /// <summary>
    /// Gets or sets the field name (unique identifier within the document).
    /// </summary>
    public string Name
    {
        get
        {
            if (_name != null)
                return _name;

            ThrowIfDisposed();

            // Get name length first
            var length = fpdf_annot.FPDFAnnotGetFormFieldName(_formHandle!, _handle!, ref _dummyBuffer, 0);
            if (length <= 2)
            {
                _name = string.Empty;
                return _name;
            }

            // Allocate buffer and get name
            var buffer = new ushort[length / 2];
            fixed (ushort* pBuffer = buffer)
            {
                fpdf_annot.FPDFAnnotGetFormFieldName(_formHandle!, _handle!, ref buffer[0], length);
                _name = new string((char*)pBuffer, 0, (int)(length / 2) - 1);
            }

            return _name;
        }
        // TODO: Implement set if PDFium supports renaming fields
    }

    /// <summary>
    /// Gets or sets the alternate field name (user-friendly name, often used as label).
    /// </summary>
    public string AlternateName
    {
        get
        {
            if (_alternateName != null)
                return _alternateName;

            ThrowIfDisposed();

            var length = fpdf_annot.FPDFAnnotGetFormFieldAlternateName(_formHandle!, _handle!, ref _dummyBuffer, 0);
            if (length <= 2)
            {
                _alternateName = string.Empty;
                return _alternateName;
            }

            var buffer = new ushort[length / 2];
            fixed (ushort* pBuffer = buffer)
            {
                fpdf_annot.FPDFAnnotGetFormFieldAlternateName(_formHandle!, _handle!, ref buffer[0], length);
                _alternateName = new string((char*)pBuffer, 0, (int)(length / 2) - 1);
            }

            return _alternateName;
        }
        // TODO: Implement set if PDFium supports changing alternate name
    }

    /// <summary>
    /// Gets or sets the current value of the form field (for text fields, combo boxes, etc.).
    /// Returns empty string if field has no value or is not a value-based field.
    /// </summary>
    public string Value
    {
        get
        {
            if (_value != null)
                return _value;

            ThrowIfDisposed();

            var length = fpdf_annot.FPDFAnnotGetFormFieldValue(_formHandle!, _handle!, ref _dummyBuffer, 0);
            if (length <= 2)
            {
                _value = string.Empty;
                return _value;
            }

            var buffer = new ushort[length / 2];
            fixed (ushort* pBuffer = buffer)
            {
                fpdf_annot.FPDFAnnotGetFormFieldValue(_formHandle!, _handle!, ref buffer[0], length);
                _value = new string((char*)pBuffer, 0, (int)(length / 2) - 1);
            }

            return _value;
        }
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            ThrowIfDisposed();

            // Convert to UTF-16LE (ushort array)
            var utf16Array = value.ToNullTerminatedUtf16Array();

            // Set the "V" (Value) key using FPDFAnnotSetStringValue
            var result = fpdf_annot.FPDFAnnotSetStringValue(_handle!, "V", ref utf16Array[0]);
            if (result == 0)
            {
                throw new PdfException("Failed to set form field value.");
            }

            // Clear cached value
            _value = null;

            // Update the annotation's appearance
            fpdf_annot.FPDFAnnotSetAP(_handle!, 0, ref utf16Array[0]); // 0 = Normal appearance
        }
    }

    /// <summary>
    /// Gets or sets whether this is a checkbox or radio button that is currently checked.
    /// Returns false for other field types.
    /// </summary>
    public bool IsChecked
    {
        get
        {
            ThrowIfDisposed();
            return fpdf_annot.FPDFAnnotIsChecked(_formHandle!, _handle!) != 0;
        }
        set
        {
            ThrowIfDisposed();

            if (FieldType != PdfFormFieldType.CheckBox && FieldType != PdfFormFieldType.RadioButton)
            {
                throw new InvalidOperationException($"IsChecked can only be set on CheckBox or RadioButton fields. Current field type: {FieldType}");
            }

            // For checkboxes and radio buttons, we set the appearance state (AS)
            // "Yes" or "Off" are common values, but we need to check what values this field supports
            var stateValue = value ? "Yes" : "Off";
            var utf16Array = stateValue.ToNullTerminatedUtf16Array();

            var result = fpdf_annot.FPDFAnnotSetStringValue(_handle!, "AS", ref utf16Array[0]);
            if (result == 0)
            {
                throw new PdfException("Failed to set checkbox/radio button state.");
            }
        }
    }

    /// <summary>
    /// Gets the form field flags.
    /// </summary>
    public int Flags
    {
        get
        {
            ThrowIfDisposed();
            return fpdf_annot.FPDFAnnotGetFormFieldFlags(_formHandle!, _handle!);
        }
    }

    private ushort _dummyBuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfFormField"/> class.
    /// Internal constructor - created by PdfPage only.
    /// </summary>
    internal PdfFormField(FpdfAnnotationT annotHandle, FpdfFormHandleT formHandle, PdfPage page, int index)
        : base(annotHandle, page, PdfAnnotationType.Widget, index)
    {
        _formHandle = formHandle ?? throw new ArgumentNullException(nameof(formHandle));

        // Get field type
        var typeValue = fpdf_annot.FPDFAnnotGetFormFieldType(_formHandle, _handle!);
        FieldType = (PdfFormFieldType)typeValue;
    }

    /// <summary>
    /// Gets the number of options for combo boxes and list boxes.
    /// Returns 0 for other field types.
    /// </summary>
    public int GetOptionCount()
    {
        ThrowIfDisposed();
        return fpdf_annot.FPDFAnnotGetOptionCount(_formHandle!, _handle!);
    }

    /// <summary>
    /// Gets the label text for an option at the specified index (for combo boxes and list boxes).
    /// </summary>
    /// <param name="index">Zero-based option index.</param>
    /// <returns>The option label text.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range.</exception>
    public string GetOptionLabel(int index)
    {
        ThrowIfDisposed();

        var optionCount = GetOptionCount();
        if (index < 0 || index >= optionCount)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Option index {index} is out of range (0-{optionCount - 1}).");

        var length = fpdf_annot.FPDFAnnotGetOptionLabel(_formHandle!, _handle!, index, ref _dummyBuffer, 0);
        if (length <= 2)
            return string.Empty;

        var buffer = new ushort[length / 2];
        fixed (ushort* pBuffer = buffer)
        {
            fpdf_annot.FPDFAnnotGetOptionLabel(_formHandle!, _handle!, index, ref buffer[0], length);
            return new string((char*)pBuffer, 0, (int)(length / 2) - 1);
        }
    }

    /// <summary>
    /// Gets all option labels for combo boxes and list boxes.
    /// </summary>
    /// <returns>An array of option labels.</returns>
    public string[] GetAllOptions()
    {
        var count = GetOptionCount();
        if (count == 0)
            return Array.Empty<string>();

        var options = new string[count];
        for (int i = 0; i < count; i++)
        {
            options[i] = GetOptionLabel(i);
        }

        return options;
    }

    /// <summary>
    /// Checks if an option at the specified index is selected (for list boxes).
    /// </summary>
    /// <param name="index">Zero-based option index.</param>
    /// <returns>True if the option is selected, false otherwise.</returns>
    public bool IsOptionSelected(int index)
    {
        ThrowIfDisposed();
        return fpdf_annot.FPDFAnnotIsOptionSelected(_formHandle!, _handle!, index) != 0;
    }

    /// <summary>
    /// Returns a string representation of the form field.
    /// </summary>
    public override string ToString()
    {
        var displayName = !string.IsNullOrEmpty(AlternateName) ? AlternateName : Name;
        return $"{FieldType}: {displayName} = {Value}";
    }

    #region Write Operations

    /// <summary>
    /// Sets the selected options for combo boxes and list boxes.
    /// </summary>
    /// <param name="indices">Zero-based indices of the options to select.</param>
    /// <exception cref="ArgumentNullException">indices is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Any index is out of range.</exception>
    /// <exception cref="ObjectDisposedException">The form field has been disposed.</exception>
    /// <exception cref="InvalidOperationException">This field is not a combo box or list box.</exception>
    /// <exception cref="PdfException">Failed to select options.</exception>
    public void SetSelectedOptions(params int[] indices)
    {
        if (indices is null)
            throw new ArgumentNullException(nameof(indices));

        ThrowIfDisposed();

        if (FieldType != PdfFormFieldType.ComboBox && FieldType != PdfFormFieldType.ListBox)
        {
            throw new InvalidOperationException($"SetSelectedOptions can only be called on ComboBox or ListBox fields. Current field type: {FieldType}");
        }

        var optionCount = GetOptionCount();

        // For combo boxes and single-selection list boxes, we only support one selection
        var isMultiSelect = (Flags & 0x00200000) != 0; // bit 22: MultiSelect

        if (!isMultiSelect && indices.Length > 1)
        {
            throw new InvalidOperationException("This field does not support multiple selections.");
        }

        // First, deselect all options (only for list boxes, combo boxes only have one selection anyway)
        if (FieldType == PdfFormFieldType.ListBox)
        {
            for (int i = 0; i < optionCount; i++)
            {
                fpdf_formfill.FORM_SetIndexSelected(_formHandle!, _page._handle!, i, 0);
            }
        }

        // Then select the specified indices
        foreach (var index in indices)
        {
            if (index < 0 || index >= optionCount)
                throw new ArgumentOutOfRangeException(nameof(indices),
                    $"Option index {index} is out of range (0-{optionCount - 1}).");

            var result = fpdf_formfill.FORM_SetIndexSelected(_formHandle!, _page._handle!, index, 1);
            if (result == 0)
            {
                throw new PdfException($"Failed to select option at index {index}.");
            }
        }

        // Clear cached value
        _value = null;
    }

    #endregion

    /// <summary>
    /// Releases the native resources used by the <see cref="PdfFormField"/>.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (_formHandle != null)
        {
            _page._document.DestroyFormHandle(_formHandle);
            _formHandle = null;
        }

        base.Dispose(disposing);
    }
}
