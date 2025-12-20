using PDFiumCore;
using PDFiumCore.HighLevel;

PdfiumLibrary.Initialize();
try
{
    using var doc = PdfDocument.Open("output-modified.pdf");
    Console.WriteLine($"Pages: {doc.PageCount}");
    
    for (int i = 0; i < doc.PageCount; i++)
    {
        using var page = doc.GetPage(i);
        var text = page.ExtractText();
        Console.WriteLine($"\nPage {i + 1} ({page.Width:F0}x{page.Height:F0}):");
        Console.WriteLine($"  Text length: {text.Length} chars");
        if (text.Length > 0)
            Console.WriteLine($"  Preview: {text.Substring(0, Math.Min(80, text.Length))}...");
        else
            Console.WriteLine($"  (Blank page)");
    }
}
finally
{
    PdfiumLibrary.Shutdown();
}
