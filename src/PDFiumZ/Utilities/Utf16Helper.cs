using System;
using System.Runtime.InteropServices;

namespace PDFiumZ.Utilities;

/// <summary>
/// Provides optimized UTF-16 string conversion utilities for PDFium interop.
/// PDFium uses null-terminated UTF-16 (ushort[]) for string parameters.
/// </summary>
internal static class Utf16Helper
{
    /// <summary>
    /// Maximum size for stack allocation (characters).
    /// Strings longer than this will be heap-allocated.
    /// </summary>
    private const int StackAllocThreshold = 128;

    /// <summary>
    /// Converts a string to null-terminated UTF-16 on the stack (for small strings).
    /// Use this method when the string length is known to be small (&lt;128 characters).
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <param name="destination">The destination span to write to. Must have length >= text.Length + 1.</param>
    /// <exception cref="ArgumentException">Destination span is too small.</exception>
    /// <example>
    /// <code>
    /// Span&lt;ushort&gt; utf16 = stackalloc ushort[text.Length + 1];
    /// Utf16Helper.ToNullTerminatedUtf16(text, utf16);
    /// </code>
    /// </example>
    public static void ToNullTerminatedUtf16(this string text, Span<ushort> destination)
    {
        if (text == null)
        {
            if (destination.Length > 0)
                destination[0] = 0;
            return;
        }

        if (destination.Length < text.Length + 1)
            throw new ArgumentException(
                $"Destination span must have length >= {text.Length + 1} (text.Length + 1)",
                nameof(destination));

        // Copy string characters to ushort span
        text.AsSpan().CopyTo(MemoryMarshal.Cast<ushort, char>(destination));

        // Add null terminator
        destination[text.Length] = 0;
    }

    /// <summary>
    /// Converts a string to null-terminated UTF-16 array (heap allocation).
    /// Use this extension method for convenience when performance is not critical
    /// or when string length is unknown.
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <returns>A null-terminated UTF-16 array.</returns>
    /// <example>
    /// <code>
    /// var utf16 = searchText.ToNullTerminatedUtf16Array();
    /// var handle = fpdf_text.FPDFTextFindStart(textPage, ref utf16[0], flags, 0);
    /// </code>
    /// </example>
    public static ushort[] ToNullTerminatedUtf16Array(this string text)
    {
        if (text == null)
            return new ushort[] { 0 };

        var result = new ushort[text.Length + 1];

        // Copy string characters directly using MemoryMarshal for zero-copy
        text.AsSpan().CopyTo(MemoryMarshal.Cast<ushort, char>(result.AsSpan()));

        // Null terminator is already 0 by default in new array
        result[result.Length - 1] = 0;

        return result;
    }

    /// <summary>
    /// Converts a string to null-terminated UTF-16 with automatic stack/heap selection.
    /// Small strings (&lt;128 chars) use stack allocation, large strings use heap.
    /// This method requires a callback to use the UTF-16 buffer.
    /// </summary>
    /// <typeparam name="TResult">The result type of the operation.</typeparam>
    /// <param name="text">The string to convert.</param>
    /// <param name="action">The action to perform with the UTF-16 buffer. First parameter is ref to first element.</param>
    /// <returns>The result of the action.</returns>
    /// <example>
    /// <code>
    /// var result = text.UseNullTerminatedUtf16(
    ///     (ref ushort utf16) => fpdf_text.FPDFTextFindStart(textPage, ref utf16, flags, 0));
    /// </code>
    /// </example>
    public static TResult UseNullTerminatedUtf16<TResult>(
        this string text,
        Utf16Action<TResult> action)
    {
        if (text == null || text.Length == 0)
        {
            ushort zero = 0;
            return action(ref zero);
        }

        // Small strings: use stack allocation
        if (text.Length < StackAllocThreshold)
        {
            Span<ushort> stackBuffer = stackalloc ushort[text.Length + 1];
            text.ToNullTerminatedUtf16(stackBuffer);
            return action(ref MemoryMarshal.GetReference(stackBuffer));
        }

        // Large strings: use heap allocation
        var heapBuffer = text.ToNullTerminatedUtf16Array();
        return action(ref heapBuffer[0]);
    }

    /// <summary>
    /// Delegate for actions that use a UTF-16 buffer reference.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="utf16">Reference to the first element of the UTF-16 buffer.</param>
    public delegate TResult Utf16Action<TResult>(ref ushort utf16);
}
