using PDFiumCore;
using PDFiumCore.HighLevel;
using PDFiumCore.SkiaSharp;
using System;

namespace PDFiumCoreDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Demonstrate the new high-level API
            Console.WriteLine("=== PDFiumCore High-Level API Demo ===\n");

            DemoHighLevelAPI();
            DemoAdvancedRendering();
            DemoTextExtraction();

            Console.WriteLine("\nDemo completed successfully!");
        }

        /// <summary>
        /// Demonstrates the simple high-level API usage.
        /// </summary>
        static void DemoHighLevelAPI()
        {
            Console.WriteLine("1. Simple Rendering (High-Level API)");

            // Initialize PDFium library
            PdfiumLibrary.Initialize();

            try
            {
                // Open document - automatic resource management via 'using'
                using var document = PdfDocument.Open("pdf-sample.pdf");
                Console.WriteLine($"   Loaded: {document.PageCount} page(s)");

                // Get first page
                using var page = document.GetPage(0);
                Console.WriteLine($"   Page size: {page.Width:F1} x {page.Height:F1} points");

                // Render to image with default settings
                using var image = page.RenderToImage();
                Console.WriteLine($"   Rendered: {image.Width} x {image.Height} pixels");

                // Save as PNG using SkiaSharp extension
                image.SaveAsSkiaPng("output.png");
                Console.WriteLine("   Saved: output.png\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates advanced rendering options using fluent API.
        /// </summary>
        static void DemoAdvancedRendering()
        {
            Console.WriteLine("2. Advanced Rendering (Fluent Options)");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.Open("pdf-sample.pdf");
                using var page = document.GetPage(0);

                // High DPI rendering with fluent configuration
                var options = RenderOptions.Default
                    .WithDpi(150)  // 150 DPI (2.08x scale)
                    .WithTransparency()  // Transparent background
                    .AddFlags(RenderFlags.OptimizeTextForLcd);  // LCD text optimization

                using var image = page.RenderToImage(options);
                Console.WriteLine($"   High DPI: {image.Width} x {image.Height} pixels @ 150 DPI");

                image.SaveAsSkiaPng("output-hires.png");
                Console.WriteLine("   Saved: output-hires.png\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates text extraction.
        /// </summary>
        static void DemoTextExtraction()
        {
            Console.WriteLine("3. Text Extraction");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.Open("pdf-sample.pdf");
                using var page = document.GetPage(0);

                // Extract text content
                var text = page.ExtractText();
                Console.WriteLine($"   Extracted {text.Length} characters");
                Console.WriteLine($"   Preview: {text.Substring(0, Math.Min(100, text.Length))}...\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Legacy low-level API example (commented out).
        /// The high-level API above achieves the same result with much less code.
        /// </summary>
        /*
        static void RenderPageToImageLegacy()
        {
            double pageWidth = 0;
            double pageHeight = 0;
            float scale = 1;
            uint color = uint.MaxValue;

            var document = fpdfview.FPDF_LoadDocument("pdf-sample.pdf", null);
            var page = fpdfview.FPDF_LoadPage(document, 0);
            fpdfview.FPDF_GetPageSizeByIndex(document, 0, ref pageWidth, ref pageHeight);

            var viewport = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = pageWidth,
                Height = pageHeight,
            };

            var bitmap = fpdfview.FPDFBitmapCreateEx(
                (int)viewport.Width,
                (int)viewport.Height,
                (int)FPDFBitmapFormat.BGRA,
                IntPtr.Zero,
                0);

            if (bitmap == null)
                throw new Exception("failed to create a bitmap object");

            fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, (int)viewport.Width, (int)viewport.Height, color);

            using var matrix = new FS_MATRIX_();
            using var clipping = new FS_RECTF_();

            matrix.A = scale;
            matrix.B = 0;
            matrix.C = 0;
            matrix.D = scale;
            matrix.E = (float)-viewport.X;
            matrix.F = (float)-viewport.Y;

            clipping.Left = 0;
            clipping.Right = (float)viewport.Width;
            clipping.Bottom = 0;
            clipping.Top = (float)viewport.Height;

            fpdfview.FPDF_RenderPageBitmapWithMatrix(bitmap, page, matrix, clipping, (int)RenderFlags.RenderAnnotations);

            var image = new PdfImage(bitmap, (int)pageWidth, (int)pageHeight);

            using (var fileStream = System.IO.File.OpenWrite("output.png"))
            {
                image.ImageData.Encode(fileStream, SKEncodedImageFormat.Png, 100);
            }

            // Manual cleanup required (easy to forget!)
            fpdfview.FPDF_ClosePage(page);
            fpdfview.FPDF_CloseDocument(document);
        }
        */
    }
}
