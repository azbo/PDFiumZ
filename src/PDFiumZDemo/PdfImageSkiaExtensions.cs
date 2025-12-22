using PDFiumZ;
using PDFiumZ.HighLevel;
using SkiaSharp;
using System;
using System.IO;

namespace PDFiumZ.SkiaSharp
{
    /// <summary>
    /// Extension methods for integrating PdfImage with SkiaSharp.
    /// </summary>
    public static class PdfImageSkiaExtensions
    {
        /// <summary>
        /// Converts a PdfImage to an SKBitmap (zero-copy).
        /// </summary>
        /// <param name="pdfImage">The PDF image to convert.</param>
        /// <returns>SKBitmap wrapping the same pixel buffer.</returns>
        /// <exception cref="ArgumentNullException">pdfImage is null.</exception>
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
        /// <param name="pdfImage">The PDF image to convert.</param>
        /// <returns>SKImage created from the bitmap data.</returns>
        /// <exception cref="ArgumentNullException">pdfImage is null.</exception>
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
        /// <param name="pdfImage">The PDF image to save.</param>
        /// <param name="filePath">The file path to save to.</param>
        /// <param name="quality">The encoding quality (0-100).</param>
        /// <exception cref="ArgumentNullException">pdfImage or filePath is null.</exception>
        public static void SaveAsSkiaPng(this PdfImage pdfImage, string filePath, int quality = 100)
        {
            if (pdfImage is null)
                throw new ArgumentNullException(nameof(pdfImage));
            if (filePath is null)
                throw new ArgumentNullException(nameof(filePath));

            using var skiaBitmap = pdfImage.ToSkiaBitmap();
            using var fileStream = File.OpenWrite(filePath);
            skiaBitmap.Encode(fileStream, SKEncodedImageFormat.Png, quality);
        }

        /// <summary>
        /// Saves the image as JPEG.
        /// </summary>
        /// <param name="pdfImage">The PDF image to save.</param>
        /// <param name="filePath">The file path to save to.</param>
        /// <param name="quality">The encoding quality (0-100).</param>
        /// <exception cref="ArgumentNullException">pdfImage or filePath is null.</exception>
        public static void SaveAsSkiaJpeg(this PdfImage pdfImage, string filePath, int quality = 90)
        {
            if (pdfImage is null)
                throw new ArgumentNullException(nameof(pdfImage));
            if (filePath is null)
                throw new ArgumentNullException(nameof(filePath));

            using var skiaBitmap = pdfImage.ToSkiaBitmap();
            using var fileStream = File.OpenWrite(filePath);
            skiaBitmap.Encode(fileStream, SKEncodedImageFormat.Jpeg, quality);
        }

        /// <summary>
        /// Saves the image as WebP.
        /// </summary>
        /// <param name="pdfImage">The PDF image to save.</param>
        /// <param name="filePath">The file path to save to.</param>
        /// <param name="quality">The encoding quality (0-100).</param>
        /// <exception cref="ArgumentNullException">pdfImage or filePath is null.</exception>
        public static void SaveAsSkiaWebP(this PdfImage pdfImage, string filePath, int quality = 90)
        {
            if (pdfImage is null)
                throw new ArgumentNullException(nameof(pdfImage));
            if (filePath is null)
                throw new ArgumentNullException(nameof(filePath));

            using var skiaBitmap = pdfImage.ToSkiaBitmap();
            using var fileStream = File.OpenWrite(filePath);
            skiaBitmap.Encode(fileStream, SKEncodedImageFormat.Webp, quality);
        }

        /// <summary>
        /// Converts SKBitmap to BGRA byte array for PDF embedding.
        /// </summary>
        /// <param name="bitmap">The SKBitmap to convert.</param>
        /// <returns>BGRA pixel data byte array.</returns>
        /// <exception cref="ArgumentNullException">bitmap is null.</exception>
        public static byte[] ToBgraBytes(this SKBitmap bitmap)
        {
            if (bitmap is null)
                throw new ArgumentNullException(nameof(bitmap));

            // Ensure bitmap is in BGRA format
            var info = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);

            using var convertedBitmap = new SKBitmap(info);
            using (var canvas = new SKCanvas(convertedBitmap))
            {
                canvas.DrawBitmap(bitmap, 0, 0);
            }

            // Copy pixel data
            var bytes = new byte[convertedBitmap.Width * convertedBitmap.Height * 4];
            System.Runtime.InteropServices.Marshal.Copy(convertedBitmap.GetPixels(), bytes, 0, bytes.Length);

            return bytes;
        }

        /// <summary>
        /// Extension method to add SKBitmap as image to PDF content editor.
        /// </summary>
        /// <param name="editor">The PDF content editor.</param>
        /// <param name="bitmap">The SKBitmap to add.</param>
        /// <param name="bounds">Position and size on the page.</param>
        /// <returns>The editor instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">editor or bitmap is null.</exception>
        public static PdfContentEditor AddSkiaImage(this PdfContentEditor editor, SKBitmap bitmap, PdfRectangle bounds)
        {
            if (editor is null)
                throw new ArgumentNullException(nameof(editor));
            if (bitmap is null)
                throw new ArgumentNullException(nameof(bitmap));

            var bgraData = bitmap.ToBgraBytes();
            editor.AddImage(bgraData, bitmap.Width, bitmap.Height, bounds);

            return editor;
        }

        /// <summary>
        /// Extension method to add SKImage as image to PDF content editor.
        /// </summary>
        /// <param name="editor">The PDF content editor.</param>
        /// <param name="image">The SKImage to add.</param>
        /// <param name="bounds">Position and size on the page.</param>
        /// <returns>The editor instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">editor or image is null.</exception>
        public static PdfContentEditor AddSkiaImage(this PdfContentEditor editor, SKImage image, PdfRectangle bounds)
        {
            if (editor is null)
                throw new ArgumentNullException(nameof(editor));
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            using var bitmap = SKBitmap.FromImage(image);
            return editor.AddSkiaImage(bitmap, bounds);
        }

        /// <summary>
        /// Loads an image file and adds it to the PDF page using SkiaSharp.
        /// </summary>
        /// <param name="editor">The PDF content editor.</param>
        /// <param name="imagePath">Path to the image file.</param>
        /// <param name="bounds">Position and size on the page.</param>
        /// <returns>The editor instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">editor or imagePath is null.</exception>
        /// <exception cref="FileNotFoundException">Image file not found.</exception>
        /// <exception cref="InvalidOperationException">Failed to decode image.</exception>
        public static PdfContentEditor AddImageFromFile(this PdfContentEditor editor, string imagePath, PdfRectangle bounds)
        {
            if (editor is null)
                throw new ArgumentNullException(nameof(editor));
            if (imagePath is null)
                throw new ArgumentNullException(nameof(imagePath));
            if (!File.Exists(imagePath))
                throw new FileNotFoundException($"Image file not found: {imagePath}", imagePath);

            using var bitmap = SKBitmap.Decode(imagePath);
            if (bitmap == null)
                throw new InvalidOperationException($"Failed to decode image: {imagePath}");

            return editor.AddSkiaImage(bitmap, bounds);
        }
    }
}
