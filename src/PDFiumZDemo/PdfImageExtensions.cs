using PDFiumZ;
using PDFiumZ.HighLevel;
using SkiaSharp;
using Svg.Skia;
using System;
using System.IO;

namespace PDFiumZ.SkiaSharp
{
    /// <summary>
    /// Extension methods for integrating SkiaSharp and Svg.Skia with PDFiumZ.
    /// </summary>
    public static class PdfImageExtensions
    {
        #region SkiaSharp Image Extensions

        /// <summary>
        /// Converts a PdfImage to an SKBitmap (zero-copy).
        /// </summary>
        public static unsafe SKBitmap ToSkiaBitmap(this PdfImage pdfImage)
        {
            if (pdfImage is null)
                throw new ArgumentNullException(nameof(pdfImage));

            var info = new SKImageInfo(
                pdfImage.Width,
                pdfImage.Height,
                SKColorType.Bgra8888,
                pdfImage.Format == FPDFBitmapFormat.BGRA
                    ? SKAlphaType.Premul
                    : SKAlphaType.Opaque);

            var bitmap = new SKBitmap();
            bitmap.InstallPixels(info, pdfImage.Buffer, pdfImage.Stride);

            return bitmap;
        }

        /// <summary>
        /// Converts a PdfImage to an SKImage.
        /// </summary>
        public static SKImage ToSkiaImage(this PdfImage pdfImage)
        {
            if (pdfImage is null)
                throw new ArgumentNullException(nameof(pdfImage));

            using var bitmap = pdfImage.ToSkiaBitmap();
            return SKImage.FromBitmap(bitmap);
        }

        /// <summary>
        /// Saves the image to a file using SkiaSharp encoding.
        /// </summary>
        public static void SaveAsSkiaPng(this PdfImage pdfImage, string filePath, int quality = 100) =>
            SaveAsSkiaFormat(pdfImage, filePath, SKEncodedImageFormat.Png, quality);

        /// <summary>
        /// Saves the image as JPEG.
        /// </summary>
        public static void SaveAsSkiaJpeg(this PdfImage pdfImage, string filePath, int quality = 90) =>
            SaveAsSkiaFormat(pdfImage, filePath, SKEncodedImageFormat.Jpeg, quality);

        /// <summary>
        /// Saves the image as WebP.
        /// </summary>
        public static void SaveAsSkiaWebP(this PdfImage pdfImage, string filePath, int quality = 90) =>
            SaveAsSkiaFormat(pdfImage, filePath, SKEncodedImageFormat.Webp, quality);

        private static void SaveAsSkiaFormat(PdfImage pdfImage, string filePath, SKEncodedImageFormat format, int quality)
        {
            if (pdfImage is null) throw new ArgumentNullException(nameof(pdfImage));
            if (filePath is null) throw new ArgumentNullException(nameof(filePath));

            using var skiaBitmap = pdfImage.ToSkiaBitmap();
            using var fileStream = File.OpenWrite(filePath);
            skiaBitmap.Encode(fileStream, format, quality);
        }

        /// <summary>
        /// Converts SKBitmap to BGRA byte array for PDF embedding.
        /// </summary>
        public static byte[] ToBgraBytes(this SKBitmap bitmap)
        {
            if (bitmap is null) throw new ArgumentNullException(nameof(bitmap));

            // If already BGRA_8888, we can just copy
            if (bitmap.ColorType == SKColorType.Bgra8888)
            {
                var bytes = new byte[bitmap.Width * bitmap.Height * 4];
                System.Runtime.InteropServices.Marshal.Copy(bitmap.GetPixels(), bytes, 0, bytes.Length);
                return bytes;
            }

            // Otherwise convert
            var info = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var convertedBitmap = new SKBitmap(info);
            using (var canvas = new SKCanvas(convertedBitmap))
            {
                canvas.DrawBitmap(bitmap, 0, 0);
            }

            var result = new byte[convertedBitmap.Width * convertedBitmap.Height * 4];
            System.Runtime.InteropServices.Marshal.Copy(convertedBitmap.GetPixels(), result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Adds a SkiaSharp bitmap as an image to the PDF page (fluent API).
        /// </summary>
        public static PdfContentEditor Image(this PdfContentEditor editor, SKBitmap bitmap, PdfRectangle bounds)
        {
            if (editor is null) throw new ArgumentNullException(nameof(editor));
            if (bitmap is null) throw new ArgumentNullException(nameof(bitmap));

            var bgraData = bitmap.ToBgraBytes();
            return editor.Image(bgraData, bitmap.Width, bitmap.Height, bounds);
        }

        /// <summary>
        /// Adds a SkiaSharp image as an image to the PDF page (fluent API).
        /// </summary>
        public static PdfContentEditor Image(this PdfContentEditor editor, SKImage image, PdfRectangle bounds)
        {
            if (editor is null) throw new ArgumentNullException(nameof(editor));
            if (image is null) throw new ArgumentNullException(nameof(image));

            using var bitmap = SKBitmap.FromImage(image);
            return editor.Image(bitmap, bounds);
        }

        /// <summary>
        /// Loads an image or SVG (from file path or SVG content string) and adds it to the PDF page (fluent API).
        /// </summary>
        public static PdfContentEditor Image(this PdfContentEditor editor, string source, PdfRectangle bounds)
        {
            if (editor is null) throw new ArgumentNullException(nameof(editor));
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));

            // Check if it's SVG content
            if (source.TrimStart().StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
            {
                using var bitmap = LoadSvgContentToBitmap(source, (int)bounds.Width, (int)bounds.Height);
                return editor.Image(bitmap, bounds);
            }

            // Otherwise treat as file path
            if (source.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                using var bitmap = LoadSvgToBitmap(source, (int)bounds.Width, (int)bounds.Height);
                return editor.Image(bitmap, bounds);
            }

            using var decodedBitmap = SKBitmap.Decode(source);
            if (decodedBitmap == null)
                throw new InvalidOperationException($"Failed to decode image: {source}");

            return editor.Image(decodedBitmap, bounds);
        }

        /// <summary>
        /// Adds an image or SVG from a byte array to the PDF page (fluent API).
        /// </summary>
        public static PdfContentEditor Image(this PdfContentEditor editor, byte[] data, PdfRectangle bounds)
        {
            if (editor is null) throw new ArgumentNullException(nameof(editor));
            if (data is null) throw new ArgumentNullException(nameof(data));

            // Try decoding as standard image first
            using var decodedBitmap = SKBitmap.Decode(data);
            if (decodedBitmap != null)
                return editor.Image(decodedBitmap, bounds);

            // Try as SVG
            try
            {
                using var stream = new MemoryStream(data);
                using var svg = new SKSvg();
                if (svg.Load(stream) != null)
                {
                    using var bitmap = RenderSvgToBitmap(svg, (int)bounds.Width, (int)bounds.Height);
                    return editor.Image(bitmap, bounds);
                }
            }
            catch { /* Fall through to exception */ }

            throw new InvalidOperationException("Failed to decode image or SVG from byte array.");
        }

        /// <summary>
        /// Adds an image or SVG from a stream to the PDF page (fluent API).
        /// </summary>
        public static PdfContentEditor Image(this PdfContentEditor editor, Stream stream, PdfRectangle bounds)
        {
            if (editor is null) throw new ArgumentNullException(nameof(editor));
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            if (stream is MemoryStream ms)
            {
                return editor.Image(ms.ToArray(), bounds);
            }

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return editor.Image(memoryStream.ToArray(), bounds);
        }

        #endregion

        #region SVG Extensions

        /// <summary>
        /// Loads an SVG file and renders it to an SKBitmap.
        /// </summary>
        public static SKBitmap LoadSvgToBitmap(string svgPath, int? width = null, int? height = null)
        {
            if (string.IsNullOrEmpty(svgPath)) throw new ArgumentException("SVG path cannot be null or empty.", nameof(svgPath));
            if (!File.Exists(svgPath)) throw new FileNotFoundException("SVG file not found.", svgPath);

            using var svg = new SKSvg();
            if (svg.Load(svgPath) is null)
                throw new InvalidOperationException($"Failed to load SVG: {svgPath}");

            return RenderSvgToBitmap(svg, width, height);
        }

        /// <summary>
        /// Loads an SVG string and renders it to an SKBitmap.
        /// </summary>
        public static SKBitmap LoadSvgContentToBitmap(string svgContent, int width, int height)
        {
            if (string.IsNullOrEmpty(svgContent)) throw new ArgumentException("SVG content cannot be null or empty.", nameof(svgContent));

            using var svg = new SKSvg();
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent));
            if (svg.Load(stream) is null)
                throw new InvalidOperationException("Failed to load SVG content.");

            return RenderSvgToBitmap(svg, width, height);
        }

        private static SKBitmap RenderSvgToBitmap(SKSvg svg, int? width, int? height)
        {
            var svgWidth = svg.Picture?.CullRect.Width ?? 0;
            var svgHeight = svg.Picture?.CullRect.Height ?? 0;

            if (svgWidth <= 0 || svgHeight <= 0)
            {
                if (width.HasValue && height.HasValue)
                {
                    svgWidth = width.Value;
                    svgHeight = height.Value;
                }
                else
                {
                    throw new InvalidOperationException("SVG has invalid dimensions and no target dimensions provided.");
                }
            }

            int targetWidth = width ?? (int)Math.Ceiling(svgWidth);
            int targetHeight = height ?? (int)Math.Ceiling(svgHeight);

            var bitmap = new SKBitmap(targetWidth, targetHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Transparent);
                float scaleX = (float)targetWidth / svgWidth;
                float scaleY = (float)targetHeight / svgHeight;
                var matrix = SKMatrix.CreateScale(scaleX, scaleY);
                canvas.DrawPicture(svg.Picture, in matrix);
            }

            return bitmap;
        }

        #endregion
    }
}
