using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF document permission flags as defined in PDF Reference 1.7, Table 3.20.
/// These flags control what operations are allowed on an encrypted PDF.
/// </summary>
[Flags]
public enum PdfPermissions : uint
{
    /// <summary>
    /// No permissions (fully restricted).
    /// </summary>
    None = 0,

    /// <summary>
    /// Print the document (security handlers revision 2).
    /// Bit 3 (0x4).
    /// </summary>
    Print = 0x00000004,

    /// <summary>
    /// Modify the contents of the document by operations other than those controlled
    /// by ModifyAnnotations, FillForms, and AssembleDocument. Bit 4 (0x8).
    /// </summary>
    ModifyContents = 0x00000008,

    /// <summary>
    /// Copy or otherwise extract text and graphics from the document. Bit 5 (0x10).
    /// </summary>
    CopyContent = 0x00000010,

    /// <summary>
    /// Add or modify text annotations, fill in interactive form fields. Bit 6 (0x20).
    /// If ModifyContents is also set, create or modify interactive form fields.
    /// </summary>
    ModifyAnnotations = 0x00000020,

    /// <summary>
    /// Fill in existing interactive form fields (including signature fields). Bit 9 (0x100).
    /// Security handlers revision 3 or greater.
    /// </summary>
    FillForms = 0x00000100,

    /// <summary>
    /// Extract text and graphics (in support of accessibility to users with disabilities
    /// or for other purposes). Bit 10 (0x200). Security handlers revision 3 or greater.
    /// </summary>
    ExtractAccessibility = 0x00000200,

    /// <summary>
    /// Assemble the document (insert, rotate, or delete pages and create bookmarks or thumbnail images).
    /// Bit 11 (0x400). Security handlers revision 3 or greater.
    /// </summary>
    AssembleDocument = 0x00000400,

    /// <summary>
    /// Print the document to a representation from which a faithful digital copy could be generated.
    /// Bit 12 (0x800). Security handlers revision 3 or greater.
    /// </summary>
    PrintHighQuality = 0x00000800,

    /// <summary>
    /// All permissions enabled.
    /// </summary>
    All = 0xFFFFFFFF
}
