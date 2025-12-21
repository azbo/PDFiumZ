using PDFiumZ;
using PDFiumZ.HighLevel;
using PDFiumZ.SkiaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PDFiumZDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create output directory for generated files
            System.IO.Directory.CreateDirectory("output");

            // Demonstrate the new high-level API
            Console.WriteLine("=== PDFiumZ High-Level API Demo ===\n");

            DemoCreateNewDocument();  // NEW: Test CreateNew + CreatePage
            DemoHighLevelAPI();
            DemoAdvancedRendering();
            DemoTextExtraction();
            DemoTextSearch();
            DemoImageExtraction();
            DemoMetadata();
            DemoPageLabels();
            DemoPageManipulation();
            DemoFormFields();
            DemoAnnotations();
            DemoContentCreation();
            await DemoAsyncOperations();
            DemoBatchOperations();

            Console.WriteLine("\nDemo completed successfully!");
        }

        /// <summary>
        /// Demonstrates creating a new PDF document from scratch.
        /// </summary>
        static void DemoCreateNewDocument()
        {
            Console.WriteLine("0. Create New PDF Document (From Scratch)");

            // Initialize PDFium library
            PdfiumLibrary.Initialize();

            try
            {
                // Create a new empty PDF document
                using var document = PdfDocument.CreateNew();
                Console.WriteLine("   Created new empty document");

                // Create an A4 page (595 x 842 points)
                using var page1 = document.CreatePage(595, 842);
                Console.WriteLine($"   Created page 1: {page1.Width:F1} x {page1.Height:F1} points (A4)");

                // Create a Letter size page (612 x 792 points)
                using var page2 = document.CreatePage(612, 792);
                Console.WriteLine($"   Created page 2: {page2.Width:F1} x {page2.Height:F1} points (Letter)");

                // Create a custom size page
                using var page3 = document.CreatePage(800, 600);
                Console.WriteLine($"   Created page 3: {page3.Width:F1} x {page3.Height:F1} points (Custom)");

                Console.WriteLine($"   Total pages: {document.PageCount}");

                // Save the new document
                document.SaveToFile("output/new-document.pdf");
                Console.WriteLine("   Saved: new-document.pdf\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
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
                image.SaveAsSkiaPng("output/output.png");
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

                image.SaveAsSkiaPng("output/output-hires.png");
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
                        extractedImage.Image.SaveAsSkiaPng($"output/{filename}");
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
                document.SaveToFile("output/output-modified.pdf");
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
        /// Demonstrates annotation creation and management.
        /// </summary>
        static void DemoAnnotations()
        {
            Console.WriteLine("10. Annotation Management");

            PdfiumLibrary.Initialize();

            try
            {
                // Load document and get first page
                using var document = PdfDocument.Open("pdf-sample.pdf");
                using var page = document.GetPage(0);

                Console.WriteLine($"   Original annotations: {page.GetAnnotationCount()}");

                // 1. Create a highlight annotation
                Console.WriteLine("\n   Creating highlight annotation...");
                var highlightBounds = new PdfRectangle(100, 700, 200, 20);
                var highlight = PdfHighlightAnnotation.Create(page, highlightBounds, 0x80FFFF00); // Yellow, 50% opacity
                Console.WriteLine($"      Created highlight at {highlightBounds}");
                Console.WriteLine($"      Color: 0x{highlight.Color:X8} (ARGB)");

                // Set multiple quad points for the highlight
                var quads = new[]
                {
                    new PdfRectangle(100, 700, 200, 10),
                    new PdfRectangle(100, 710, 150, 10)
                };
                highlight.SetQuadPoints(quads);
                Console.WriteLine($"      Set {quads.Length} quad points");

                // 2. Create a text annotation (sticky note)
                Console.WriteLine("\n   Creating text annotation...");
                var textAnnot = PdfTextAnnotation.Create(page, 50, 650, "This is a test comment");
                textAnnot.Author = "PDFiumZ Demo";
                Console.WriteLine($"      Created text annotation at (50, 650)");
                Console.WriteLine($"      Contents: \"{textAnnot.Contents}\"");
                Console.WriteLine($"      Author: \"{textAnnot.Author}\"");

                // 3. Create stamp annotations with different types
                Console.WriteLine("\n   Creating stamp annotations...");

                var draftStamp = PdfStampAnnotation.Create(
                    page,
                    new PdfRectangle(350, 700, 120, 50),
                    PdfStampType.Draft);
                Console.WriteLine($"      Created DRAFT stamp at (350, 700)");

                var approvedStamp = PdfStampAnnotation.Create(
                    page,
                    new PdfRectangle(350, 640, 120, 50),
                    PdfStampType.Approved);
                Console.WriteLine($"      Created APPROVED stamp at (350, 640)");

                var confidentialStamp = PdfStampAnnotation.Create(
                    page,
                    new PdfRectangle(350, 580, 120, 50),
                    PdfStampType.Confidential);
                Console.WriteLine($"      Created CONFIDENTIAL stamp at (350, 580)");

                // Save document with annotations
                Console.WriteLine("\n   Saving document with annotations...");
                document.SaveToFile("output/output-with-annotations.pdf");
                Console.WriteLine("      Saved: output-with-annotations.pdf");

                // Dispose annotations before reading them back
                highlight.Dispose();
                textAnnot.Dispose();
                draftStamp.Dispose();
                approvedStamp.Dispose();
                confidentialStamp.Dispose();

                Console.WriteLine($"\n   Total annotations now: {page.GetAnnotationCount()}");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }

            // Now read the annotations back in a new session
            Console.WriteLine("\n   --- Reading annotations from saved PDF ---");
            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.Open("output-with-annotations.pdf");
                using var page = document.GetPage(0);

                var annotCount = page.GetAnnotationCount();
                Console.WriteLine($"   Found {annotCount} annotation(s)");

                // Enumerate all annotations
                int index = 0;
                foreach (var annotation in page.GetAnnotations())
                {
                    using (annotation)
                    {
                        Console.WriteLine($"\n   Annotation {index + 1}:");
                        Console.WriteLine($"      Type: {annotation.Type}");
                        Console.WriteLine($"      Index: {annotation.Index}");
                        Console.WriteLine($"      Bounds: {annotation.Bounds}");
                        Console.WriteLine($"      Color: 0x{annotation.Color:X8}");

                        // Type-specific information
                        if (annotation is PdfHighlightAnnotation highlightAnnot)
                        {
                            var quadPoints = highlightAnnot.GetQuadPoints();
                            Console.WriteLine($"      Quad points: {quadPoints.Length}");
                        }
                        else if (annotation is PdfTextAnnotation textAnnot)
                        {
                            Console.WriteLine($"      Contents: \"{textAnnot.Contents}\"");
                            Console.WriteLine($"      Author: \"{textAnnot.Author}\"");
                        }
                        else if (annotation is PdfStampAnnotation stampAnnot)
                        {
                            Console.WriteLine($"      Stamp type: {stampAnnot.StampType}");
                        }

                        index++;
                    }
                }

                // Test filtering by type
                var highlights = page.GetAnnotations<PdfHighlightAnnotation>().ToList();
                Console.WriteLine($"\n   Highlights: {highlights.Count}");
                highlights.ForEach(h => h.Dispose());

                var textAnnotations = page.GetAnnotations<PdfTextAnnotation>().ToList();
                Console.WriteLine($"   Text annotations: {textAnnotations.Count}");
                textAnnotations.ForEach(t => t.Dispose());

                var stamps = page.GetAnnotations<PdfStampAnnotation>().ToList();
                Console.WriteLine($"   Stamps: {stamps.Count}");
                stamps.ForEach(s => s.Dispose());

                // Test annotation removal
                Console.WriteLine("\n   Testing annotation removal...");
                Console.WriteLine($"   Annotations before removal: {page.GetAnnotationCount()}");

                if (page.GetAnnotationCount() > 0)
                {
                    // Remove the first annotation
                    page.RemoveAnnotation(0);
                    Console.WriteLine($"   Removed annotation at index 0");
                    Console.WriteLine($"   Annotations after removal: {page.GetAnnotationCount()}");

                    // Save modified document
                    document.SaveToFile("output/output-annotations-removed.pdf");
                    Console.WriteLine("   Saved: output-annotations-removed.pdf");
                }

                Console.WriteLine();
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates content creation (adding text, shapes, images to pages).
        /// </summary>
        static void DemoContentCreation()
        {
            Console.WriteLine("11. Content Creation");

            PdfiumLibrary.Initialize();

            try
            {
                // Load document
                using var document = PdfDocument.Open("pdf-sample.pdf");

                // Insert a blank page at the beginning for our content
                document.InsertBlankPage(0, 595, 842);  // A4 size
                Console.WriteLine("   Created blank A4 page at beginning");

                // Get the blank page
                using var page = document.GetPage(0);

                // 1. Add text with standard font
                Console.WriteLine("\n   Adding text content...");

                // Load Helvetica font
                var helvetica = PdfFont.LoadStandardFont(document, PdfStandardFont.Helvetica);
                Console.WriteLine("      Loaded Helvetica font");

                // Load Times-Roman font
                var times = PdfFont.LoadStandardFont(document, PdfStandardFont.TimesBold);
                Console.WriteLine("      Loaded Times-Roman Bold font");

                // Begin editing the page
                using (var editor = page.BeginEdit())
                {
                    // Add title text
                    var titleText = editor.AddText(
                        "PDFiumZ Content Creation Demo",
                        50,   // x position
                        750,  // y position
                        times,
                        24    // font size
                    );
                    Console.WriteLine("      Added title text at (50, 750)");

                    // Add body text
                    var bodyText = editor.AddText(
                        "This page was created programmatically using PDFiumZ content creation API.",
                        50,
                        700,
                        helvetica,
                        14
                    );
                    Console.WriteLine("      Added body text at (50, 700)");

                    // Add more text
                    editor.AddText(
                        "Features demonstrated:",
                        50,
                        650,
                        helvetica,
                        14
                    );

                    editor.AddText(
                        "- Text rendering with standard PDF fonts",
                        70,
                        620,
                        helvetica,
                        12
                    );

                    editor.AddText(
                        "- Rectangle shapes with colors",
                        70,
                        600,
                        helvetica,
                        12
                    );

                    editor.AddText(
                        "- Page content generation",
                        70,
                        580,
                        helvetica,
                        12
                    );

                    Console.WriteLine("      Added feature list");

                    // 2. Add rectangles with different colors
                    Console.WriteLine("\n   Adding shapes...");

                    // Red rectangle (stroke only)
                    var redRect = editor.AddRectangle(
                        new PdfRectangle(50, 450, 150, 100),
                        0xFFFF0000,  // Red stroke (ARGB)
                        0            // No fill
                    );
                    Console.WriteLine("      Added red rectangle (stroke only)");

                    // Blue rectangle (filled)
                    var blueRect = editor.AddRectangle(
                        new PdfRectangle(220, 450, 150, 100),
                        0,           // No stroke
                        0x800000FF   // Blue fill, 50% opacity (ARGB)
                    );
                    Console.WriteLine("      Added blue rectangle (filled, 50% opacity)");

                    // Green rectangle (stroke and fill)
                    var greenRect = editor.AddRectangle(
                        new PdfRectangle(390, 450, 150, 100),
                        0xFF00FF00,  // Green stroke
                        0x8000FF00   // Green fill, 50% opacity
                    );
                    Console.WriteLine("      Added green rectangle (stroke + fill)");

                    // Add labels for rectangles
                    editor.AddText("Stroke only", 80, 520, helvetica, 10);
                    editor.AddText("Fill only", 260, 520, helvetica, 10);
                    editor.AddText("Stroke + Fill", 415, 520, helvetica, 10);

                    Console.WriteLine("      Added rectangle labels");

                    // Regenerate page content to persist changes
                    Console.WriteLine("\n   Generating page content...");
                    editor.GenerateContent();
                }

                // Dispose fonts
                helvetica.Dispose();
                times.Dispose();

                // Save document with new content
                Console.WriteLine("   Saving document...");
                document.SaveToFile("output/output-with-content.pdf");
                Console.WriteLine("      Saved: output-with-content.pdf");

                Console.WriteLine("\n   Content creation complete!");
                Console.WriteLine("   Open output-with-content.pdf to see the created content.\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates async API operations.
        /// </summary>
        static async Task DemoAsyncOperations()
        {
            Console.WriteLine("12. Async Operations");

            PdfiumLibrary.Initialize();

            try
            {
                // 1. Asynchronous document loading
                Console.WriteLine("\n   Testing async document loading...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                using var document = await PdfDocument.OpenAsync("pdf-sample.pdf");
                stopwatch.Stop();

                Console.WriteLine($"      Loaded {document.PageCount} pages asynchronously in {stopwatch.ElapsedMilliseconds}ms");

                // 2. Asynchronous page rendering
                Console.WriteLine("\n   Testing async page rendering...");
                using var page = document.GetPage(0);

                stopwatch.Restart();
                using var image = await page.RenderToImageAsync();
                stopwatch.Stop();

                Console.WriteLine($"      Rendered {image.Width}x{image.Height} image asynchronously in {stopwatch.ElapsedMilliseconds}ms");

                // 3. Asynchronous rendering with options
                var options = RenderOptions.Default.WithDpi(150);
                stopwatch.Restart();
                using var hiresImage = await page.RenderToImageAsync(options);
                stopwatch.Stop();

                Console.WriteLine($"      Rendered {hiresImage.Width}x{hiresImage.Height} high-DPI image asynchronously in {stopwatch.ElapsedMilliseconds}ms");

                // 4. Asynchronous document saving
                Console.WriteLine("\n   Testing async document saving...");
                stopwatch.Restart();
                await document.SaveToFileAsync("output/output-async.pdf");
                stopwatch.Stop();

                Console.WriteLine($"      Saved document asynchronously in {stopwatch.ElapsedMilliseconds}ms");

                // 5. Cancellation token support
                Console.WriteLine("\n   Testing cancellation support...");
                using var cts = new System.Threading.CancellationTokenSource();
                cts.CancelAfter(100); // Cancel after 100ms

                try
                {
                    // This might or might not be canceled depending on timing
                    using var doc = await PdfDocument.OpenAsync("pdf-sample.pdf", cancellationToken: cts.Token);
                    Console.WriteLine("      Operation completed before cancellation");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("      Operation was successfully canceled");
                }

                Console.WriteLine("\n   All async operations completed successfully!\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates batch operations.
        /// </summary>
        static void DemoBatchOperations()
        {
            Console.WriteLine("13. Batch Operations");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.Open("pdf-sample.pdf");
                Console.WriteLine($"   Original: {document.PageCount} page(s)");

                // Make a copy for testing batch operations
                document.SaveToFile("output/batch-test.pdf");

                using var batchDoc = PdfDocument.Open("output/batch-test.pdf");

                // 1. Get multiple pages at once
                Console.WriteLine("\n   Testing GetPages (batch retrieval)...");
                if (batchDoc.PageCount >= 3)
                {
                    var pages = batchDoc.GetPages(0, 3).ToList();
                    Console.WriteLine($"      Retrieved {pages.Count} pages in batch");

                    foreach (var p in pages)
                    {
                        Console.WriteLine($"         Page {p.Index}: {p.Width:F1} x {p.Height:F1} points");
                        p.Dispose();
                    }
                }

                // 2. Delete multiple pages by indices
                Console.WriteLine("\n   Testing DeletePages (multiple indices)...");

                // Add some blank pages for testing
                for (int i = 0; i < 5; i++)
                {
                    batchDoc.InsertBlankPage(batchDoc.PageCount, 595, 842);
                }
                Console.WriteLine($"      Added 5 blank pages: {batchDoc.PageCount} total");
                Console.WriteLine($"      Pages: [original content, blank, blank, blank, blank, blank]");

                // Delete pages 2, 3, 5 (blank pages, keep original at index 0)
                int originalCount = batchDoc.PageCount;
                batchDoc.DeletePages(2, 3, 5);
                Console.WriteLine($"      Deleted pages at indices 2, 3, 5 (blank pages)");
                Console.WriteLine($"      Result: {batchDoc.PageCount} pages (was {originalCount})");
                Console.WriteLine($"      Remaining: [original content, blank, blank]");

                // 3. Delete a range of consecutive pages
                Console.WriteLine("\n   Testing DeletePages (range)...");
                originalCount = batchDoc.PageCount;

                if (batchDoc.PageCount >= 3)
                {
                    batchDoc.DeletePages(1, 2);  // Delete last 2 blank pages (indices 1-2)
                    Console.WriteLine($"      Deleted pages 1-2 (last 2 blank pages)");
                    Console.WriteLine($"      Result: {batchDoc.PageCount} pages (was {originalCount})");
                    Console.WriteLine($"      Final: [original content page]");
                }

                // Save result
                batchDoc.SaveToFile("output/batch-result.pdf");
                Console.WriteLine("\n   Saved batch operation result to: batch-result.pdf");

                Console.WriteLine("\n   All batch operations completed successfully!\n");
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

            var bitmap = fpdfview.FPDFBitmapCreateEx(
                (int)pageWidth,
                (int)pageHeight,
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
