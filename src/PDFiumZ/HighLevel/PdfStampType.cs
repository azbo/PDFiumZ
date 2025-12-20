using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents the type of PDF stamp annotation.
/// Based on standard PDF stamp types defined in the PDF specification.
/// </summary>
public enum PdfStampType
{
    /// <summary>
    /// Approved stamp.
    /// </summary>
    Approved = 0,

    /// <summary>
    /// Experimental stamp.
    /// </summary>
    Experimental = 1,

    /// <summary>
    /// Not Approved stamp.
    /// </summary>
    NotApproved = 2,

    /// <summary>
    /// As Is stamp.
    /// </summary>
    AsIs = 3,

    /// <summary>
    /// Expired stamp.
    /// </summary>
    Expired = 4,

    /// <summary>
    /// Not For Public Release stamp.
    /// </summary>
    NotForPublicRelease = 5,

    /// <summary>
    /// Confidential stamp.
    /// </summary>
    Confidential = 6,

    /// <summary>
    /// Final stamp.
    /// </summary>
    Final = 7,

    /// <summary>
    /// Sold stamp.
    /// </summary>
    Sold = 8,

    /// <summary>
    /// Departmental stamp.
    /// </summary>
    Departmental = 9,

    /// <summary>
    /// For Comment stamp.
    /// </summary>
    ForComment = 10,

    /// <summary>
    /// Top Secret stamp.
    /// </summary>
    TopSecret = 11,

    /// <summary>
    /// For Public Release stamp.
    /// </summary>
    ForPublicRelease = 12,

    /// <summary>
    /// Draft stamp.
    /// </summary>
    Draft = 13
}
