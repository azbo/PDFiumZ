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
        private static readonly string SamplePdfPath = ResolveSamplePdfPath();

        private static string ResolveSamplePdfPath()
        {
            // In the repo the sample PDF is under `src/`, but when running from a build output
            // it may be copied next to the executable. Try a few common locations so the demo
            // runs from either the repo root or the build output folder.
            var candidates = new[]
            {
                "pdf-sample.pdf",
                System.IO.Path.Combine(AppContext.BaseDirectory, "pdf-sample.pdf"),
                System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "pdf-sample.pdf"))
            };

            foreach (var candidate in candidates)
            {
                if (System.IO.File.Exists(candidate))
                    return candidate;
            }

            return "pdf-sample.pdf";
        }

        static async Task Main(string[] args)
        {
            // Create output directory for generated files
            System.IO.Directory.CreateDirectory("output");

            // Demonstrate the new high-level API
            Console.WriteLine("=== PDFiumZ High-Level API Demo ===\n");

            DemoCreateNewDocument();  // NEW: Test CreateNew + CreatePage
            DemoWatermark();  // NEW: Test AddTextWatermark
            DemoMergeAndSplit();  // NEW: Test Merge + Split
            DemoRotatePages();  // NEW: Test page rotation
            DemoFluentAPI();  // NEW: Test enhanced fluent API with colors and shapes
            DemoHtmlToPdf();  // NEW: Test HTML to PDF conversion
            DemoHtmlListsToPdf();  // NEW: Test HTML lists (ul, ol, li)
            DemoHtmlWithImages();  // NEW: Test HTML with images
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

                var helvetica = PdfFont.LoadStandardFont(document, PdfStandardFont.Helvetica);

                using (var page1 = document.CreatePage(595, 842))
                {
                    Console.WriteLine($"   Created page 1: {page1.Width:F1} x {page1.Height:F1} points (A4)");
                    using var editor = page1.BeginEdit();
                    editor.AddText("PDFiumZ Demo - New Document", 50, page1.Height - 80, helvetica, 24);
                    editor.AddText("Page 1 (A4)", 50, page1.Height - 120, helvetica, 14);
                    editor.AddText($"Size: {page1.Width:F0} x {page1.Height:F0} pt", 50, page1.Height - 145, helvetica, 12);
                    // PDFium requires regenerating page content after edits, otherwise the page may save as blank.
                    editor.GenerateContent();
                }

                using (var page2 = document.CreatePage(612, 792))
                {
                    Console.WriteLine($"   Created page 2: {page2.Width:F1} x {page2.Height:F1} points (Letter)");
                    using var editor = page2.BeginEdit();
                    editor.AddText("Page 2 (Letter)", 50, page2.Height - 80, helvetica, 18);
                    editor.AddText($"Size: {page2.Width:F0} x {page2.Height:F0} pt", 50, page2.Height - 110, helvetica, 12);
                    editor.GenerateContent();
                }

                using (var page3 = document.CreatePage(800, 600))
                {
                    Console.WriteLine($"   Created page 3: {page3.Width:F1} x {page3.Height:F1} points (Custom)");
                    using var editor = page3.BeginEdit();
                    editor.AddText("Page 3 (Custom)", 50, page3.Height - 80, helvetica, 18);
                    editor.AddText($"Size: {page3.Width:F0} x {page3.Height:F0} pt", 50, page3.Height - 110, helvetica, 12);
                    editor.GenerateContent();
                }

                helvetica.Dispose();

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
        /// Demonstrates PDF merge and split operations.
        /// </summary>
        static void DemoMergeAndSplit()
        {
            Console.WriteLine("0.6. Merge and Split PDFs");

            PdfiumLibrary.Initialize();

            try
            {
                // First, create 3 simple PDF documents for testing
                Console.WriteLine("   Creating test documents...");

                using (var doc1 = PdfDocument.CreateNew())
                {
                    var font = PdfFont.LoadStandardFont(doc1, PdfStandardFont.Helvetica);
                    using (var page = doc1.CreatePage(595, 842))
                    {
                        using var editor = page.BeginEdit();
                        editor.AddText("Merge/Split Test - Doc1 - Page 1", 50, page.Height - 80, font, 18);
                        // Add some visible content so merged/split PDFs are not visually blank.
                        editor.GenerateContent();
                    }
                    font.Dispose();
                    doc1.SaveToFile("output/test-doc1.pdf");
                }

                using (var doc2 = PdfDocument.CreateNew())
                {
                    var font = PdfFont.LoadStandardFont(doc2, PdfStandardFont.Helvetica);
                    using (var page = doc2.CreatePage(595, 842))
                    {
                        using var editor = page.BeginEdit();
                        editor.AddText("Merge/Split Test - Doc2 - Page 1", 50, page.Height - 80, font, 18);
                        editor.GenerateContent();
                    }
                    using (var page = doc2.CreatePage(595, 842))
                    {
                        using var editor = page.BeginEdit();
                        editor.AddText("Merge/Split Test - Doc2 - Page 2", 50, page.Height - 80, font, 18);
                        editor.GenerateContent();
                    }
                    font.Dispose();
                    doc2.SaveToFile("output/test-doc2.pdf");
                }

                using (var doc3 = PdfDocument.CreateNew())
                {
                    var font = PdfFont.LoadStandardFont(doc3, PdfStandardFont.Helvetica);
                    using (var page = doc3.CreatePage(595, 842))
                    {
                        using var editor = page.BeginEdit();
                        editor.AddText("Merge/Split Test - Doc3 - Page 1", 50, page.Height - 80, font, 18);
                        editor.GenerateContent();
                    }
                    using (var page = doc3.CreatePage(595, 842))
                    {
                        using var editor = page.BeginEdit();
                        editor.AddText("Merge/Split Test - Doc3 - Page 2", 50, page.Height - 80, font, 18);
                        editor.GenerateContent();
                    }
                    using (var page = doc3.CreatePage(595, 842))
                    {
                        using var editor = page.BeginEdit();
                        editor.AddText("Merge/Split Test - Doc3 - Page 3", 50, page.Height - 80, font, 18);
                        editor.GenerateContent();
                    }
                    font.Dispose();
                    doc3.SaveToFile("output/test-doc3.pdf");
                }

                Console.WriteLine("   Created: test-doc1.pdf (1 page), test-doc2.pdf (2 pages), test-doc3.pdf (3 pages)");

                // Test Merge: combine all 3 documents
                Console.WriteLine("\n   Testing Merge...");
                using (var merged = PdfDocument.Merge(
                    "output/test-doc1.pdf",
                    "output/test-doc2.pdf",
                    "output/test-doc3.pdf"))
                {
                    Console.WriteLine($"   Merged document has {merged.PageCount} pages (expected: 6)");
                    merged.SaveToFile("output/merged.pdf");
                    Console.WriteLine("   Saved: merged.pdf");
                }

                // Test Split: extract first 3 pages from merged document
                Console.WriteLine("\n   Testing Split...");
                using (var mergedDoc = PdfDocument.Open("output/merged.pdf"))
                {
                    Console.WriteLine($"   Source document has {mergedDoc.PageCount} pages");

                    using (var split1 = mergedDoc.Split(0, 3))
                    {
                        Console.WriteLine($"   Split 1: pages 0-2 → {split1.PageCount} pages");
                        split1.SaveToFile("output/split-first3.pdf");
                        Console.WriteLine("   Saved: split-first3.pdf");
                    }

                    using (var split2 = mergedDoc.Split(3, 3))
                    {
                        Console.WriteLine($"   Split 2: pages 3-5 → {split2.PageCount} pages");
                        split2.SaveToFile("output/split-last3.pdf");
                        Console.WriteLine("   Saved: split-last3.pdf");
                    }
                }

                Console.WriteLine("\n   Merge and Split operations completed successfully!\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates page rotation functionality.
        /// </summary>
        static void DemoRotatePages()
        {
            Console.WriteLine("0.7. Rotate PDF Pages");

            PdfiumLibrary.Initialize();

            try
            {
                // Create a test document with 4 pages
                Console.WriteLine("   Creating test document with 4 pages...");
                using var document = PdfDocument.CreateNew();
                var font = PdfFont.LoadStandardFont(document, PdfStandardFont.Helvetica);

                for (int i = 0; i < 4; i++)
                {
                    using var page = document.CreatePage(595, 842);
                    using var editor = page.BeginEdit();
                    editor.AddText($"Rotate Test - Page {i + 1}", 50, page.Height - 80, font, 18);
                    editor.AddText("Use this label to verify rotation direction.", 50, page.Height - 110, font, 12);
                    // Persist the added text so rotation output is easy to validate visually.
                    editor.GenerateContent();
                }
                font.Dispose();
                Console.WriteLine($"   Created document with {document.PageCount} pages");

                // Test 1: Rotate individual pages
                Console.WriteLine("\n   Test 1: Rotating individual pages...");
                document.RotatePages(PdfRotation.Rotate90, 0);      // First page: 90°
                Console.WriteLine("      Page 0: Rotated 90° clockwise");

                document.RotatePages(PdfRotation.Rotate180, 1);     // Second page: 180°
                Console.WriteLine("      Page 1: Rotated 180°");

                document.RotatePages(PdfRotation.Rotate270, 2);     // Third page: 270°
                Console.WriteLine("      Page 2: Rotated 270° clockwise");

                // Save individual rotation result
                document.SaveToFile("output/rotated-individual.pdf");
                Console.WriteLine("   Saved: rotated-individual.pdf");

                // Test 2: Rotate all pages
                Console.WriteLine("\n   Test 2: Rotating all pages...");
                using var document2 = PdfDocument.CreateNew();
                var font2 = PdfFont.LoadStandardFont(document2, PdfStandardFont.Helvetica);
                for (int i = 0; i < 3; i++)
                {
                    using var page = document2.CreatePage(595, 842);
                    using var editor = page.BeginEdit();
                    editor.AddText($"Rotate-All Test - Page {i + 1}", 50, page.Height - 80, font2, 18);
                    editor.GenerateContent();
                }
                font2.Dispose();

                document2.RotateAllPages(PdfRotation.Rotate90);
                Console.WriteLine($"      Rotated all {document2.PageCount} pages by 90°");

                // Save batch rotation result
                document2.SaveToFile("output/rotated-all.pdf");
                Console.WriteLine("   Saved: rotated-all.pdf");

                // Test 3: Read rotation
                Console.WriteLine("\n   Test 3: Reading page rotation...");
                using var page0 = document.GetPage(0);
                using var page1 = document.GetPage(1);
                using var page2 = document.GetPage(2);
                using var page3 = document.GetPage(3);

                Console.WriteLine($"      Page 0 rotation: {page0.Rotation}");
                Console.WriteLine($"      Page 1 rotation: {page1.Rotation}");
                Console.WriteLine($"      Page 2 rotation: {page2.Rotation}");
                Console.WriteLine($"      Page 3 rotation: {page3.Rotation} (no rotation)");

                Console.WriteLine("\n   Page rotation operations completed successfully!\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates adding watermarks to PDF documents.
        /// </summary>
        static void DemoWatermark()
        {
            Console.WriteLine("0.5. Add Watermarks to PDF");

            PdfiumLibrary.Initialize();

            try
            {
                // Open existing document
                using var document = PdfDocument.Open(SamplePdfPath);
                Console.WriteLine($"   Loaded: {document.PageCount} page(s)");

                // Add watermark with custom options (center, 45° rotation, 30% opacity)
                document.AddTextWatermark("CONFIDENTIAL", WatermarkPosition.Center,
                    new WatermarkOptions { Opacity = 0.3, Rotation = 45 });
                Console.WriteLine("   Added CONFIDENTIAL watermark (center, 45° rotation, 30% opacity)");

                // Save with watermark
                document.SaveToFile("output/watermarked.pdf");
                Console.WriteLine("   Saved: watermarked.pdf\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates the enhanced fluent API with colors, shapes, and default settings.
        /// </summary>
        static void DemoFluentAPI()
        {
            Console.WriteLine("0.8. Enhanced Fluent API with Colors and Shapes");

            PdfiumLibrary.Initialize();

            try
            {
                // Create a new document
                using var document = PdfDocument.CreateNew();
                Console.WriteLine("   Created new document");

                // Create A4 page
                using var page = document.CreatePage(595, 842);
                Console.WriteLine("   Created A4 page");

                // Load fonts
                var helvetica = PdfFont.LoadStandardFont(document, PdfStandardFont.Helvetica);
                var helveticaBold = PdfFont.LoadStandardFont(document, PdfStandardFont.HelveticaBold);

                // Use fluent API with default settings
                using (var editor = page.BeginEdit())
                {
                    Console.WriteLine("\n   Testing fluent API features:");

                    // 1. Set default font and size, then use simplified Text method
                    Console.WriteLine("      1. Using WithFont() and simplified Text()");
                    editor
                        .WithFont(helveticaBold)
                        .WithFontSize(PdfFontSize.Heading1)
                        .WithTextColor(PdfColor.DarkBlue)
                        .Text("Enhanced Fluent API Demo", 50, 780);

                    // 2. Change font and add description
                    Console.WriteLine("      2. Changing font size and adding description");
                    editor
                        .WithFont(helvetica)
                        .WithFontSize(PdfFontSize.Normal)
                        .WithTextColor(PdfColor.Black)
                        .Text("Demonstrating predefined colors, font sizes, and shapes", 50, 750);

                    // 3. Draw colored shapes section
                    Console.WriteLine("      3. Drawing shapes with predefined colors");
                    editor
                        .WithFont(helvetica)
                        .WithFontSize(PdfFontSize.Medium)
                        .WithTextColor(PdfColor.DarkRed)
                        .Text("Shapes with Predefined Colors:", 50, 700);

                    // Rectangles with different colors
                    editor
                        .WithStrokeColor(PdfColor.Red)
                        .WithFillColor(PdfColor.WithOpacity(PdfColor.Red, 0.3))
                        .Rectangle(new PdfRectangle(50, 630, 80, 50))
                        .Text("Red", 70, 645);

                    editor
                        .WithStrokeColor(PdfColor.Green)
                        .WithFillColor(PdfColor.WithOpacity(PdfColor.Green, 0.3))
                        .Rectangle(new PdfRectangle(150, 630, 80, 50))
                        .Text("Green", 165, 645);

                    editor
                        .WithStrokeColor(PdfColor.Blue)
                        .WithFillColor(PdfColor.WithOpacity(PdfColor.Blue, 0.3))
                        .Rectangle(new PdfRectangle(250, 630, 80, 50))
                        .Text("Blue", 270, 645);

                    editor
                        .WithStrokeColor(PdfColor.Orange)
                        .WithFillColor(PdfColor.WithOpacity(PdfColor.Orange, 0.3))
                        .Rectangle(new PdfRectangle(350, 630, 80, 50))
                        .Text("Orange", 363, 645);

                    // 4. Draw circles section
                    Console.WriteLine("      4. Drawing circles");
                    editor
                        .WithFontSize(PdfFontSize.Medium)
                        .WithTextColor(PdfColor.DarkGreen)
                        .Text("Circles and Ellipses:", 50, 580);

                    // Filled circles
                    editor
                        .WithStrokeColor(PdfColor.Purple)
                        .WithFillColor(PdfColor.WithOpacity(PdfColor.Purple, 0.5))
                        .Circle(90, 520, 30)
                        .WithTextColor(PdfColor.Black)
                        .WithFontSize(PdfFontSize.Small)
                        .Text("Circle", 70, 480);

                    editor
                        .WithStrokeColor(PdfColor.Teal)
                        .WithFillColor(PdfColor.WithOpacity(PdfColor.Teal, 0.5))
                        .Ellipse(new PdfRectangle(150, 490, 80, 60))
                        .Text("Ellipse", 170, 480);

                    editor
                        .WithStrokeColor(PdfColor.Maroon)
                        .WithFillColor(PdfColor.Transparent)
                        .Circle(290, 520, 30)
                        .Text("Stroke Only", 257, 480);

                    // 5. Draw lines section
                    Console.WriteLine("      5. Drawing lines");
                    editor
                        .WithFontSize(PdfFontSize.Medium)
                        .WithTextColor(PdfColor.DarkRed)
                        .Text("Lines with Different Widths:", 50, 430);

                    // Lines with different widths
                    editor
                        .WithLineWidth(1)
                        .Line(50, 410, 200, 410, PdfColor.Black)
                        .WithFontSize(PdfFontSize.Small)
                        .Text("1pt", 210, 405);

                    editor
                        .WithLineWidth(2)
                        .Line(50, 390, 200, 390, PdfColor.DarkBlue)
                        .Text("2pt", 210, 385);

                    editor
                        .WithLineWidth(4)
                        .Line(50, 370, 200, 370, PdfColor.DarkGreen)
                        .Text("4pt", 210, 365);

                    editor
                        .WithLineWidth(6)
                        .Line(50, 345, 200, 345, PdfColor.DarkRed)
                        .Text("6pt", 210, 340);

                    // 6. Hex color demonstration
                    Console.WriteLine("      6. Using hex colors");
                    editor
                        .WithFontSize(PdfFontSize.Medium)
                        .WithTextColor(PdfColor.Navy)
                        .Text("Hex Colors:", 50, 300);

                    var customColor1 = PdfColor.FromHex("#FF6B6B");  // Coral red
                    var customColor2 = PdfColor.FromHex("#4ECDC4");  // Turquoise
                    var customColor3 = PdfColor.FromHex("#95E1D3");  // Mint

                    editor
                        .Rectangle(new PdfRectangle(50, 250, 60, 30),
                            customColor1, PdfColor.WithOpacity(customColor1, 0.5))
                        .Text("#FF6B6B", 55, 260);

                    editor
                        .Rectangle(new PdfRectangle(130, 250, 60, 30),
                            customColor2, PdfColor.WithOpacity(customColor2, 0.5))
                        .Text("#4ECDC4", 135, 260);

                    editor
                        .Rectangle(new PdfRectangle(210, 250, 60, 30),
                            customColor3, PdfColor.WithOpacity(customColor3, 0.5))
                        .Text("#95E1D3", 215, 260);

                    // 7. Font sizes demonstration
                    Console.WriteLine("      7. Demonstrating font sizes");
                    editor
                        .WithTextColor(PdfColor.Black)
                        .WithFontSize(PdfFontSize.Heading2)
                        .Text("Font Sizes", 50, 200);

                    var yPos = 175.0;
                    var sizes = new[]
                    {
                        (PdfFontSize.Small, "Small (8pt)"),
                        (PdfFontSize.Normal, "Normal (10pt)"),
                        (PdfFontSize.Default, "Default (12pt)"),
                        (PdfFontSize.Medium, "Medium (14pt)"),
                        (PdfFontSize.Large, "Large (16pt)")
                    };

                    foreach (var (size, label) in sizes)
                    {
                        editor.WithFontSize(size).Text(label, 50, yPos);
                        yPos -= 20;
                    }

                    // 8. Complex drawing using chaining
                    Console.WriteLine("      8. Complex chained drawing");
                    editor
                        .WithTextColor(PdfColor.DarkBlue)
                        .WithFontSize(PdfFontSize.Small)
                        .Text("Complex chained operations:", 320, 180)
                        .WithStrokeColor(PdfColor.Gold)
                        .WithFillColor(PdfColor.WithOpacity(PdfColor.Gold, 0.3))
                        .WithLineWidth(2)
                        .Rectangle(new PdfRectangle(320, 130, 200, 40))
                        .WithTextColor(PdfColor.Black)
                        .WithFontSize(PdfFontSize.Normal)
                        .Text("Fluent API is Powerful!", 335, 145);

                    // Commit all changes
                    Console.WriteLine("      9. Committing changes");
                    editor.Commit();
                }

                // Dispose fonts
                helvetica.Dispose();
                helveticaBold.Dispose();

                // Save document
                document.SaveToFile("output/fluent-api-demo.pdf");
                Console.WriteLine("\n   Saved: fluent-api-demo.pdf");
                Console.WriteLine("   Enhanced fluent API demo complete!\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates HTML to PDF conversion with various styles.
        /// </summary>
        static void DemoHtmlToPdf()
        {
            Console.WriteLine("0.9. HTML to PDF Conversion");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.CreateNew();
                Console.WriteLine("   Created new document");

                // Example 1: Simple HTML with headings
                string html1 = @"
                    <h1>HTML to PDF Conversion Demo</h1>
                    <p>This demonstrates converting HTML content to PDF with basic styling.</p>
                    <h2>Features</h2>
                    <p>Supports headings, paragraphs, and inline styles.</p>
                ";

                Console.WriteLine("\n   Example 1: Basic HTML");
                document.CreatePageFromHtml(html1);
                Console.WriteLine("      Created page from basic HTML");

                // Example 2: HTML with inline styles
                string html2 = @"
                    <h1 style='color: #0066CC; text-align: center;'>Styled Content</h1>
                    <p style='font-size: 14pt; text-align: center;'>This paragraph is centered with custom font size.</p>

                    <h2 style='color: #FF6600;'>Text Formatting</h2>
                    <p>This is <b>bold text</b>, this is <i>italic text</i>, and this is <u>underlined text</u>.</p>
                    <p>You can also <b><i>combine</i></b> different styles.</p>

                    <h3 style='color: #009933;'>Colors</h3>
                    <p style='color: red;'>This text is red.</p>
                    <p style='color: blue;'>This text is blue.</p>
                    <p style='color: #FF6B6B;'>This text uses a hex color.</p>
                ";

                Console.WriteLine("\n   Example 2: Styled HTML");
                document.CreatePageFromHtml(html2);
                Console.WriteLine("      Created page with styled HTML");

                // Example 3: HTML with different alignments
                string html3 = @"
                    <h1 style='text-align: center;'>Text Alignment Demo</h1>

                    <p style='text-align: left;'>This paragraph is aligned to the left (default).</p>
                    <p style='text-align: center;'>This paragraph is centered.</p>
                    <p style='text-align: right;'>This paragraph is aligned to the right.</p>

                    <h2 style='text-align: center; color: #6A0DAD;'>Multiple Font Sizes</h2>
                    <p style='font-size: 10pt;'>Small text (10pt)</p>
                    <p style='font-size: 12pt;'>Normal text (12pt)</p>
                    <p style='font-size: 16pt;'>Large text (16pt)</p>
                    <p style='font-size: 20pt;'>Extra large text (20pt)</p>
                ";

                Console.WriteLine("\n   Example 3: Alignment and sizes");
                document.CreatePageFromHtml(html3);
                Console.WriteLine("      Created page with alignment examples");

                // Example 4: Complex content
                string html4 = @"
                    <h1 style='color: #2C3E50; text-align: center;'>PDFiumZ HTML Converter</h1>
                    <p style='text-align: center; color: #7F8C8D; font-size: 11pt;'>
                        A simple but powerful HTML to PDF converter
                    </p>
                    <br/>

                    <h2 style='color: #E74C3C;'>Supported Tags</h2>
                    <p>The converter supports the following HTML tags:</p>
                    <p><b>Headings:</b> h1, h2, h3, h4, h5, h6</p>
                    <p><b>Text:</b> p, div, span, b, strong, i, em, u</p>
                    <p><b>Layout:</b> br (line break)</p>
                    <br/>

                    <h2 style='color: #3498DB;'>Supported CSS Properties</h2>
                    <p><b>font-size:</b> 10pt, 12px, 1.5em</p>
                    <p><b>color:</b> red, #FF0000, #F00</p>
                    <p><b>text-align:</b> left, center, right</p>
                    <p><b>font-weight:</b> bold, normal</p>
                    <p><b>font-style:</b> italic, normal</p>
                    <p><b>text-decoration:</b> underline</p>
                    <br/>

                    <h3 style='color: #27AE60; text-align: center;'>Thank you for using PDFiumZ!</h3>
                ";

                Console.WriteLine("\n   Example 4: Complex content");
                document.CreatePageFromHtml(html4);
                Console.WriteLine("      Created page with complex content");

                // Save document
                document.SaveToFile("output/html-to-pdf-demo.pdf");
                Console.WriteLine("\n   Saved: html-to-pdf-demo.pdf");
                Console.WriteLine("   HTML to PDF conversion demo complete!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n   Error: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}\n");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }

        /// <summary>
        /// Demonstrates HTML lists (ul, ol, li) to PDF conversion.
        /// </summary>
        static void DemoHtmlListsToPdf()
        {
            Console.WriteLine("0.10. HTML Lists to PDF Conversion");

            PdfiumLibrary.Initialize();

            try
            {
                using var document = PdfDocument.CreateNew();
                Console.WriteLine("   Created new document");

                // Example 1: Simple unordered list
                string html1 = @"
                    <h1 style='color: #2C3E50;'>Unordered Lists</h1>
                    <p>A simple bullet list:</p>
                    <ul>
                        <li>First item</li>
                        <li>Second item</li>
                        <li>Third item</li>
                    </ul>
                ";

                Console.WriteLine("\n   Example 1: Simple unordered list");
                document.CreatePageFromHtml(html1);
                Console.WriteLine("      Created page with unordered list");

                // Example 2: Simple ordered list
                string html2 = @"
                    <h1 style='color: #E74C3C;'>Ordered Lists</h1>
                    <p>A numbered list:</p>
                    <ol>
                        <li>First step</li>
                        <li>Second step</li>
                        <li>Third step</li>
                        <li>Fourth step</li>
                    </ol>
                ";

                Console.WriteLine("\n   Example 2: Simple ordered list");
                document.CreatePageFromHtml(html2);
                Console.WriteLine("      Created page with ordered list");

                // Example 3: Nested lists
                string html3 = @"
                    <h1 style='color: #3498DB;'>Nested Lists</h1>
                    <p>Lists within lists:</p>
                    <ul>
                        <li>Main item 1</li>
                        <li>Main item 2
                            <ul>
                                <li>Sub item 2.1</li>
                                <li>Sub item 2.2</li>
                            </ul>
                        </li>
                        <li>Main item 3</li>
                    </ul>
                ";

                Console.WriteLine("\n   Example 3: Nested lists");
                document.CreatePageFromHtml(html3);
                Console.WriteLine("      Created page with nested lists");

                // Example 4: Mixed lists with formatting
                string html4 = @"
                    <h1 style='color: #27AE60; text-align: center;'>Mixed Lists Demo</h1>

                    <h2 style='color: #E67E22;'>Todo List</h2>
                    <ol>
                        <li><b>Complete</b> the HTML converter</li>
                        <li>Add <i>list support</i></li>
                        <li>Test with <u>various browsers</u></li>
                    </ol>

                    <h2 style='color: #8E44AD;'>Features</h2>
                    <ul>
                        <li><b>Bold text</b> in lists</li>
                        <li><i>Italic text</i> in lists</li>
                        <li>Mixed <b><i>bold and italic</i></b></li>
                        <li>Colors: <span style='color: red;'>red</span>,
                            <span style='color: blue;'>blue</span>,
                            <span style='color: green;'>green</span></li>
                    </ul>
                ";

                Console.WriteLine("\n   Example 4: Mixed lists with formatting");
                document.CreatePageFromHtml(html4);
                Console.WriteLine("      Created page with mixed formatted lists");

                // Example 5: Complex nested lists
                string html5 = @"
                    <h1 style='text-align: center; color: #34495E;'>Complex Nested Lists</h1>

                    <h2>Project Structure</h2>
                    <ol>
                        <li>Backend
                            <ul>
                                <li>API endpoints</li>
                                <li>Database
                                    <ul>
                                        <li>Users table</li>
                                        <li>Products table</li>
                                    </ul>
                                </li>
                                <li>Authentication</li>
                            </ul>
                        </li>
                        <li>Frontend
                            <ul>
                                <li>Components</li>
                                <li>Pages</li>
                                <li>Styles</li>
                            </ul>
                        </li>
                        <li>Testing
                            <ol>
                                <li>Unit tests</li>
                                <li>Integration tests</li>
                                <li>E2E tests</li>
                            </ol>
                        </li>
                    </ol>
                ";

                Console.WriteLine("\n   Example 5: Complex nested lists");
                document.CreatePageFromHtml(html5);
                Console.WriteLine("      Created page with complex nested lists");

                // Save document
                document.SaveToFile("output/html-lists-demo.pdf");
                Console.WriteLine("\n   Saved: html-lists-demo.pdf");
                Console.WriteLine("   HTML lists conversion demo complete!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n   Error: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}\n");
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
                using var document = PdfDocument.Open(SamplePdfPath);
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
                using var document = PdfDocument.Open(SamplePdfPath);
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
                using var document = PdfDocument.Open(SamplePdfPath);
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
                using var document = PdfDocument.Open(SamplePdfPath);
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
                using var document = PdfDocument.Open(SamplePdfPath);
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
                using var document = PdfDocument.Open(SamplePdfPath);
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
                using var document = PdfDocument.Open(SamplePdfPath);

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
                using var document = PdfDocument.Open(SamplePdfPath);
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
                using var document = PdfDocument.Open(SamplePdfPath);
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
                using var document = PdfDocument.Open(SamplePdfPath);
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
                using var document = PdfDocument.Open("output/output-with-annotations.pdf");
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
                using var document = PdfDocument.Open(SamplePdfPath);

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

                using var document = await PdfDocument.OpenAsync(SamplePdfPath);
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
                    using var doc = await PdfDocument.OpenAsync(SamplePdfPath, cancellationToken: cts.Token);
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
                using var document = PdfDocument.Open(SamplePdfPath);
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
        /// Creates a simple test image using SkiaSharp.
        /// </summary>
        static void CreateTestImage(string outputPath, int width, int height, string text, SkiaSharp.SKColor bgColor, SkiaSharp.SKColor textColor)
        {
            using var surface = SkiaSharp.SKSurface.Create(new SkiaSharp.SKImageInfo(width, height));
            var canvas = surface.Canvas;

            // Fill background
            canvas.Clear(bgColor);

            // Create font and paint
            float fontSize = Math.Min(width, height) / 4;
            using var font = new SkiaSharp.SKFont
            {
                Size = fontSize
            };
            using var paint = new SkiaSharp.SKPaint
            {
                Color = textColor,
                IsAntialias = true
            };

            // Measure text for centering
            float textWidth = font.MeasureText(text);
            float x = (width - textWidth) / 2;
            float y = height / 2 + fontSize / 3;

            canvas.DrawText(text, x, y, font, paint);

            // Save image
            using var image = surface.Snapshot();
            using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            using var stream = System.IO.File.OpenWrite(outputPath);
            data.SaveTo(stream);
        }

        /// <summary>
        /// Image loader using SkiaSharp to load and decode images.
        /// </summary>
        static (byte[] bgraData, int width, int height)? SkiaSharpImageLoader(string src)
        {
            try
            {
                // For this demo, we only support file paths
                if (!System.IO.File.Exists(src))
                {
                    Console.WriteLine($"      Warning: Image file not found: {src}");
                    return null;
                }

                // Load image using SkiaSharp
                using var originalBitmap = SkiaSharp.SKBitmap.Decode(src);
                if (originalBitmap == null)
                {
                    Console.WriteLine($"      Warning: Failed to decode image: {src}");
                    return null;
                }

                int width = originalBitmap.Width;
                int height = originalBitmap.Height;

                // Create a new bitmap in BGRA_8888 format (which PDFium expects)
                using var bitmap = new SkiaSharp.SKBitmap(width, height, SkiaSharp.SKColorType.Bgra8888, SkiaSharp.SKAlphaType.Premul);

                // Draw the original bitmap onto the new one to convert format
                using (var canvas = new SkiaSharp.SKCanvas(bitmap))
                {
                    canvas.DrawBitmap(originalBitmap, 0, 0);
                }

                // Get pixel data directly
                byte[] bgraData = new byte[width * height * 4];
                System.Runtime.InteropServices.Marshal.Copy(bitmap.GetPixels(), bgraData, 0, bgraData.Length);

                return (bgraData, width, height);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      Warning: Error loading image '{src}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Demonstrates HTML to PDF conversion with images.
        /// </summary>
        static void DemoHtmlWithImages()
        {
            Console.WriteLine("0.11. HTML with Images to PDF Conversion");

            PdfiumLibrary.Initialize();

            try
            {
                // Create test images
                Console.WriteLine("   Creating test images...");
                CreateTestImage("output/test-image-red.png", 200, 100, "RED",
                    SkiaSharp.SKColors.Red, SkiaSharp.SKColors.White);
                CreateTestImage("output/test-image-blue.png", 200, 100, "BLUE",
                    SkiaSharp.SKColors.Blue, SkiaSharp.SKColors.White);
                CreateTestImage("output/test-image-green.png", 200, 100, "GREEN",
                    SkiaSharp.SKColors.Green, SkiaSharp.SKColors.White);
                Console.WriteLine("      Created 3 test images");

                using var document = PdfDocument.CreateNew();
                using var converter = new PDFiumZ.HighLevel.HtmlToPdf.HtmlToPdfConverter(document);

                // Set image loader using SkiaSharp
                converter.ImageLoaderFunc = SkiaSharpImageLoader;
                Console.WriteLine("   Set SkiaSharp image loader\n");

                // Example 1: Simple image
                string html1 = @"
                    <h1 style='color: #2C3E50; text-align: center;'>HTML Images Demo</h1>
                    <p style='text-align: center;'>Images loaded from files:</p>
                    <div style='text-align: center;'>
                        <img src='output/test-image-red.png' />
                    </div>
                ";

                Console.WriteLine("   Example 1: Simple image");
                converter.ConvertToPdf(html1);
                Console.WriteLine("      Created page with single image");

                // Example 2: Multiple images
                string html2 = @"
                    <h1 style='color: #E74C3C; text-align: center;'>Multiple Images</h1>
                    <p style='text-align: center;'>Three images in sequence:</p>
                    <div style='text-align: center;'>
                        <img src='output/test-image-red.png' />
                        <img src='output/test-image-blue.png' />
                        <img src='output/test-image-green.png' />
                    </div>
                ";

                Console.WriteLine("\n   Example 2: Multiple images");
                converter.ConvertToPdf(html2);
                Console.WriteLine("      Created page with multiple images");

                // Example 3: Images with custom sizes
                string html3 = @"
                    <h1 style='color: #3498DB; text-align: center;'>Custom Sized Images</h1>
                    <p style='text-align: center;'>Images with width and height attributes:</p>
                    <div style='text-align: center;'>
                        <img src='output/test-image-red.png' width='300' />
                        <p>Large image (300pt width)</p>
                        <img src='output/test-image-blue.png' width='150' />
                        <p>Medium image (150pt width)</p>
                        <img src='output/test-image-green.png' width='100' />
                        <p>Small image (100pt width)</p>
                    </div>
                ";

                Console.WriteLine("\n   Example 3: Custom sized images");
                converter.ConvertToPdf(html3);
                Console.WriteLine("      Created page with custom sized images");

                // Example 4: Mixed content
                string html4 = @"
                    <h1 style='color: #27AE60; text-align: center;'>Mixed Content</h1>
                    <p>Combining text, lists, and images:</p>
                    <ul>
                        <li><b>Text formatting:</b> bold, italic, colors</li>
                        <li><b>Lists:</b> ordered and unordered</li>
                        <li><b>Images:</b> loaded from files</li>
                    </ul>
                    <div style='text-align: center;'>
                        <img src='output/test-image-red.png' width='200' />
                        <p style='text-align: center;'>A colorful image</p>
                    </div>
                    <p>All features work together seamlessly!</p>
                ";

                Console.WriteLine("\n   Example 4: Mixed content (text + lists + images)");
                converter.ConvertToPdf(html4);
                Console.WriteLine("      Created page with mixed content");

                // Save document
                document.SaveToFile("output/html-with-images.pdf");
                Console.WriteLine("\n   Saved: html-with-images.pdf");
                Console.WriteLine("   HTML with images conversion demo complete!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n   Error: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}\n");
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

            var document = fpdfview.FPDF_LoadDocument(SamplePdfPath, null);
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
