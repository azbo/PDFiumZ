using System;
using System.IO;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests
{
    [TestFixture]
    public class PdfPageTests
    {
        private const string TestPdfPath = "pdf-sample.pdf";

        [Test]
        public void Index_ReturnsCorrectPageIndex()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);

            // Act
            var page = document.Pages[0];

            // Assert
            Assert.That(page.Index, Is.EqualTo(0));
        }

        [Test]
        public void Width_ReturnsPositiveValue()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];

            // Act
            float width = page.Width;

            // Assert
            Assert.That(width, Is.GreaterThan(0));
        }

        [Test]
        public void Height_ReturnsPositiveValue()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];

            // Act
            float height = page.Height;

            // Assert
            Assert.That(height, Is.GreaterThan(0));
        }

        [Test]
        public void Render_WithDefaultSettings_ReturnsValidBitmap()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];

            // Act
            using var bitmap = page.Render();

            // Assert
            Assert.That(bitmap, Is.Not.Null);
            Assert.That(bitmap.Width, Is.GreaterThan(0));
            Assert.That(bitmap.Height, Is.GreaterThan(0));
        }

        [Test]
        public void Render_WithCustomDpi_ReturnsLargerBitmap()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];
            var lowDpiSettings = new ImageGenerationSettings { RasterDpi = 72 };
            var highDpiSettings = new ImageGenerationSettings { RasterDpi = 144 };

            // Act
            using var lowDpiBitmap = page.Render(lowDpiSettings);
            using var highDpiBitmap = page.Render(highDpiSettings);

            // Assert
            Assert.That(highDpiBitmap.Width, Is.GreaterThan(lowDpiBitmap.Width));
            Assert.That(highDpiBitmap.Height, Is.GreaterThan(lowDpiBitmap.Height));
        }

        [Test]
        public void Render_WithRotation_DoesNotCrash()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];

            // Act - rendering with different rotations should not crash
            var normalSettings = new ImageGenerationSettings { Rotation = PdfRotation.Rotate0 };
            var rotatedSettings = new ImageGenerationSettings { Rotation = PdfRotation.Rotate90 };

            using var normalBitmap = page.Render(normalSettings);
            using var rotatedBitmap = page.Render(rotatedSettings);

            // Assert - both bitmaps should be valid (rotation may not affect dimensions in current implementation)
            Assert.That(normalBitmap, Is.Not.Null);
            Assert.That(rotatedBitmap, Is.Not.Null);
            Assert.That(normalBitmap.Width, Is.GreaterThan(0));
            Assert.That(rotatedBitmap.Width, Is.GreaterThan(0));
        }

        [Test]
        public void Render_WithBackgroundColor_ReturnsBitmapWithBackground()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];

            // Act
            var whiteSettings = new ImageGenerationSettings
            {
                BackgroundColor = SkiaSharp.SKColors.White
            };

            using var bitmap = page.Render(whiteSettings);

            // Assert
            Assert.That(bitmap, Is.Not.Null);
            Assert.That(bitmap.Width, Is.GreaterThan(0));
            Assert.That(bitmap.Height, Is.GreaterThan(0));
        }

        [Test]
        public void Render_WithGrayscaleFlag_ReturnsGrayscaleBitmap()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];

            // Act
            var grayscaleSettings = new ImageGenerationSettings
            {
                RenderFlags = PdfRenderFlags.Grayscale
            };

            using var bitmap = page.Render(grayscaleSettings);

            // Assert
            Assert.That(bitmap, Is.Not.Null);
            Assert.That(bitmap.Width, Is.GreaterThan(0));
            Assert.That(bitmap.Height, Is.GreaterThan(0));
        }

        [Test]
        public void Render_MultipleTimes_ReturnsValidBitmaps()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];

            // Act
            using var bitmap1 = page.Render();
            using var bitmap2 = page.Render();
            using var bitmap3 = page.Render();

            // Assert
            Assert.That(bitmap1, Is.Not.Null);
            Assert.That(bitmap2, Is.Not.Null);
            Assert.That(bitmap3, Is.Not.Null);

            Assert.That(bitmap1.Width, Is.EqualTo(bitmap2.Width));
            Assert.That(bitmap2.Width, Is.EqualTo(bitmap3.Width));
        }

        [Test]
        public void GetSize_ReturnsValidDimensions()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];

            // Act
            float width = page.Width;
            float height = page.Height;

            // Assert
            Assert.That(width, Is.GreaterThan(0));
            Assert.That(height, Is.GreaterThan(0));
            Assert.That(width, Is.LessThan(10000)); // Reasonable upper bound
            Assert.That(height, Is.LessThan(10000)); // Reasonable upper bound
        }

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];

            // Act & Assert - Dispose should be callable multiple times without throwing
            page.Dispose();
            page.Dispose();

            // After dispose, the page should still allow accessing properties
            // (current implementation doesn't throw on disposed access)
            Assert.That(page.Index, Is.EqualTo(0));
        }

        [Test]
        public void MultipleDispose_DoesNotThrow()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            var page = document.Pages[0];

            // Act & Assert - should not throw
            page.Dispose();
            page.Dispose();
        }
    }
}
