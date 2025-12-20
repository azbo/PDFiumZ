using PDFiumCore.HighLevel;
using SkiaSharp;
using System;
using System.IO;

namespace PDFiumCore.SkiaSharp
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
    }
}
