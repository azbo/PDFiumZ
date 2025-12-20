using PDFiumCore;
using PDFiumCore.HighLevel;
using PDFiumCore.SkiaSharp;
using System;
using System.Linq;

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
            DemoTextSearch();
            DemoImageExtraction();
            DemoMetadata();
            DemoPageLabels();
            DemoPageManipulation();
            DemoFormFields();

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
        /// Demonstrates text search functionality.
        /// </summary>
        static void DemoTextSearch()
        {
            Console.WriteLine("4. Text Search");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.Open("pdf-sample.pdf");
                using var page = document.GetPage(0);

                // Search for text (case-insensitive)
                var searchTerm = "PDF";
                var results = page.SearchText(searchTerm, matchCase: false);

                Console.WriteLine($"   Searching for '{searchTerm}' (case-insensitive):");
                Console.WriteLine($"   Found {results.Count} occurrence(s)");

                foreach (var result in results)
                {
                    Console.WriteLine($"      Match at char {result.CharIndex}: \"{result.Text}\"");
                    Console.WriteLine($"      Bounding boxes: {result.BoundingRectangles.Count}");

                    foreach (var rect in result.BoundingRectangles)
                    {
                        Console.WriteLine($"         {rect}");
                    }
                }

                // Search with case sensitivity
                var caseSensitiveResults = page.SearchText(searchTerm, matchCase: true);
                Console.WriteLine($"\n   Searching for '{searchTerm}' (case-sensitive):");
                Console.WriteLine($"   Found {caseSensitiveResults.Count} occurrence(s)\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates image extraction from PDF pages.
        /// </summary>
        static void DemoImageExtraction()
        {
            Console.WriteLine("5. Image Extraction");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.Open("pdf-sample.pdf");
                using var page = document.GetPage(0);

                // Get page object count
                var objectCount = page.GetPageObjectCount();
                Console.WriteLine($"   Page has {objectCount} object(s)");

                // Extract all images
                var images = page.ExtractImages();
                Console.WriteLine($"   Found {images.Count} image(s)");

                int imageIndex = 0;
                foreach (var extractedImage in images)
                {
                    using (extractedImage)
                    {
                        Console.WriteLine($"\n   Image {imageIndex + 1}:");
                        Console.WriteLine($"      Object index: {extractedImage.ObjectIndex}");
                        Console.WriteLine($"      Dimensions: {extractedImage.Width} x {extractedImage.Height} pixels");

                        // Save extracted image
                        var filename = $"extracted-image-{imageIndex + 1}.png";
                        extractedImage.Image.SaveAsSkiaPng(filename);
                        Console.WriteLine($"      Saved: {filename}");

                        imageIndex++;
                    }
                }

                if (images.Count == 0)
                {
                    Console.WriteLine("   (Note: pdf-sample.pdf may not contain embedded images)");
                }

                Console.WriteLine();
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates document metadata access.
        /// </summary>
        static void DemoMetadata()
        {
            Console.WriteLine("6. Document Metadata");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.Open("pdf-sample.pdf");
                var meta = document.Metadata;

                Console.WriteLine($"   Title: {(string.IsNullOrEmpty(meta.Title) ? "(none)" : meta.Title)}");
                Console.WriteLine($"   Author: {(string.IsNullOrEmpty(meta.Author) ? "(none)" : meta.Author)}");
                Console.WriteLine($"   Subject: {(string.IsNullOrEmpty(meta.Subject) ? "(none)" : meta.Subject)}");
                Console.WriteLine($"   Keywords: {(string.IsNullOrEmpty(meta.Keywords) ? "(none)" : meta.Keywords)}");
                Console.WriteLine($"   Creator: {(string.IsNullOrEmpty(meta.Creator) ? "(none)" : meta.Creator)}");
                Console.WriteLine($"   Producer: {(string.IsNullOrEmpty(meta.Producer) ? "(none)" : meta.Producer)}");
                Console.WriteLine($"   Creation Date: {meta.CreationDate ?? "(none)"}");
                Console.WriteLine($"   Modification Date: {meta.ModificationDate ?? "(none)"}\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates page labels (custom page numbering).
        /// </summary>
        static void DemoPageLabels()
        {
            Console.WriteLine("7. Page Labels");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.Open("pdf-sample.pdf");

                Console.WriteLine($"   Document has {document.PageCount} page(s)");
                Console.WriteLine("   Page labels:");

                // Show labels for each page
                for (int i = 0; i < Math.Min(document.PageCount, 10); i++)
                {
                    var label = document.GetPageLabel(i);
                    Console.WriteLine($"      Page {i} → \"{label}\"");
                }

                if (document.PageCount > 10)
                {
                    Console.WriteLine($"      ... ({document.PageCount - 10} more pages)");
                }

                Console.WriteLine();
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates page manipulation operations.
        /// </summary>
        static void DemoPageManipulation()
        {
            Console.WriteLine("8. Page Manipulation");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.Open("pdf-sample.pdf");
                Console.WriteLine($"   Original: {document.PageCount} page(s)");

                // Insert a blank A4 page at the beginning
                document.InsertBlankPage(0, 595, 842);
                Console.WriteLine($"   After insert: {document.PageCount} page(s)");

                // Insert another blank page at the end
                document.InsertBlankPage(document.PageCount, 595, 842);
                Console.WriteLine($"   After 2nd insert: {document.PageCount} page(s)");

                // Delete the first blank page we inserted
                document.DeletePage(0);
                Console.WriteLine($"   After delete: {document.PageCount} page(s)");

                // Move last page to position 1 (after original content)
                if (document.PageCount > 1)
                {
                    document.MovePages(new[] { document.PageCount - 1 }, 1);
                    Console.WriteLine($"   Moved last page to position 1");
                }

                // Save modified document (should have: original page, blank page)
                document.SaveToFile("output-modified.pdf");
                Console.WriteLine($"   Saved: output-modified.pdf (original page + blank page)\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates form field reading (if PDF contains form fields).
        /// </summary>
        static void DemoFormFields()
        {
            Console.WriteLine("9. Form Field Reading");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.Open("pdf-sample.pdf");
                bool foundFormFields = false;

                // Iterate through all pages looking for form fields
                for (int pageNum = 0; pageNum < document.PageCount; pageNum++)
                {
                    using var page = document.GetPage(pageNum);
                    var formFields = page.GetFormFields().ToList();

                    if (formFields.Count > 0)
                    {
                        Console.WriteLine($"   Page {pageNum + 1}: Found {formFields.Count} form field(s)");
                        foundFormFields = true;

                        foreach (var field in formFields)
                        {
                            using (field)
                            {
                                Console.WriteLine($"      Type: {field.FieldType}");
                                Console.WriteLine($"      Name: {field.Name}");

                                if (!string.IsNullOrEmpty(field.AlternateName))
                                    Console.WriteLine($"      Label: {field.AlternateName}");

                                if (!string.IsNullOrEmpty(field.Value))
                                    Console.WriteLine($"      Value: {field.Value}");

                                if (field.FieldType == PdfFormFieldType.CheckBox || field.FieldType == PdfFormFieldType.RadioButton)
                                    Console.WriteLine($"      Checked: {field.IsChecked}");

                                if (field.FieldType == PdfFormFieldType.ComboBox || field.FieldType == PdfFormFieldType.ListBox)
                                {
                                    var options = field.GetAllOptions();
                                    if (options.Length > 0)
                                    {
                                        Console.WriteLine($"      Options: {string.Join(", ", options)}");
                                    }
                                }

                                Console.WriteLine();
                            }
                        }
                    }
                }

                if (!foundFormFields)
                {
                    Console.WriteLine("   No form fields found in this PDF");
                    Console.WriteLine("   (Note: pdf-sample.pdf is a simple document without forms)\n");
                }
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
