using System;

namespace PDFiumCore.HighLevel;

/// <summary>
/// Thrown when a password is required or incorrect.
/// </summary>
public class PdfPasswordException : PdfLoadException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfPasswordException"/> class.
    /// </summary>
    public PdfPasswordException()
        : base("Password required or incorrect.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfPasswordException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PdfPasswordException(string message) : base(message) { }
}
