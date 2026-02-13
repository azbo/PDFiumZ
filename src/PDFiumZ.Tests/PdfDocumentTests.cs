using System;
using System.IO;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests
{
    [TestFixture]
    public class PdfDocumentTests
    {
        private const string TestPdfPath = "pdf-sample.pdf";

        [Test]
        public void Constructor_FromFilePath_LoadsDocument()
        {
            // Arrange & Act
            using var document = new PdfDocument(TestPdfPath);

            // Assert
            Assert.That(document, Is.Not.Null);
            Assert.That(document.PageCount, Is.GreaterThan(0));
        }

        [Test]
        public void Constructor_FromInvalidFilePath_ThrowsException()
        {
            // Arrange
            const string invalidPath = "nonexistent.pdf";

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                using var document = new PdfDocument(invalidPath);
            });

            Assert.That(ex?.Message, Does.Contain("Failed to load PDF document"));
        }

        [Test]
        public void Constructor_FromEmptyFilePath_ThrowsException()
        {
            // Arrange
            const string emptyPath = "";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var document = new PdfDocument(emptyPath);
            });
        }

        [Test]
        public void Constructor_FromStream_LoadsDocument()
        {
            // Arrange
            using var fs = File.OpenRead(TestPdfPath);

            // Act
            using var document = new PdfDocument(fs);

            // Assert
            Assert.That(document, Is.Not.Null);
            Assert.That(document.PageCount, Is.GreaterThan(0));
        }

        [Test]
        public void Constructor_FromByteArray_LoadsDocument()
        {
            // Arrange
            byte[] fileBytes = File.ReadAllBytes(TestPdfPath);

            // Act
            using var document = new PdfDocument(fileBytes);

            // Assert
            Assert.That(document, Is.Not.Null);
            Assert.That(document.PageCount, Is.GreaterThan(0));
        }

        [Test]
        public void Constructor_FromEmptyByteArray_ThrowsException()
        {
            // Arrange
            byte[] emptyBytes = Array.Empty<byte>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var document = new PdfDocument(emptyBytes);
            });
        }

        [Test]
        public void PageCount_ReturnsCorrectNumber()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);

            // Act
            int pageCount = document.PageCount;

            // Assert
            Assert.That(pageCount, Is.EqualTo(1));
        }

        [Test]
        public void Pages_ReturnsValidCollection()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);

            // Act
            var pages = document.Pages;

            // Assert
            Assert.That(pages, Is.Not.Null);
            Assert.That(pages.Count, Is.EqualTo(document.PageCount));
        }

        [Test]
        public void Pages_Indexer_ReturnsValidPage()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);

            // Act
            var page = document.Pages[0];

            // Assert
            Assert.That(page, Is.Not.Null);
            Assert.That(page.Index, Is.EqualTo(0));
        }

        [Test]
        public void Pages_Indexer_InvalidIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var page = document.Pages[-1];
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var page = document.Pages[999];
            });
        }

        [Test]
        public void Pages_Foreach_IteratesAllPages()
        {
            // Arrange
            using var document = new PdfDocument(TestPdfPath);
            int count = 0;

            // Act
            foreach (var page in document.Pages)
            {
                count++;
                Assert.That(page, Is.Not.Null);
            }

            // Assert
            Assert.That(count, Is.EqualTo(document.PageCount));
        }

        [Test]
        public void Dispose_ReleasesResources()
        {
            // Arrange
            var document = new PdfDocument(TestPdfPath);

            // Act
            document.Dispose();

            // Assert - accessing disposed document should throw
            Assert.Throws<ObjectDisposedException>(() =>
            {
                var _ = document.PageCount;
            });
        }

        [Test]
        public void MultipleDispose_DoesNotThrow()
        {
            // Arrange
            var document = new PdfDocument(TestPdfPath);

            // Act & Assert - should not throw
            document.Dispose();
            document.Dispose();
        }

        [Test]
        public void UsingStatement_AutomaticallyDisposes()
        {
            // Arrange & Act
            using (var document = new PdfDocument(TestPdfPath))
            {
                Assert.That(document.PageCount, Is.GreaterThan(0));
            }

            // Assert - document should be disposed here
            // If there were resource leaks, tests would fail in subsequent runs
        }
    }
}
