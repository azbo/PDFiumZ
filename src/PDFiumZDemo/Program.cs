using PDFiumZ.HighLevel;
using System.Drawing;

namespace PDFiumZDemo;

class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("=== PDFium.Z 高层 API 示例 ===\n");

        // 示例 1: 基础用法 - 获取文档信息
        BasicExample();

        System.Console.WriteLine();

        // 示例 2: 生成单页图像
        SinglePageExample();

        System.Console.WriteLine();

        // 示例 3: 生成所有页面图像（字节数组）
        AllPagesAsBytesExample();

        System.Console.WriteLine();

        // 示例 4: 生成并保存所有页面
        AllPagesSaveExample();

        System.Console.WriteLine();

        // 示例 5: 自定义设置生成图像
        CustomSettingsExample();

        System.Console.WriteLine();

        // 示例 6: 保存到目录
        SaveToDirectoryExample();
    }

    /// <summary>
    /// 示例 1: 基础用法 - 获取文档信息
    /// </summary>
    static void BasicExample()
    {
        System.Console.WriteLine("--- 示例 1: 基础用法 ---");
        using var document = new PdfDocument("pdf-sample.pdf");

        System.Console.WriteLine($"文档页数: {document.PageCount}");

        foreach (var page in document.Pages)
        {
            System.Console.WriteLine($"  页面 {page.Index}: {page.Width:F2} x {page.Height:F2} 点");
        }
    }

    /// <summary>
    /// 示例 2: 生成单页图像
    /// </summary>
    static void SinglePageExample()
    {
        System.Console.WriteLine("--- 示例 2: 单页图像 ---");
        using var document = new PdfDocument("pdf-sample.pdf");
        using var page = document[0];

        // 保存为 PNG
        page.SaveAsImage("output-single.png");
        System.Console.WriteLine($"已保存: output-single.png ({page.Width:F2} x {page.Height:F2})");
    }

    /// <summary>
    /// 示例 3: 生成所有页面图像（返回字节数组）
    /// </summary>
    static void AllPagesAsBytesExample()
    {
        System.Console.WriteLine("--- 示例 3: 所有页面为字节数组 ---");
        using var document = new PdfDocument("pdf-sample.pdf");

        System.Collections.Generic.IEnumerable<byte[]> images = document.GenerateImages();

        int count = 0;
        foreach (var imageBytes in images)
        {
            System.Console.WriteLine($"  页面 {count}: {imageBytes.Length:N0} 字节");
            count++;
        }
    }

    /// <summary>
    /// 示例 4: 生成并保存所有页面
    /// </summary>
    static void AllPagesSaveExample()
    {
        System.Console.WriteLine("--- 示例 4: 保存所有页面 ---");
        using var document = new PdfDocument("pdf-sample.pdf");

        // 方式 1: 使用回调函数命名
        document.GenerateImages(imageIndex => $"page{imageIndex}.png");
        System.Console.WriteLine("已保存: page0.png, page1.png, ...");

        // 方式 2: 使用自定义命名
        document.GenerateImages(imageIndex => $"document_page_{imageIndex:D3}.png");
        System.Console.WriteLine("已保存: document_page_000.png, document_page_001.png, ...");
    }

    /// <summary>
    /// 示例 5: 自定义设置生成图像
    /// </summary>
    static void CustomSettingsExample()
    {
        System.Console.WriteLine("--- 示例 5: 自定义设置 ---");
        using var document = new PdfDocument("pdf-sample.pdf");

        var settings = new ImageGenerationSettings
        {
            ImageFormat = ImageFormat.Png,
            ImageCompressionQuality = ImageCompressionQuality.High,
            RasterDpi = 300
        };

        System.Collections.Generic.IEnumerable<byte[]> images = document.GenerateImages(settings);
        System.Console.WriteLine($"使用 DPI {settings.RasterDpi} 生成了 {document.PageCount} 个图像");
    }

    /// <summary>
    /// 示例 6: 保存到目录
    /// </summary>
    static void SaveToDirectoryExample()
    {
        System.Console.WriteLine("--- 示例 6: 保存到目录 ---");
        using var document = new PdfDocument("pdf-sample.pdf");

        string outputDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "output");
        if (!System.IO.Directory.Exists(outputDir))
            System.IO.Directory.CreateDirectory(outputDir);

        document.GenerateImagesToDirectory(outputDir, "pdf-page");
        System.Console.WriteLine($"已保存所有页面到: {outputDir}");
        System.Console.WriteLine("文件: pdf-page0.png, pdf-page1.png, ...");
    }
}
