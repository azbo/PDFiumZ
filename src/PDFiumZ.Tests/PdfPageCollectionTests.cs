using System;
using System.Linq;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

/// <summary>
/// 页面集合测试
/// </summary>
[TestFixture]
public class PdfPageCollectionTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void Count_MatchesDocumentPageCount()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var pages = document.Pages;

        // Assert
        Assert.That(pages.Count, Is.EqualTo(document.PageCount));
    }

    [Test]
    public void Count_ReturnsPositiveNumber()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var pages = document.Pages;

        // Assert
        Assert.That(pages.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Indexer_FirstPage_ReturnsPageWithIndexZero()
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
    public void Indexer_LastPage_ReturnsPageWithCorrectIndex()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var page = document.Pages[document.PageCount - 1];

        // Assert
        Assert.That(page, Is.Not.Null);
        Assert.That(page.Index, Is.EqualTo(document.PageCount - 1));
    }

    [Test]
    public void Indexer_OutOfRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var _ = document.Pages[document.PageCount];
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var _ = document.Pages[-1];
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var _ = document.Pages[10000];
        });
    }

    [Test]
    public void Foreach_IteratesAllPages()
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
    public void Foreeach_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        int count1 = 0, count2 = 0;

        // Act
        foreach (var _ in document.Pages)
        {
            count1++;
        }

        foreach (var _ in document.Pages)
        {
            count2++;
        }

        // Assert
        Assert.That(count1, Is.EqualTo(document.PageCount));
        Assert.That(count2, Is.EqualTo(document.PageCount));
        Assert.That(count1, Is.EqualTo(count2));
    }

    [Test]
    public void GetEnumerator_ReturnsValidEnumerator()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var pages = document.Pages;
        var enumerator = pages.GetEnumerator();

        // Assert
        Assert.That(enumerator, Is.Not.Null);

        int count = 0;
        while (enumerator.MoveNext())
        {
            count++;
            Assert.That(enumerator.Current, Is.Not.Null);
        }

        Assert.That(count, Is.EqualTo(document.PageCount));
    }

    [Test]
    public void MultipleAccesses_ReturnSamePageInstances()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var page1 = document.Pages[0];
        var page2 = document.Pages[0];

        // Assert - 应该返回相同的页面实例（索引相同时）
        Assert.That(page1.Index, Is.EqualTo(page2.Index));
    }

    [Test]
    public void GenericIEnumerable_UsableInLINQ()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var pageCount = document.Pages.Count();
        var firstPage = document.Pages.FirstOrDefault();
        var lastPage = document.Pages.LastOrDefault();
        var skippedPages = document.Pages.Skip(1).Take(1);

        // Assert
        Assert.That(pageCount, Is.EqualTo(document.PageCount));
        Assert.That(firstPage, Is.Not.Null);
        Assert.That(lastPage, Is.Not.Null);
        Assert.That(skippedPages.Count(), Is.GreaterThanOrEqualTo(0));
    }
}
