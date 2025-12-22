using PDFiumZ;
using PDFiumZ.HighLevel;
using SkiaSharp;
using System;

PdfiumLibrary.Initialize();

try
{
    // Create test image
    using (var surface = SKSurface.Create(new SKImageInfo(200, 100)))
    {
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Red);
        using var font = new SKFont { Size = 24 };
        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawText("TEST", 70, 60, font, paint);
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = System.IO.File.OpenWrite("output/direct-test.png");
        data.SaveTo(stream);
    }
    
    // Load and convert to BGRA
    using var bitmap = SKBitmap.Decode("output/direct-test.png");
    int width = bitmap.Width;
    int height = bitmap.Height;
    byte[] bgraData = new byte[width * height * 4];
    
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            var pixel = bitmap.GetPixel(x, y);
            int index = (y * width + x) * 4;
            bgraData[index + 0] = pixel.Blue;
            bgraData[index + 1] = pixel.Green;
            bgraData[index + 2] = pixel.Red;
            bgraData[index + 3] = pixel.Alpha;
        }
    }
    
    // Create PDF
    using var document = PdfDocument.CreateNew();
    using var page = document.CreatePage(595, 842);
    
    using (var editor = page.BeginEdit())
    {
        var helvetica = PdfFont.Load(document, PdfStandardFont.Helvetica);
        editor.AddText("Direct Image Test", 50, 750, helvetica, 24);
        
        // Add image directly
        var bounds = new PdfRectangle(50, 600, 200, 100);
        editor.AddImage(bgraData, width, height, bounds);
        
        helvetica.Dispose();
        editor.GenerateContent();
    }
    
    document.Save("output/direct-image-test.pdf");
    Console.WriteLine("Saved: direct-image-test.pdf");
}
finally
{
    PdfiumLibrary.Shutdown();
}

