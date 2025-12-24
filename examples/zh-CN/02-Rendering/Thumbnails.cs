using PDFiumZ;
using PDFiumZ.HighLevel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ThumbnailsExample;

/// <summary>
/// PDFiumZ 缩略图生成示例
/// 演示如何为 PDF 文档生成各种尺寸和质量的缩略图
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== PDFiumZ 缩略图生成示例 ===\n");

        // 检查是否有 PDF 文件
        const string samplePdf = "sample.pdf";
        if (!File.Exists(samplePdf))
        {
            Console.WriteLine($"错误: 找不到示例 PDF 文件 '{samplePdf}'");
            Console.WriteLine("\n请将一个 PDF 文件重命名为 'sample.pdf' 并放在当前目录下，");
            Console.WriteLine("或者修改代码中的文件名。");
            return;
        }

        // 初始化 PDFium
        PdfiumLibrary.Initialize();

        try
        {
            using var document = PdfDocument.Open(samplePdf);
            Console.WriteLine($"已打开文档: {samplePdf}");
            Console.WriteLine($"总页数: {document.PageCount}\n");

            // ============================================
            // 示例 1: 生成默认缩略图
            // ============================================
            Console.WriteLine("示例 1: 生成默认缩略图 (200px, 中等质量)");
            await Example1_DefaultThumbnails(document);
            Console.WriteLine();

            // ============================================
            // 示例 2: 生成高质量缩略图
            // ============================================
            Console.WriteLine("示例 2: 生成高质量缩略图 (400px)");
            await Example2_HighQualityThumbnails(document);
            Console.WriteLine();

            // ============================================
            // 示例 3: 生成快速预览缩略图
            // ============================================
            Console.WriteLine("示例 3: 生成快速预览缩略图 (150px, 低质量)");
            await Example3_FastPreviews(document);
            Console.WriteLine();

            // ============================================
            // 示例 4: 为指定页面生成缩略图
            // ============================================
            Console.WriteLine("示例 4: 为指定页面生成缩略图");
            await Example4_SelectedPagesThumbnails(document);
            Console.WriteLine();

            // ============================================
            // 示例 5: 批量生成多种规格
            // ============================================
            Console.WriteLine("示例 5: 批量生成多种规格缩略图");
            await Example5_MultipleSizes(document);
            Console.WriteLine();

            // ============================================
            // 示例 6: 使用 ThumbnailOptions 进行精细控制
            // ============================================
            Console.WriteLine("示例 6: 使用 ThumbnailOptions 高级配置");
            await Example6_AdvancedOptions(document);
            Console.WriteLine();

            Console.WriteLine("=== 所有缩略图生成完成 ===");
            Console.WriteLine("\n生成的目录:");
            Console.WriteLine("  - thumbnails_default/  (默认缩略图)");
            Console.WriteLine("  - thumbnails_hq/       (高质量)");
            Console.WriteLine("  - thumbnails_fast/     (快速预览)");
            Console.WriteLine("  - thumbnails_selected/  (指定页面)");
            Console.WriteLine("  - thumbnails_multi/    (多种规格)");
            Console.WriteLine("  - thumbnails_custom/    (自定义配置)");
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
    /// 示例 1: 生成默认缩略图
    /// </summary>
    private static async Task Example1_DefaultThumbnails(PdfDocument document)
    {
        string outputDir = "thumbnails_default";
        Directory.CreateDirectory(outputDir);

        // 使用默认 ThumbnailOptions
        var options = ThumbnailOptions.Default;

        int pageIndex = 0;
        await Task.Run(() =>
        {
            foreach (var thumb in document.GenerateThumbnails(options))
            {
                using (thumb)
                {
                    string path = Path.Combine(outputDir, $"thumb_{pageIndex:D3}.png");
                    thumb.SaveAsPng(path);
                    Console.WriteLine($"  生成: {Path.GetFileName(path)} ({thumb.Width}x{thumb.Height})");
                    pageIndex++;
                }
            }
        });

        Console.WriteLine($"  ✓ 共生成 {pageIndex} 个默认缩略图");
    }

    /// <summary>
    /// 示例 2: 生成高质量缩略图
    /// </summary>
    private static async Task Example2_HighQualityThumbnails(PdfDocument document)
    {
        string outputDir = "thumbnails_hq";
        Directory.CreateDirectory(outputDir);

        // 大尺寸 + 高质量
        var options = new ThumbnailOptions
        {
            MaxWidth = 400,
            Quality = 2  // 0=低, 1=中, 2=高
        };

        int pageIndex = 0;
        await Task.Run(() =>
        {
            foreach (var thumb in document.GenerateThumbnails(options))
            {
                using (thumb)
                {
                    string path = Path.Combine(outputDir, $"thumb_hq_{pageIndex:D3}.png");
                    thumb.SaveAsPng(path);
                    pageIndex++;
                }
            }
        });

        Console.WriteLine($"  ✓ 共生成 {pageIndex} 个高质量缩略图 (400px, 最高质量)");
    }

    /// <summary>
    /// 示例 3: 生成快速预览缩略图
    /// </summary>
    private static async Task Example3_FastPreviews(PdfDocument document)
    {
        string outputDir = "thumbnails_fast";
        Directory.CreateDirectory(outputDir);

        // 小尺寸 + 低质量（最快）
        var options = ThumbnailOptions.Default
            .WithMaxWidth(150)
            .WithLowQuality();

        int pageIndex = 0;
        await Task.Run(() =>
        {
            foreach (var thumb in document.GenerateThumbnails(options))
            {
                using (thumb)
                {
                    string path = Path.Combine(outputDir, $"preview_{pageIndex:D3}.png");
                    thumb.SaveAsPng(path);
                    pageIndex++;
                }
            }
        });

        Console.WriteLine($"  ✓ 共生成 {pageIndex} 个快速预览 (150px, 低质量)");
    }

    /// <summary>
    /// 示例 4: 为指定页面生成缩略图
    /// </summary>
    private static async Task Example4_SelectedPagesThumbnails(PdfDocument document)
    {
        string outputDir = "thumbnails_selected";
        Directory.CreateDirectory(outputDir);

        // 只为前 5 页和最后一页生成缩略图
        var selectedPages = Enumerable.Range(0, Math.Min(5, document.PageCount)).ToList();
        if (document.PageCount > 5)
        {
            selectedPages.Add(document.PageCount - 1);
        }

        var options = new ThumbnailOptions
        {
            MaxWidth = 250,
            Quality = 1,
            PageIndices = selectedPages.ToArray()
        };

        int count = 0;
        await Task.Run(() =>
        {
            foreach (var thumb in document.GenerateThumbnails(options))
            {
                using (thumb)
                {
                    string path = Path.Combine(outputDir, $"thumb_page_{selectedPages[count]}.png");
                    thumb.SaveAsPng(path);
                    Console.WriteLine($"  生成: thumb_page_{selectedPages[count]}.png");
                    count++;
                }
            }
        });

        Console.WriteLine($"  ✓ 共生成 {count} 个指定页面的缩略图");
    }

    /// <summary>
    /// 示例 5: 批量生成多种规格
    /// </summary>
    private static async Task Example5_MultipleSizes(PdfDocument document)
    {
        string baseDir = "thumbnails_multi";
        Directory.CreateDirectory(baseDir);

        // 定义多种规格
        var sizes = new[]
        {
            (name: "small", width: 100),
            (name: "medium", width: 200),
            (name: "large", width: 400)
        };

        await Task.Run(() =>
        {
            foreach (var (name, width) in sizes)
            {
                string sizeDir = Path.Combine(baseDir, name);
                Directory.CreateDirectory(sizeDir);

                var options = new ThumbnailOptions
                {
                    MaxWidth = width,
                    Quality = 1
                };

                int pageIndex = 0;
                foreach (var thumb in document.GenerateThumbnails(options))
                {
                    using (thumb)
                    {
                        string path = Path.Combine(sizeDir, $"thumb_{pageIndex:D3}.png");
                        thumb.SaveAsPng(path);
                        pageIndex++;
                    }
                }

                Console.WriteLine($"  生成 {name} 规格 ({width}px): {pageIndex} 个文件");
            }
        });

        Console.WriteLine($"  ✓ 共生成 3 种规格的缩略图");
    }

    /// <summary>
    /// 示例 6: 使用 ThumbnailOptions 高级配置
    /// </summary>
    private static async Task Example6_AdvancedOptions(PdfDocument document)
    {
        string outputDir = "thumbnails_custom";
        Directory.CreateDirectory(outputDir);

        // 使用流畅 API 配置
        var options = ThumbnailOptions.Default
            .WithMaxWidth(300)
            .WithMediumQuality()
            .WithPages(new[] { 0, 1, document.PageCount - 1 });  // 首尾页

        Console.WriteLine("  配置:");
        Console.WriteLine($"    - 最大宽度: 300px");
        Console.WriteLine($"    - 质量: 中等");
        Console.WriteLine($"    - 页面: 首页、第二页、末页");

        int pageIndex = 0;
        await Task.Run(() =>
        {
            foreach (var thumb in document.GenerateThumbnails(options))
            {
                using (thumb)
                {
                    string path = Path.Combine(outputDir, $"custom_{pageIndex:D3}.png");
                    thumb.SaveAsPng(path);
                    Console.WriteLine($"  生成: {Path.GetFileName(path)} ({thumb.Width}x{thumb.Height})");
                    pageIndex++;
                }
            }
        });

        Console.WriteLine($"  ✓ 使用 ThumbnailOptions 生成 {pageIndex} 个自定义缩略图");
    }
}
