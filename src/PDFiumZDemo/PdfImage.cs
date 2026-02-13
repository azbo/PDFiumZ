using System;
using System.Threading.Tasks;
using PDFiumZ;

namespace PDFiumZDemo
{
    public unsafe class PdfImage
    {
        private readonly FpdfBitmapT _pdfBitmap;
        private readonly UnmanagedMemoryManager<byte> _mgr;

        public int Width { get; }

        public int Height { get; }

        public int Stride { get; }

        public UnmanagedMemoryManager<byte> ImageData { get; }

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
            _mgr = new UnmanagedMemoryManager<byte>((byte*)scan0, Stride * Height);

            ImageData = _mgr;
        }
    }
}
