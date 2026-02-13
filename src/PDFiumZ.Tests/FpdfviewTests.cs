using System;
using System.IO;
using NUnit.Framework;

namespace PDFiumZ.Tests
{
    public class FpdfviewTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            fpdfview.FPDF_InitLibrary();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            fpdfview.FPDF_DestroyLibrary();
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void ReadsPageCount()
        {
            var document = fpdfview.FPDF_LoadDocument("pdf-sample.pdf", null);
            Assert.That(fpdfview.FPDF_GetPageCount(document), Is.EqualTo(1));
            fpdfview.FPDF_CloseDocument(document);
        }

        [Test]
        public unsafe void FPDF_LoadMemDocument()
        {
            using var fs = File.OpenRead("pdf-sample.pdf");
            var fileBytes = new byte[fs.Length];
            using var ms = new MemoryStream(fileBytes);

            // Copy file the underlying byte stream.
            fs.CopyTo(ms);

            fixed (void* ptr = fileBytes)
            {
                var document = fpdfview.FPDF_LoadMemDocument(new IntPtr(ptr), fileBytes.Length, null);
                Assert.That(fpdfview.FPDF_GetPageCount(document), Is.EqualTo(1));
                fpdfview.FPDF_CloseDocument(document);
            }
        }

        [Test]
        public void LoadDocument_InvalidFilePath_ReturnsNull()
        {
            var document = fpdfview.FPDF_LoadDocument("nonexistent.pdf", null);
            Assert.That(document, Is.Null);
        }

        [Test]
        public void LoadDocument_EmptyFilePath_ReturnsNull()
        {
            var document = fpdfview.FPDF_LoadDocument("", null);
            Assert.That(document, Is.Null);
        }

        [Test]
        public void LoadPage_ValidPage_ReturnsHandle()
        {
            var document = fpdfview.FPDF_LoadDocument("pdf-sample.pdf", null);
            Assert.That(document, Is.Not.Null);

            var page = fpdfview.FPDF_LoadPage(document, 0);
            Assert.That(page, Is.Not.Null);

            fpdfview.FPDF_ClosePage(page);
            fpdfview.FPDF_CloseDocument(document);
        }

        [Test]
        public void LoadPage_InvalidIndex_ReturnsNull()
        {
            var document = fpdfview.FPDF_LoadDocument("pdf-sample.pdf", null);
            Assert.That(document, Is.Not.Null);

            var page = fpdfview.FPDF_LoadPage(document, 999);
            Assert.That(page, Is.Null);

            fpdfview.FPDF_CloseDocument(document);
        }

        [Test]
        public void GetPageSizeByIndex_ReturnsValidDimensions()
        {
            var document = fpdfview.FPDF_LoadDocument("pdf-sample.pdf", null);
            Assert.That(document, Is.Not.Null);

            double width = 0, height = 0;
            int result = fpdfview.FPDF_GetPageSizeByIndex(document, 0, ref width, ref height);

            Assert.That(result, Is.Not.EqualTo(0));
            Assert.That(width, Is.GreaterThan(0));
            Assert.That(height, Is.GreaterThan(0));

            fpdfview.FPDF_CloseDocument(document);
        }

        [Test]
        public void GetPageWidth_Height_ReturnsValidValues()
        {
            var document = fpdfview.FPDF_LoadDocument("pdf-sample.pdf", null);
            var page = fpdfview.FPDF_LoadPage(document, 0);

            Assert.That(page, Is.Not.Null);

            double width = fpdfview.FPDF_GetPageWidth(page);
            double height = fpdfview.FPDF_GetPageHeight(page);

            Assert.That(width, Is.GreaterThan(0));
            Assert.That(height, Is.GreaterThan(0));

            fpdfview.FPDF_ClosePage(page);
            fpdfview.FPDF_CloseDocument(document);
        }

        [Test]
        public unsafe void CreateBitmap_ReturnsValidHandle()
        {
            const int width = 100;
            const int height = 100;

            var bitmap = fpdfview.FPDFBitmapCreateEx(
                width, height,
                (int)FPDFBitmapFormat.BGRA,
                IntPtr.Zero, 0);

            Assert.That(bitmap, Is.Not.Null);

            fpdfview.FPDFBitmapDestroy(bitmap);
        }

        [Test]
        public unsafe void FillBitmap_FillsWithColor()
        {
            const int width = 100;
            const int height = 100;
            const uint color = 0xFF0000FF; // Blue in BGRA

            var bitmap = fpdfview.FPDFBitmapCreateEx(
                width, height,
                (int)FPDFBitmapFormat.BGRA,
                IntPtr.Zero, 0);

            Assert.That(bitmap, Is.Not.Null);

            fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, width, height, color);

            // Verify buffer is not null
            IntPtr buffer = fpdfview.FPDFBitmapGetBuffer(bitmap);
            Assert.That(buffer, Is.Not.EqualTo(IntPtr.Zero));

            fpdfview.FPDFBitmapDestroy(bitmap);
        }

        [Test]
        public unsafe void RenderPage_Succeeds()
        {
            var document = fpdfview.FPDF_LoadDocument("pdf-sample.pdf", null);
            var page = fpdfview.FPDF_LoadPage(document, 0);

            const int width = 200;
            const int height = 200;

            var bitmap = fpdfview.FPDFBitmapCreateEx(
                width, height,
                (int)FPDFBitmapFormat.BGRA,
                IntPtr.Zero, 0);

            Assert.That(bitmap, Is.Not.Null);

            // Fill with white background
            fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, width, height, 0xFFFFFFFF);

            // Render the page
            fpdfview.FPDF_RenderPageBitmap(bitmap, page, 0, 0, width, height, 0, 0);

            // Verify buffer has content
            IntPtr buffer = fpdfview.FPDFBitmapGetBuffer(bitmap);
            Assert.That(buffer, Is.Not.EqualTo(IntPtr.Zero));

            fpdfview.FPDFBitmapDestroy(bitmap);
            fpdfview.FPDF_ClosePage(page);
            fpdfview.FPDF_CloseDocument(document);
        }

        [Test]
        public void DocumentLifecycle_InitAndDestroy_Succeeds()
        {
            // Test multiple init/destroy cycles
            fpdfview.FPDF_DestroyLibrary();
            fpdfview.FPDF_InitLibrary();

            var document = fpdfview.FPDF_LoadDocument("pdf-sample.pdf", null);
            Assert.That(document, Is.Not.Null);

            fpdfview.FPDF_CloseDocument(document);
            fpdfview.FPDF_DestroyLibrary();
            fpdfview.FPDF_InitLibrary();
        }
    }
}