using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Thrown when a PDF document fails to load.
/// </summary>
public class PdfLoadException : PdfException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLoadException"/> class.
    /// </summary>
    public PdfLoadException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLoadException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PdfLoadException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLoadException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public PdfLoadException(string message, Exception inner) : base(message, inner) { }
}
