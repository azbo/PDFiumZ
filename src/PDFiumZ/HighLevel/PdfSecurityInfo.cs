using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Contains security and encryption information about a PDF document.
/// </summary>
public sealed class PdfSecurityInfo
{
    /// <summary>
    /// Gets whether the document is encrypted (password-protected).
    /// </summary>
    public bool IsEncrypted { get; }

    /// <summary>
    /// Gets the security handler revision number.
    /// Returns -1 if the document is not encrypted.
    /// Common values: 2 (40-bit RC4), 3 (128-bit RC4), 4 (128-bit AES), 5 (256-bit AES).
    /// </summary>
    public int SecurityHandlerRevision { get; }

    /// <summary>
    /// Gets the document permissions (what operations are allowed).
    /// For unencrypted documents, this returns All permissions.
    /// </summary>
    public PdfPermissions Permissions { get; }

    /// <summary>
    /// Gets the user permissions specifically.
    /// This may differ from Permissions if the document was opened with owner password.
    /// </summary>
    public PdfPermissions UserPermissions { get; }

    /// <summary>
    /// Gets whether printing is allowed.
    /// For unencrypted documents, always returns true.
    /// </summary>
    public bool CanPrint => !IsEncrypted || Permissions.HasFlag(PdfPermissions.Print);

    /// <summary>
    /// Gets whether high-quality printing is allowed.
    /// For unencrypted documents, always returns true.
    /// </summary>
    public bool CanPrintHighQuality => !IsEncrypted || Permissions.HasFlag(PdfPermissions.PrintHighQuality);

    /// <summary>
    /// Gets whether modifying the document contents is allowed.
    /// For unencrypted documents, always returns true.
    /// </summary>
    public bool CanModifyContents => !IsEncrypted || Permissions.HasFlag(PdfPermissions.ModifyContents);

    /// <summary>
    /// Gets whether copying content (text and graphics) is allowed.
    /// For unencrypted documents, always returns true.
    /// </summary>
    public bool CanCopyContent => !IsEncrypted || Permissions.HasFlag(PdfPermissions.CopyContent);

    /// <summary>
    /// Gets whether adding or modifying annotations is allowed.
    /// For unencrypted documents, always returns true.
    /// </summary>
    public bool CanModifyAnnotations => !IsEncrypted || Permissions.HasFlag(PdfPermissions.ModifyAnnotations);

    /// <summary>
    /// Gets whether filling form fields is allowed.
    /// For unencrypted documents, always returns true.
    /// </summary>
    public bool CanFillForms => !IsEncrypted || Permissions.HasFlag(PdfPermissions.FillForms);

    /// <summary>
    /// Gets whether extracting content for accessibility purposes is allowed.
    /// For unencrypted documents, always returns true.
    /// </summary>
    public bool CanExtractForAccessibility => !IsEncrypted || Permissions.HasFlag(PdfPermissions.ExtractAccessibility);

    /// <summary>
    /// Gets whether assembling the document (rotating, inserting pages) is allowed.
    /// For unencrypted documents, always returns true.
    /// </summary>
    public bool CanAssembleDocument => !IsEncrypted || Permissions.HasFlag(PdfPermissions.AssembleDocument);

    /// <summary>
    /// Gets the encryption method description based on security handler revision.
    /// </summary>
    public string EncryptionMethod
    {
        get
        {
            if (!IsEncrypted)
                return "None";

            return SecurityHandlerRevision switch
            {
                2 => "40-bit RC4",
                3 => "128-bit RC4",
                4 => "128-bit AES",
                5 => "256-bit AES",
                6 => "256-bit AES (PDF 2.0)",
                _ => $"Unknown (Revision {SecurityHandlerRevision})"
            };
        }
    }

    internal PdfSecurityInfo(bool isEncrypted, int revision, uint permissions, uint userPermissions)
    {
        IsEncrypted = isEncrypted;
        SecurityHandlerRevision = revision;
        Permissions = (PdfPermissions)permissions;
        UserPermissions = (PdfPermissions)userPermissions;
    }

    /// <summary>
    /// Returns a string representation of the security information.
    /// </summary>
    public override string ToString()
    {
        if (!IsEncrypted)
            return "Not encrypted";

        return $"Encrypted ({EncryptionMethod}), Permissions: {Permissions}";
    }
}
