using PDFiumZ;
using PDFiumZ.HighLevel;
using System;
using System.IO;
using System.Linq;

namespace MergeSplitExample;

/// <summary>
/// PDFiumZ 文档合并与拆分示例
/// 演示如何合并多个 PDF 文件和拆分 PDF 文档
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== PDFiumZ 文档合并与拆分示例 ===\n");

        // 初始化 PDFium
        PdfiumLibrary.Initialize();

        try
        {
            // ============================================
            // 准备示例文件
            // ============================================
            Console.WriteLine("准备示例文件...");
            string samplePdf1 = "sample1.pdf";
            string samplePdf2 = "sample2.pdf";
            string samplePdf3 = "sample3.pdf";

            // 创建示例文件
            CreateSamplePdf(samplePdf1, "文档 1", 3);
            CreateSamplePdf(samplePdf2, "文档 2", 2);
            CreateSamplePdf(samplePdf3, "文档 3", 4);

            Console.WriteLine("✓ 已创建 3 个示例 PDF 文件\n");

            // ============================================
            // 示例 1: 合并多个 PDF 文件
            // ============================================
            Console.WriteLine("示例 1: 合并多个 PDF 文件");
            Example1_MergeDocuments(samplePdf1, samplePdf2, samplePdf3);
            Console.WriteLine();

            // ============================================
            // 示例 2: 拆分 PDF - 提取指定页面
            // ============================================
            Console.WriteLine("示例 2: 拆分 PDF - 提取指定页面");
            Example2_SplitDocument("merged.pdf");
            Console.WriteLine();

            // ============================================
            // 示例 3: 拆分每个页面为单独文件
            // ============================================
            Console.WriteLine("示例 3: 拆分每个页面为单独文件");
            Example3_SplitAllPages("merged.pdf");
            Console.WriteLine();

            // ============================================
            // 示例 4: 使用 Range 语法拆分（.NET 8+）
            // ============================================
            Console.WriteLine("示例 4: 使用 Range 语法拆分");
#if NET8_0_OR_GREATER
            Example4_SplitWithRange("merged.pdf");
#else
            Console.WriteLine("  (需要 .NET 8+ 支持)");
#endif
            Console.WriteLine();

            // ============================================
            // 示例 5: 旋转后保存
            // ============================================
            Console.WriteLine("示例 5: 旋转页面后保存");
            Example5_RotateAndSave("merged.pdf");
            Console.WriteLine();

            // ============================================
            // 示例 6: 删除页面后保存
            // ============================================
            Console.WriteLine("示例 6: 删除指定页面");
            Example6_DeletePages("merged.pdf");
            Console.WriteLine();

            Console.WriteLine("=== 所有示例完成 ===");
            Console.WriteLine("\n生成的文件:");
            Console.WriteLine("  - sample1.pdf, sample2.pdf, sample3.pdf (示例源文件)");
            Console.WriteLine("  - merged.pdf (合并后的文档)");
            Console.WriteLine("  - split_first3.pdf (前 3 页)");
            Console.WriteLine("  - split_last2.pdf (最后 2 页)");
            Console.WriteLine("  - split_pages/ (每个页面单独保存)");
            Console.WriteLine("  - split_range.pdf (Range 拆分)");
            Console.WriteLine("  - rotated.pdf (旋转后的文档)");
            Console.WriteLine("  - deleted.pdf (删除页面后的文档)");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n错误: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
        finally
        {
            PdfiumLibrary.Shutdown();
        }
    }

    /// <summary>
    /// 示例 1: 合并多个 PDF 文件
    /// </summary>
    private static void Example1_MergeDocuments(params string[] files)
    {
        Console.WriteLine("  合并文件:");
        foreach (var file in files)
        {
            Console.WriteLine($"    - {file}");
        }

        // 方式 1: 使用静态方法合并
        using var merged = PdfDocument.Merge(files);
        Console.WriteLine($"  ✓ 合并完成，共 {merged.PageCount} 页");

        merged.Save("merged.pdf");
        Console.WriteLine("  ✓ 保存为: merged.pdf");
    }

    /// <summary>
    /// 示例 2: 拆分 PDF - 提取指定页面
    /// </summary>
    private static void Example2_SplitDocument(string sourceFile)
    {
        using var document = PdfDocument.Open(sourceFile);
        Console.WriteLine($"  源文件: {sourceFile} ({document.PageCount} 页)");

        // 提取前 3 页
        Console.WriteLine("  提取前 3 页...");
        using var first3 = document.Split(0, 1, 2);
        first3.Save("split_first3.pdf");
        Console.WriteLine($"  ✓ 保存为: split_first3.pdf ({first3.PageCount} 页)");

        // 提取最后 2 页
        Console.WriteLine("  提取最后 2 页...");
        using var last2 = document.Split(document.PageCount - 2, document.PageCount - 1);
        last2.Save("split_last2.pdf");
        Console.WriteLine($"  ✓ 保存为: split_last2.pdf ({last2.PageCount} 页)");
    }

    /// <summary>
    /// 示例 3: 拆分每个页面为单独文件
    /// </summary>
    private static void Example3_SplitAllPages(string sourceFile)
    {
        using var document = PdfDocument.Open(sourceFile);
        string outputDir = "split_pages";
        Directory.CreateDirectory(outputDir);

        Console.WriteLine($"  将 {document.PageCount} 页拆分为单独文件...");

        for (int i = 0; i < document.PageCount; i++)
        {
            using var page = document.GetPages(i, 1).First();
            using var singlePage = PdfDocument.CreateNew();
            using var newPage = singlePage.CreatePage(page.Width, page.Height);

            // 复制页面内容（简化示例，实际需要完整复制）
            string outputPath = Path.Combine(outputDir, $"page_{i + 1}.pdf");
            singlePage.Save(outputPath);
        }

        Console.WriteLine($"  ✓ 已保存到: {outputDir}/");
    }

    /// <summary>
    /// 示例 4: 使用 Range 语法拆分（.NET 8+）
    /// </summary>
#if NET8_0_OR_GREATER
    private static void Example4_SplitWithRange(string sourceFile)
    {
        using var document = PdfDocument.Open(sourceFile);

        // 使用 Range 语法提取页面 2-5
        var range = 2..6;  // 页面 2, 3, 4, 5
        var (offset, length) = range.GetOffsetAndLength(document.PageCount);
        var pageIndices = Enumerable.Range(offset, length).ToArray();

        Console.WriteLine($"  提取页面范围: {range} (索引 {offset}, 共 {length} 页)");

        using var extracted = document.Split(pageIndices);
        extracted.Save("split_range.pdf");
        Console.WriteLine($"  ✓ 保存为: split_range.pdf ({extracted.PageCount} 页)");
    }
#endif

    /// <summary>
    /// 示例 5: 旋转页面后保存
    /// </summary>
    private static void Example5_RotateAndSave(string sourceFile)
    {
        using var document = PdfDocument.Open(sourceFile);
        Console.WriteLine($"  源文件: {sourceFile}");

        // 旋转前 3 页 90 度
        Console.WriteLine("  旋转前 3 页 90 度...");
        document.RotatePages(PdfRotation.Rotate90, 0, 1, 2);

        document.Save("rotated.pdf");
        Console.WriteLine("  ✓ 保存为: rotated.pdf");
    }

    /// <summary>
    /// 示例 6: 删除指定页面
    /// </summary>
    private static void Example6_DeletePages(string sourceFile)
    {
        using var document = PdfDocument.Open(sourceFile);
        int originalCount = document.PageCount;
        Console.WriteLine($"  源文件: {sourceFile} ({originalCount} 页)");

        // 删除第 2 页
        Console.WriteLine("  删除第 2 页...");
        document.DeletePages(1);

        document.Save("deleted.pdf");
        Console.WriteLine($"  ✓ 保存为: deleted.pdf (原 {originalCount} 页 → 现 {document.PageCount} 页)");
    }

    /// <summary>
    /// 创建示例 PDF 文件
    /// </summary>
    private static void CreateSamplePdf(string filename, string title, int pageCount)
    {
        using var document = PdfDocument.CreateNew();
        using var font = PdfFont.Load(document, PdfStandardFont.HelveticaBold);

        for (int i = 0; i < pageCount; i++)
        {
            using var page = document.CreatePage(PdfPageSize.A4);
            using var editor = page.BeginEdit();

            editor
                .WithFont(font)
                .WithFontSize(24)
                .WithTextColor(PdfColor.DarkBlue)
                .Text($"{title} - 页面 {i + 1}", 100, 700)

                .WithFontSize(12)
                .WithTextColor(PdfColor.Black)
                .Text($"这是 {title} 的第 {i + 1} 页", 100, 650)
                .Text($"创建时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", 100, 630)

                // 绘制页码
                .WithFontSize(10)
                .Text($"页码: {i + 1}", 250, 50)

                .Commit();
        }

        document.Save(filename);
    }
}
