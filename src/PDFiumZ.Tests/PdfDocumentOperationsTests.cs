using System;
using System.IO;
using NUnit.Framework;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Tests;

[TestFixture]
public class PdfDocumentOperationsTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    [SetUp]
    public void Setup()
    {
        // 清理输出目录
        string outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "output");
        if (Directory.Exists(outputDir))
            Directory.Delete(outputDir, true);
    }

    [TearDown]
    public void TearDown()
    {
        // 清理输出目录
        string outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "output");
        if (Directory.Exists(outputDir))
            Directory.Delete(outputDir, true);
    }

    [Test]
    public void ImportPages_ImportPagesFromPdf()
    {
        // Arrange
        byte[] pdfBytes = File.ReadAllBytes(TestPdfPath);
        using var sourceDocument = new PdfDocument(pdfBytes);

        // Act
        Assert.DoesNotThrow(() =>
        {
            PdfDocumentOperations.ImportPages(sourceDocument, pdfBytes, new[] { 0 });
        });

        // Assert - 页数应该增加
        Assert.That(sourceDocument.PageCount, Is.GreaterThan(0));
    }

    [Test]
    public void ExportPages_ThenExportSinglePages()
    {
        // Arrange
        byte[] pdfBytes = File.ReadAllBytes(TestPdfPath);
        using var document = new PdfDocument(pdfBytes);

        string outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "output");

        // Act - 导出所有页面
        Assert.DoesNotThrow(() =>
        {
            PdfDocumentOperations.ExportPages(document, Path.Combine(outputDir, "all_pages.pdf"), new[] { 0, 1 });
        });

        // Act - 导出单页
        Assert.DoesNotThrow(() =>
        {
            PdfDocumentOperations.ExportPages(document, Path.Combine(outputDir, "single_page.pdf"), new[] { 0 });
        });

        // 验证文件存在
        Assert.That(File.Exists(Path.Combine(outputDir, "all_pages.pdf")), Is.True);
        Assert.That(File.Exists(Path.Combine(outputDir, "single_page.pdf")), Is.True);
    }

    [Test]
    public void DeletePages_ShouldRemovePages()
    {
        // Arrange
        byte[] pdfBytes = File.ReadAllBytes(TestPdfPath);
        using var document = new PdfDocument(pdfBytes);

        int pageCount = document.PageCount;

        // Act - 删除所有页面
        Assert.DoesNotThrow(() =>
        {
            PdfDocumentOperations.DeletePages(document, new[] { 0, 1 });
        });

        // Assert - 页数应该为 0
        Assert.That(document.PageCount, Is.EqualTo(0));
    }

    [Test]
    public void SplitDocument_ShouldCreateMultipleFiles()
    {
        // Arrange
        byte[] pdfBytes = File.ReadAllBytes(TestPdfPath);
        using var document = new PdfDocument(pdfBytes);

        string outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "output");

        // Act - 拆分文档
        string[] files = PdfDocumentOperations.SplitToSinglePageDocuments(document, outputDir);

        // Assert - 应该创建多文件
        Assert.That(files.Length, Is.EqualTo(2), "Should create 2 separate page files");
        foreach (var file in files)
        {
            Assert.That(File.Exists(file), Is.True, $"File {file} should exist");
        }
    }

    [Test]
    public void MergeDocuments_ShouldCombineMultipleFiles()
    {
        // Arrange
        byte[] pdfBytes = File.ReadAllBytes(TestPdfPath);
        using var document = new PdfDocument(pdfBytes);

        // 先创建两个单页文件用于合并
        string outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "output");
        string file1 = Path.Combine(outputDir, "page1.pdf");
        string file2 = Path.Combine(outputDir, "page2.pdf");

        PdfDocumentOperations.ExportPages(document, file1, new[] { 0 });
        PdfDocumentOperations.ExportPages(document, file2, new[] { 1 });

        // Act - 合并文件
        string mergedFile = Path.Combine(outputDir, "merged.pdf");
        Assert.DoesNotThrow(() =>
        {
            PdfDocumentOperations.MergeDocuments(mergedFile, new[] { file1, file2 });
        });

        // Assert - 合并文件应该存在且页数为 2
        Assert.That(File.Exists(mergedFile), Is.True);
        using var mergedDocument = new PdfDocument(File.ReadAllBytes(mergedFile));
        Assert.That(mergedDocument.PageCount, Is.EqualTo(2));
    }
}
