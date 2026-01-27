using PDFiumZ.HighLevel;
using SkiaSharp;
using System;
using System.Threading.Tasks;

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
            using var openDoc = PdfDocument.Open("demo_output.pdf");
            using var openPage = openDoc.GetPage(0);
            
            // 5. Extract text
            var text = openPage.ExtractText();
            Console.WriteLine($"Extracted text: {text}");
            
            // 6. Render to image
            Console.WriteLine("Rendering to image...");
            using var image = openPage.RenderToImage();

            // Save using SkiaSharp
            var pixelData = image.ToByteArray();
            var info = new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var skImage = SKImage.FromPixelCopy(info, pixelData);

            using var data = skImage.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = System.IO.File.OpenWrite("demo_output.png");
            data.SaveTo(stream);

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