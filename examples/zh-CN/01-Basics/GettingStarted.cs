using PDFiumZ;
using PDFiumZ.HighLevel;
using System;

namespace GettingStartedExample;

/// <summary>
/// PDFiumZ 快速入门示例
/// 演示 PDFiumZ 的基础操作
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        // ============================================
        // 步骤 1: 初始化 PDFium 库
        // ============================================
        Console.WriteLine("=== PDFiumZ 快速入门示例 ===\n");
        Console.WriteLine("步骤 1: 初始化 PDFium 库...");
        PdfiumLibrary.Initialize();
        Console.WriteLine("✓ PDFium 库初始化成功\n");

        try
        {
            // ============================================
            // 步骤 2: 创建一个简单的 PDF 文档
            // ============================================
            Console.WriteLine("步骤 2: 创建新文档...");
            using var document = PdfDocument.CreateNew();

            // 添加 A4 页面
            using var page = document.CreatePage(PdfPageSize.A4);
            Console.WriteLine($"✓ 创建了 {document.PageCount} 个 A4 页面");

            // 在页面上添加文本
            using var font = PdfFont.Load(document, PdfStandardFont.HelveticaBold);
            using var editor = page.BeginEdit();

            editor
                .WithFont(font)
                .WithFontSize(24)
                .WithTextColor(PdfColor.DarkBlue)
                .Text("欢迎使用 PDFiumZ", 100, 700)

                .WithFontSize(14)
                .WithTextColor(PdfColor.Black)
                .Text("这是您的第一个 PDF 文档", 100, 650)
                .Text($"创建时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", 100, 630)

                // 绘制矩形
                .WithStrokeColor(PdfColor.Red)
                .WithLineWidth(2)
                .Rectangle(new PdfRectangle(100, 550, 400, 50))

                // 绘制圆形
                .WithFillColor(PdfColor.LightBlue)
                .Circle(300, 450, 30)
                .Commit();

            Console.WriteLine("✓ 已添加文本和图形\n");

            // ============================================
            // 步骤 3: 保存文档
            // ============================================
            Console.WriteLine("步骤 3: 保存文档...");
            string outputPath = "getting-started-output.pdf";
            document.Save(outputPath);
            Console.WriteLine($"✓ 文档已保存到: {outputPath}\n");

            // ============================================
            // 步骤 4: 读取文档信息
            // ============================================
            Console.WriteLine("步骤 4: 读取文档信息...");

            // 重新打开保存的文档
            using var openedDoc = PdfDocument.Open(outputPath);
            Console.WriteLine($"  文档页数: {openedDoc.PageCount}");

            // 读取第一页
            using var firstPage = openedDoc.GetPage(0);
            Console.WriteLine($"  第 1 页尺寸: {firstPage.Width:F2} x {firstPage.Height:F2} 点");
            Console.WriteLine($"  第 1 页旋转: {firstPage.Rotation}");

            // 提取文本
            var text = firstPage.ExtractText();
            Console.WriteLine($"  提取的文本长度: {text.Length} 字符");

            // 读取元数据（如果有）
            var metadata = openedDoc.Metadata;
            if (metadata != null)
            {
                // 元数据可用
                Console.WriteLine($"  元数据: 已加载");
            }

            Console.WriteLine("✓ 文档信息读取完成\n");

            // ============================================
            // 步骤 5: 渲染页面为图像
            // ============================================
            Console.WriteLine("步骤 5: 渲染页面为图像...");

            using var image = firstPage.RenderToImage(RenderOptions.Default.WithDpi(150));
            string imagePath = "getting-started-output.png";
            image.SaveAsPng(imagePath);
            Console.WriteLine($"✓ 页面已渲染为图像: {imagePath}");
            Console.WriteLine($"  图像尺寸: {image.Width} x {image.Height} 像素\n");

            // ============================================
            // 步骤 6: 页面操作
            // ============================================
            Console.WriteLine("步骤 6: 页面操作示例...");

            // 添加第二页
            using var page2 = document.CreatePage(PdfPageSize.A4);
            using var editor2 = page2.BeginEdit();
            editor2
                .WithFont(font)
                .WithFontSize(18)
                .WithTextColor(PdfColor.Black)
                .Text("这是第二页", 100, 700)
                .Commit();

            Console.WriteLine($"✓ 文档现在有 {document.PageCount} 页");

            // 旋转第二页
            page2.Rotation = PdfRotation.Rotate90;
            Console.WriteLine($"✓ 第 2 页已旋转 90 度\n");

            // 保存修改后的文档
            string modifiedPath = "getting-started-modified.pdf";
            document.Save(modifiedPath);
            Console.WriteLine($"✓ 修改后的文档已保存: {modifiedPath}\n");

            // ============================================
            // 总结
            // ============================================
            Console.WriteLine("=== 示例完成 ===");
            Console.WriteLine("\n您学会了:");
            Console.WriteLine("  ✓ 如何初始化和清理 PDFium 库");
            Console.WriteLine("  ✓ 如何创建新的 PDF 文档");
            Console.WriteLine("  ✓ 如何添加页面和内容");
            Console.WriteLine("  ✓ 如何保存和打开文档");
            Console.WriteLine("  ✓ 如何读取文档信息");
            Console.WriteLine("  ✓ 如何渲染页面为图像");
            Console.WriteLine("  ✓ 如何操作页面（添加、旋转）");
            Console.WriteLine("\n生成的文件:");
            Console.WriteLine($"  - {outputPath}");
            Console.WriteLine($"  - {imagePath}");
            Console.WriteLine($"  - {modifiedPath}");
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
            // ============================================
            // 清理资源
            // ============================================
            Console.WriteLine("\n步骤 7: 清理资源...");
            PdfiumLibrary.Shutdown();
            Console.WriteLine("✓ PDFium 库已关闭");
        }
    }
}
