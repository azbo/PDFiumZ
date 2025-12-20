using System;
using System.Threading.Tasks;
using PDFiumCore;
using SkiaSharp;
using RectangleF = System.Drawing.RectangleF;

namespace PDFiumCoreDemo
{
    public unsafe class PdfImage : IDisposable
    {
        private readonly FpdfBitmapT _pdfBitmap;

        public int Width { get; }

        public int Height { get; }

        public int Stride { get; }

        public SKBitmap ImageData { get; }

        internal PdfImage(
            FpdfBitmapT pdfBitmap,
            int width,
            int height)
        {
            _pdfBitmap = pdfBitmap;
            var scan0 = fpdfview.FPDFBitmapGetBuffer(pdfBitmap);
            Stride = fpdfview.FPDFBitmapGetStride(pdfBitmap);
            Height = height;
            Width = width;

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888);
            ImageData = new SKBitmap();
            ImageData.InstallPixels(info, scan0, Stride);
        }

        public void Dispose()
        {
            ImageData.Dispose();
            fpdfview.FPDFBitmapDestroy(_pdfBitmap);
        }
    }
}
