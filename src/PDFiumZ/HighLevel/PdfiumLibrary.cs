using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Manages PDFium library initialization and cleanup.
/// Thread-safe singleton pattern with automatic reference counting.
/// </summary>
public static class PdfiumLibrary
{
    private static readonly object _lock = new();
    private static int _initCount = 0;
    private static bool _isInitialized = false;

    /// <summary>
    /// Gets whether the PDFium library is currently initialized.
    /// </summary>
    public static bool IsInitialized
    {
        get { lock (_lock) return _isInitialized; }
    }

    /// <summary>
    /// Initializes the PDFium library. Idempotent and reference-counted.
    /// Safe to call multiple times - internally tracked.
    /// </summary>
    public static void Initialize()
    {
        lock (_lock)
        {
            if (_initCount == 0)
            {
                fpdfview.FPDF_InitLibrary();
                _isInitialized = true;
            }
            _initCount++;
        }
    }

    /// <summary>
    /// Decrements reference count and destroys library when count reaches zero.
    /// Must be called once for each Initialize() call.
    /// </summary>
    /// <exception cref="InvalidOperationException">Shutdown called without matching Initialize.</exception>
    public static void Shutdown()
    {
        lock (_lock)
        {
            if (_initCount == 0)
                throw new InvalidOperationException("Shutdown called without matching Initialize.");

            _initCount--;
            if (_initCount == 0)
            {
                fpdfview.FPDF_DestroyLibrary();
                _isInitialized = false;
            }
        }
    }

    /// <summary>
    /// For testing/advanced scenarios: force reset internal state.
    /// WARNING: Only use if you know all documents are disposed.
    /// </summary>
    internal static void UnsafeReset()
    {
        lock (_lock)
        {
            _initCount = 0;
            _isInitialized = false;
        }
    }
}
