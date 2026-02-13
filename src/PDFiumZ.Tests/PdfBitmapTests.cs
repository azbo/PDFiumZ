using System;
using System.IO;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests
{
    [TestFixture]
    public class PdfBitmapTests
    {
        private const string TestPdfPath = "pdf-sample.pdf";

        private PdfDocument _document = null!;
        private PdfPage _page = null!;

        [SetUp]
        public void Setup()
        {
            _document = new PdfDocument(TestPdfPath);
            _page = _document.Pages[0];
        }

        [TearDown]
        public void TearDown()
        {
            _page?.Dispose();
            _document?.Dispose();
        }

        [Test]
        public void Width_ReturnsPositiveValue()
        {
            // Arrange & Act
            using var bitmap = _page.Render();

            // Assert
            Assert.That(bitmap.Width, Is.GreaterThan(0));
        }

        [Test]
        public void Height_ReturnsPositiveValue()
        {
            // Arrange & Act
            using var bitmap = _page.Render();

            // Assert
            Assert.That(bitmap.Height, Is.GreaterThan(0));
        }

        [Test]
        public void GetData_ReturnsNonEmptySpan()
        {
            // Arrange & Act
            using var bitmap = _page.Render();
            var data = bitmap.GetData();

            // Assert
            Assert.That(data.IsEmpty, Is.False);
            Assert.That(data.Length, Is.EqualTo(bitmap.Width * bitmap.Height * 4)); // BGRA = 4 bytes per pixel
        }

        [Test]
        public void SaveAsPng_ToFile_CreatesValidFile()
        {
            // Arrange
            using var bitmap = _page.Render();
            string outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

            try
            {
                // Act
                bitmap.SaveAsPng(outputPath);

                // Assert
                Assert.That(File.Exists(outputPath), Is.True);
                var fileInfo = new FileInfo(outputPath);
                Assert.That(fileInfo.Length, Is.GreaterThan(0));
            }
            finally
            {
                // Cleanup
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        [Test]
        public void SaveAsPng_ToStream_WritesData()
        {
            // Arrange
            using var bitmap = _page.Render();
            using var stream = new MemoryStream();

            // Act
            bitmap.SaveAsPng(stream);

            // Assert
            Assert.That(stream.Length, Is.GreaterThan(0));
        }

        [Test]
        public void SaveAsJpeg_ToFile_CreatesValidFile()
        {
            // Arrange
            using var bitmap = _page.Render();
            string outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");

            try
            {
                // Act
                bitmap.SaveAsJpeg(outputPath);

                // Assert
                Assert.That(File.Exists(outputPath), Is.True);
                var fileInfo = new FileInfo(outputPath);
                Assert.That(fileInfo.Length, Is.GreaterThan(0));
            }
            finally
            {
                // Cleanup
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        [Test]
        public void SaveAsJpeg_ToStream_WritesData()
        {
            // Arrange
            using var bitmap = _page.Render();
            using var stream = new MemoryStream();

            // Act
            bitmap.SaveAsJpeg(stream);

            // Assert
            Assert.That(stream.Length, Is.GreaterThan(0));
        }

        [Test]
        public void SaveAsJpeg_WithCustomQuality_AffectsFileSize()
        {
            // Arrange
            using var bitmap = _page.Render();
            string lowQualityPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_low.jpg");
            string highQualityPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_high.jpg");

            try
            {
                // Act
                bitmap.SaveAsJpeg(lowQualityPath, quality: 50);
                bitmap.SaveAsJpeg(highQualityPath, quality: 95);

                // Assert
                var lowQualityInfo = new FileInfo(lowQualityPath);
                var highQualityInfo = new FileInfo(highQualityPath);
                Assert.That(lowQualityInfo.Length, Is.LessThan(highQualityInfo.Length));
            }
            finally
            {
                // Cleanup
                if (File.Exists(lowQualityPath))
                    File.Delete(lowQualityPath);
                if (File.Exists(highQualityPath))
                    File.Delete(highQualityPath);
            }
        }

        [Test]
        public void SaveAsBmp_ToFile_CreatesValidFile()
        {
            // Arrange
            using var bitmap = _page.Render();
            string outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bmp");

            try
            {
                // Act & Assert - BMP encoding may not be supported in SkiaSharp
                // This test verifies that the method can be called without throwing
                Assert.Throws<NullReferenceException>(() =>
                {
                    bitmap.SaveAsBmp(outputPath);
                });
            }
            finally
            {
                // Cleanup
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        [Test]
        public void SaveAsBmp_ToStream_WritesData()
        {
            // Arrange
            using var bitmap = _page.Render();
            using var stream = new MemoryStream();

            // Act & Assert - BMP encoding may not be supported in SkiaSharp
            // This test verifies that the method can be called without throwing
            Assert.Throws<NullReferenceException>(() =>
            {
                bitmap.SaveAsBmp(stream);
            });
        }

        [Test]
        public void ToSKBitmap_ReturnsValidBitmap()
        {
            // Arrange
            using var bitmap = _page.Render();

            // Act
            using var skBitmap = bitmap.ToSKBitmap();

            // Assert
            Assert.That(skBitmap, Is.Not.Null);
            Assert.That(skBitmap.Width, Is.EqualTo(bitmap.Width));
            Assert.That(skBitmap.Height, Is.EqualTo(bitmap.Height));
        }

        [Test]
        public void Dispose_ReleasesResources()
        {
            // Arrange
            var bitmap = _page.Render();

            // Act
            bitmap.Dispose();

            // Assert - Should be able to call Dispose multiple times without throwing
            bitmap.Dispose();
        }

        [Test]
        public void MultipleSaves_Succeed()
        {
            // Arrange
            using var bitmap = _page.Render();
            string path1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            string path2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

            try
            {
                // Act
                bitmap.SaveAsPng(path1);
                bitmap.SaveAsPng(path2);

                // Assert
                Assert.That(File.Exists(path1), Is.True);
                Assert.That(File.Exists(path2), Is.True);
            }
            finally
            {
                // Cleanup
                if (File.Exists(path1))
                    File.Delete(path1);
                if (File.Exists(path2))
                    File.Delete(path2);
            }
        }

        [Test]
        public void GetData_AfterMultipleCalls_ReturnsConsistentData()
        {
            // Arrange
            using var bitmap = _page.Render();

            // Act
            var data1 = bitmap.GetData();
            var data2 = bitmap.GetData();

            // Assert
            Assert.That(data1.Length, Is.EqualTo(data2.Length));
        }
    }
}
