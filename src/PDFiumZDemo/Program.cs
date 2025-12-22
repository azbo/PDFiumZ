using System;
using System.IO;
using System.Threading.Tasks;
using PDFiumZ;
using PDFiumZ.HighLevel;
using PDFiumZ.SkiaSharp;

namespace PDFiumZDemo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Initializing PDFiumZ...");
        PdfiumLibrary.Initialize();

        try
        {
            // 1. Create a new document
            Console.WriteLine("Creating new document...");
            using var doc = PdfDocument.CreateNew();
            
            // 2. Add a page
            using var page = doc.CreatePage();
            
            // 3. Add text content using Fluent API
            using var font = PdfFont.Load(doc, PdfStandardFont.Helvetica);
            using var editor = page.BeginEdit();
            
            editor
                .WithFont(font)
                .WithFontSize(24)
                .WithTextColor(PdfColor.DarkBlue)
                .Text("Hello, PDFiumZ!", 100, 700)
                
                .WithFontSize(12)
                .WithTextColor(PdfColor.Black)
                .Text($"Generated on: {DateTime.Now}", 100, 650)
                
                // Draw some shapes
                .WithStrokeColor(PdfColor.Red)
                .WithLineWidth(2)
                .Rectangle(new PdfRectangle(100, 500, 200, 100))
                
                .Commit();
                
            Console.WriteLine("Saving generated document...");
            doc.Save("demo_output.pdf");

            // 4. Open and process the generated document
            Console.WriteLine("Opening document for processing...");
            using var openDoc = await PdfDocument.OpenAsync("demo_output.pdf");
            using var openPage = openDoc.GetPage(0);
            
            // 5. Extract text
            var text = await openPage.ExtractTextAsync();
            Console.WriteLine($"Extracted text: {text}");
            
            // 6. Render to image
            Console.WriteLine("Rendering to image...");
            using var image = await openPage.RenderToImageAsync();
            image.SaveAsSkiaPng("demo_output.png");
            
            Console.WriteLine("Done! Check demo_output.pdf and demo_output.png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            PdfiumLibrary.Shutdown();
        }
    }
}
