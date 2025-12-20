using System;

namespace PDFiumCore.HighLevel;

/// <summary>
/// Represents metadata information for a PDF document.
/// Metadata includes standard document properties like title, author, subject, etc.
/// </summary>
public sealed class PdfMetadata
{
    /// <summary>
    /// Gets the document title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the document author.
    /// </summary>
    public string Author { get; }

    /// <summary>
    /// Gets the document subject.
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// Gets the document keywords.
    /// </summary>
    public string Keywords { get; }

    /// <summary>
    /// Gets the name of the application that created the original document.
    /// </summary>
    public string Creator { get; }

    /// <summary>
    /// Gets the name of the application that produced the PDF (e.g., "PDFium").
    /// </summary>
    public string Producer { get; }

    /// <summary>
    /// Gets the document creation date in PDF date format (e.g., "D:20231225120000+08'00'").
    /// Returns null if not available.
    /// </summary>
    public string? CreationDate { get; }

    /// <summary>
    /// Gets the document modification date in PDF date format (e.g., "D:20231225120000+08'00'").
    /// Returns null if not available.
    /// </summary>
    public string? ModificationDate { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfMetadata"/> class.
    /// Internal constructor - created by PdfDocument only.
    /// </summary>
    internal PdfMetadata(
        string title,
        string author,
        string subject,
        string keywords,
        string creator,
        string producer,
        string? creationDate,
        string? modificationDate)
    {
        Title = title ?? string.Empty;
        Author = author ?? string.Empty;
        Subject = subject ?? string.Empty;
        Keywords = keywords ?? string.Empty;
        Creator = creator ?? string.Empty;
        Producer = producer ?? string.Empty;
        CreationDate = creationDate;
        ModificationDate = modificationDate;
    }
}
