using PDFiumZ.Fluent;
using PDFiumZ.Fluent.Document;
using PDFiumZ.HighLevel;
using static PDFiumZ.Fluent.ElementExtensions;

namespace PDFiumZDemo;

/// <summary>
/// Demonstrates QuestPDF-style Fluent API for PDF generation.
/// </summary>
public static class FluentApiDemo
{
    public static void DemoFluentDocumentAPI()
    {
        Console.WriteLine("=== Fluent Document API Demo (QuestPDF-style) ===\n");

        PdfiumLibrary.Initialize();

        try
        {
            using var document = new FluentDocument();

            // Example 1: Simple text document
            Console.WriteLine("1. Creating simple text document...");
            document.Content(page =>
            {
                page.Column(column =>
                {
                    column.Item(item => item.Text("Hello, PDFiumZ Fluent API!"));
                    column.Item(item => item.Text("This is a second line of text."));
                    column.Item(item => item.Text("And here's a third one."));
                });
            });

            // Example 2: Document with padding and background
            Console.WriteLine("2. Creating document with styling...");
            document.Content(page =>
            {
                page.Padding(20, container =>
                {
                    container.Background(PdfColor.LightGray, bg =>
                    {
                        bg.Padding(10, content =>
                        {
                            content.Text("Styled content with padding and background");
                        });
                    });
                });
            });

            // Example 3: Document with border
            Console.WriteLine("3. Creating document with border...");
            document.Content(page =>
            {
                page.Border(PdfColor.Blue, 2, bordered =>
                {
                    bordered.Padding(15, content =>
                    {
                        content.Column(column =>
                        {
                            column.Item(item => item.Text("Title with Border"));
                            column.Item(item => item.Text("Content inside border"));
                        });
                    });
                });
            });

            // Generate and save
            document.Generate();
            document.SaveToFile("output/fluent-api-demo.pdf");
            Console.WriteLine("\nSaved: output/fluent-api-demo.pdf");
        }
        finally
        {
            PdfiumLibrary.Shutdown();
        }

        Console.WriteLine("\n=== Fluent API Demo Complete ===\n");
    }

    public static void DemoElementComposition()
    {
        Console.WriteLine("=== Element Composition Demo ===\n");

        PdfiumLibrary.Initialize();

        try
        {
            using var doc = new FluentDocument();

            Console.WriteLine("Creating composed document with fluent syntax...");

            // Using extension methods for fluent composition
            var title = "PDFiumZ Fluent API"
                .Text()
                .WithFontSize(24)
                .WithColor(PdfColor.DarkBlue)
                .Padding(10);

            var subtitle = "QuestPDF-inspired Document Generation"
                .Text()
                .WithFontSize(14)
                .WithColor(PdfColor.Gray)
                .Padding(5, 10);

            var content1 = "This is paragraph 1 with some content."
                .Text()
                .Padding(5);

            var content2 = "This is paragraph 2 with more content."
                .Text()
                .Background(PdfColor.WithOpacity(PdfColor.Yellow, 0.3))
                .Padding(10);

            var content3 = "This is paragraph 3 inside a border."
                .Text()
                .Padding(10)
                .Border(PdfColor.Red, 2);

            // Compose into a column
            var page = Column(10,
                title,
                subtitle,
                content1,
                content2,
                content3
            ).Padding(20);

            // Note: Currently FluentDocument uses a different composition style
            // This demonstrates the element composition capability
            Console.WriteLine("   Composed 5 elements with fluent methods");
            Console.WriteLine("   - Title with large font");
            Console.WriteLine("   - Subtitle with gray color");
            Console.WriteLine("   - Content paragraphs with padding");
            Console.WriteLine("   - Highlighted paragraph with background");
            Console.WriteLine("   - Bordered paragraph\n");

            Console.WriteLine("Element composition demo complete!");
        }
        finally
        {
            PdfiumLibrary.Shutdown();
        }

        Console.WriteLine("\n=== Element Composition Demo Complete ===\n");
    }

    public static void DemoLayoutElements()
    {
        Console.WriteLine("=== Layout Elements Demo ===\n");

        Console.WriteLine("Demonstrated layout capabilities:");
        Console.WriteLine("- Column: Vertical stacking of elements");
        Console.WriteLine("- Row: Horizontal arrangement");
        Console.WriteLine("- Padding: Spacing around elements");
        Console.WriteLine("- Background: Color fills");
        Console.WriteLine("- Border: Outlined elements");
        Console.WriteLine("- Alignment: Positioning within containers");
        Console.WriteLine("- Width/Height: Size constraints");
        Console.WriteLine("- AspectRatio: Proportional sizing");
        Console.WriteLine("- Layers: Overlapping elements");
        Console.WriteLine("- ShowIf: Conditional rendering");
        Console.WriteLine("- PageBreak: Force new pages\n");

        Console.WriteLine("=== Layout Elements Demo Complete ===\n");
    }
}
