using PDFiumZ;
using PDFiumZ.HighLevel;
using SkiaSharp;
using Svg.Skia;
using System;
using System.IO;

namespace PDFiumZ.SkiaSharp
{
    /// <summary>
    /// Extension methods for integrating SVG with PDFiumZ using Svg.Skia and SkiaSharp.
    /// </summary>
    public static class PdfImageSvgExtensions
    {
        /// <summary>
        /// Loads an SVG file and renders it to an SKBitmap.
        /// </summary>
        /// <param name="svgPath">Path to the SVG file.</param>
        /// <param name="width">Desired width (optional, uses SVG's native width if not specified).</param>
        /// <param name="height">Desired height (optional, uses SVG's native height if not specified).</param>
        /// <returns>SKBitmap containing the rendered SVG.</returns>
        public static SKBitmap LoadSvgToBitmap(string svgPath, int? width = null, int? height = null)
        {
            if (string.IsNullOrEmpty(svgPath))
                throw new ArgumentException("SVG path cannot be null or empty.", nameof(svgPath));
            if (!File.Exists(svgPath))
                throw new FileNotFoundException("SVG file not found.", svgPath);

            using var svg = new SKSvg();
            if (svg.Load(svgPath) is null)
                throw new InvalidOperationException($"Failed to load SVG: {svgPath}");

            var svgWidth = svg.Picture?.CullRect.Width ?? 0;
            var svgHeight = svg.Picture?.CullRect.Height ?? 0;

            if (svgWidth <= 0 || svgHeight <= 0)
                throw new InvalidOperationException("SVG has invalid dimensions.");

            int targetWidth = width ?? (int)Math.Ceiling(svgWidth);
            int targetHeight = height ?? (int)Math.Ceiling(svgHeight);

            var bitmap = new SKBitmap(targetWidth, targetHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Transparent);
                float scaleX = (float)targetWidth / svgWidth;
                float scaleY = (float)targetHeight / svgHeight;
                var matrix = SKMatrix.CreateScale(scaleX, scaleY);
                canvas.DrawPicture(svg.Picture, ref matrix);
            }

            return bitmap;
        }

        /// <summary>
        /// Loads an SVG string and renders it to an SKBitmap.
        /// </summary>
        /// <param name="svgContent">The SVG XML content.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <returns>SKBitmap containing the rendered SVG.</returns>
        public static SKBitmap LoadSvgContentToBitmap(string svgContent, int width, int height)
        {
            if (string.IsNullOrEmpty(svgContent))
                throw new ArgumentException("SVG content cannot be null or empty.", nameof(svgContent));

            using var svg = new SKSvg();
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent));
            if (svg.Load(stream) is null)
                throw new InvalidOperationException("Failed to load SVG content.");

            var svgWidth = svg.Picture?.CullRect.Width ?? 0;
            var svgHeight = svg.Picture?.CullRect.Height ?? 0;

            if (svgWidth <= 0 || svgHeight <= 0)
            {
                // If SVG doesn't have dimensions, use target dimensions
                svgWidth = width;
                svgHeight = height;
            }

            var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Transparent);
                float scaleX = (float)width / svgWidth;
                float scaleY = (float)height / svgHeight;
                var matrix = SKMatrix.CreateScale(scaleX, scaleY);
                canvas.DrawPicture(svg.Picture, ref matrix);
            }

            return bitmap;
        }

        /// <summary>
        /// Extension method to add SVG as image to PDF content editor.
        /// </summary>
        /// <param name="editor">The PDF content editor.</param>
        /// <param name="svgPath">Path to the SVG file.</param>
        /// <param name="bounds">Position and size on the page.</param>
        /// <returns>The editor instance for fluent chaining.</returns>
        public static PdfContentEditor AddSvgFromFile(this PdfContentEditor editor, string svgPath, PdfRectangle bounds)
        {
            if (editor is null)
                throw new ArgumentNullException(nameof(editor));

            // Render SVG to bitmap at the size specified in bounds (in points, converted to pixels for better quality if needed)
            // For now, we'll just use the bounds width/height as pixel dimensions for the bitmap
            using var bitmap = LoadSvgToBitmap(svgPath, (int)bounds.Width, (int)bounds.Height);
            return editor.AddSkiaImage(bitmap, bounds);
        }
    }
}
