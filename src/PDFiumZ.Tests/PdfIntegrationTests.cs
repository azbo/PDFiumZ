using System;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

/// <summary>
/// 集成测试 - 多个功能组合使用
/// </summary>
[TestFixture]
public class PdfIntegrationTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [Test]
    public void LoadAndRender_CompleteWorkflow_Works()
    {
        // Arrange & Act
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];

        // 测试多个操作组合
        int pageCount = document.PageCount;
        float width = page.Width;
        float height = page.Height;

        using var bitmap = page.Render();

        // Assert - 所有操作都应该成功
        Assert.That(pageCount, Is.GreaterThan(0));
        Assert.That(width, Is.GreaterThan(0));
        Assert.That(height, Is.GreaterThan(0));
        Assert.That(bitmap, Is.Not.Null);
        Assert.That(bitmap.Width, Is.GreaterThan(0));
    }

    [Test]
    public void IterateAndRenderAllPages_CompleteWorkflow_Works()
    {
        // Arrange & Act
        using var document = new PdfDocument(TestPdfPath);

        int renderedCount = 0;
        foreach (var page in document.Pages)
        {
            // 测试每个页面的属性
            float width = page.Width;
            float height = page.Height;

            // 渲染每个页面
            using var bitmap = page.Render();

            // Assert
            Assert.That(width, Is.GreaterThan(0));
            Assert.That(height, Is.GreaterThan(0));
            Assert.That(bitmap, Is.Not.Null);
            Assert.That(bitmap.Width, Is.GreaterThan(0));

            renderedCount++;
        }

        // Assert - 渲染了所有页面
        Assert.That(renderedCount, Is.EqualTo(document.PageCount));
    }

    [Test]
    public void AccessMetadataMultipleTimes_CachingBehavior_Works()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act - 访问元数据多次
        var metadata1 = document.Metadata;
        var metadata2 = document.Metadata;
        var metadata3 = document.Metadata;

        // Assert - 应该返回相同的缓存实例
        Assert.That(metadata1, Is.SameAs(metadata2));
        Assert.That(metadata2, Is.SameAs(metadata3));
    }

    [Test]
    public void PageCollectionAndIteration_CompleteWorkflow_Works()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);

        // Act
        var pages = document.Pages;
        int countFromCount = pages.Count;
        int countFromIteration = 0;

        foreach (var page in pages)
        {
            countFromIteration++;
            // 访问页面属性
            float width = page.Width;
            float height = page.Height;
            int index = page.Index;

            // Assert
            Assert.That(width, Is.GreaterThan(0));
            Assert.That(height, Is.GreaterThan(0));
            Assert.That(index, Is.GreaterThanOrEqualTo(0));
        }

        // Assert
        Assert.That(countFromCount, Is.EqualTo(countFromIteration));
    }

    [Test]
    public void RenderAndSave_CompleteWorkflow_Works()
    {
        // Arrange
        using var document = new PdfDocument(TestPdfPath);
        var page = document.Pages[0];
        string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() + ".png");

        try
        {
            // Act
            using var bitmap = page.Render();
            bitmap.SaveAsPng(tempPath);

            // Assert
            Assert.That(bitmap, Is.Not.Null);
            Assert.That(System.IO.File.Exists(tempPath), Is.True);

            // Verify saved file can be read
            var fileBytes = System.IO.File.ReadAllBytes(tempPath);
            Assert.That(fileBytes.Length, Is.GreaterThan(0));
        }
        finally
        {
            // Cleanup
            if (System.IO.File.Exists(tempPath))
                System.IO.File.Delete(tempPath);
        }
    }

    [Test]
    public void MultipleDocumentsWithPages_CompleteWorkflow_Works()
    {
        // Arrange & Act
        using var doc1 = new PdfDocument(TestPdfPath);
        using var doc2 = new PdfDocument(TestPdfPath);

        // 访问两个文档的页面
        var page1 = doc1.Pages[0];
        var page2 = doc2.Pages[0];

        // 渲染两个文档的页面
        using var bitmap1 = page1.Render();
        using var bitmap2 = page2.Render();

        // Assert
        Assert.That(bitmap1, Is.Not.Null);
        Assert.That(bitmap2, Is.Not.Null);
        Assert.That(bitmap1.Width, Is.GreaterThan(0));
        Assert.That(bitmap2.Width, Is.GreaterThan(0));
    }

    [Test]
    public void ResourceCleanup_CompleteWorkflow_Works()
    {
        // Arrange
        byte[] fileBytes = System.IO.File.ReadAllBytes(TestPdfPath);
        string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() + ".png");

        // Act
        using (var document = new PdfDocument(fileBytes))
        using (var page = document.Pages[0])
        using (var bitmap = page.Render())
        {
            bitmap.SaveAsPng(tempPath);
        }

        // Assert - 所有资源都应该被正确释放
        Assert.That(System.IO.File.Exists(tempPath), Is.True);

        // Cleanup
        System.IO.File.Delete(tempPath);
    }

    [Test]
    public void Concurrency_CompleteWorkflow_Works()
    {
        // Arrange & Act - 测试并发访问
        using var document = new PdfDocument(TestPdfPath);

        // 同时访问多个属性
        int pageCount = document.PageCount;
        var pages = document.Pages;
        var metadata = document.Metadata;

        // 同时访问多个页面
        var page1 = pages[0];
        var page2 = pages.Count > 1 ? pages[1] : page1;
        var page3 = pages.Count > 2 ? pages[2] : page1;

        // Assert - 所有访问都应该成功
        Assert.That(pageCount, Is.GreaterThan(0));
        Assert.That(pages.Count, Is.EqualTo(pageCount));
        Assert.That(metadata, Is.Not.Null);
        Assert.That(page1, Is.Not.Null);
        if (pages.Count > 1) Assert.That(page2, Is.Not.Null);
        if (pages.Count > 2) Assert.That(page3, Is.Not.Null);
    }
}
