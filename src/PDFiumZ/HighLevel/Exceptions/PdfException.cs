using System;

namespace PDFiumCore.HighLevel;

/// <summary>
/// Base exception for all PDFium-related errors.
/// </summary>
public class PdfException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfException"/> class.
    /// </summary>
    public PdfException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PdfException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public PdfException(string message, Exception inner) : base(message, inner) { }
}
