using PDFiumZ.HighLevel;

namespace PDFiumZ.Fluent.Elements.Visual;

/// <summary>
/// Renders an image from a file or byte array.
/// </summary>
public class ImageElement : IElement
{
    public string? FilePath { get; set; }
    public byte[]? ImageData { get; set; }
    public double? RequestedWidth { get; set; }
    public double? RequestedHeight { get; set; }
    public double ImageWidth { get; set; }
    public double ImageHeight { get; set; }

    public ImageElement(string filePath)
    {
        FilePath = filePath;
    }

    public ImageElement(byte[] imageData, double width, double height)
    {
        ImageData = imageData;
        ImageWidth = width;
        ImageHeight = height;
    }

    public SpacePlan Measure(MeasureContext context)
    {
        double width = RequestedWidth ?? ImageWidth;
        double height = RequestedHeight ?? ImageHeight;

        // Maintain aspect ratio if only one dimension is specified
        if (RequestedWidth.HasValue && !RequestedHeight.HasValue)
        {
            height = ImageHeight * (RequestedWidth.Value / ImageWidth);
        }
        else if (RequestedHeight.HasValue && !RequestedWidth.HasValue)
        {
            width = ImageWidth * (RequestedHeight.Value / ImageHeight);
        }

        var size = new Size(width, height);

        if (size.Width <= context.AvailableSpace.Width && size.Height <= context.AvailableSpace.Height)
            return SpacePlan.FullRender(size);

        return SpacePlan.Wrap();
    }

    public void Render(RenderContext context)
    {
        double width = RequestedWidth ?? ImageWidth;
        double height = RequestedHeight ?? ImageHeight;

        // Maintain aspect ratio
        if (RequestedWidth.HasValue && !RequestedHeight.HasValue)
        {
            height = ImageHeight * (RequestedWidth.Value / ImageWidth);
        }
        else if (RequestedHeight.HasValue && !RequestedWidth.HasValue)
        {
            width = ImageWidth * (RequestedHeight.Value / ImageHeight);
        }

        var rect = new PdfRectangle(context.Position.X, context.Position.Y, width, height);

        // Note: This is a simplified version. In a real implementation,
        // you would need to load the image and add it using PDFium's image API
        if (!string.IsNullOrEmpty(FilePath))
        {
            // For now, just draw a placeholder rectangle
            context.Editor
                .WithStrokeColor(PdfColor.Gray)
                .WithFillColor(PdfColor.WithOpacity(PdfColor.LightGray, 0.3))
                .Rectangle(rect);
        }
    }
}
