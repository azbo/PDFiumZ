using PDFiumZ.HighLevel;

/// <summary>
/// PDFiumZ 链接注解示例
/// 演示如何在 PDF 中创建可点击的链接注解
/// </summary>
class LinkAnnotationsExample
{
    static void Main(string[] args)
    {
        // ============================================
        // 步骤 1: 初始化 PDFium 库
        // ============================================
        Console.WriteLine("=== PDFiumZ 链接注解示例 ===\n");
        Console.WriteLine("步骤 1: 初始化 PDFium 库...");
        PdfiumLibrary.Initialize();
        Console.WriteLine("✓ PDFium 库初始化成功\n");

        try
        {
            var outputPath = "link-annotations-demo.pdf";

            // ============================================
            // 步骤 2: 创建新的 PDF 文档
            // ============================================
            Console.WriteLine("步骤 2: 创建新的 PDF 文档...");
            using var document = PdfDocument.CreateNew();
            using var page = document.CreatePage(595, 842); // A4 尺寸
            Console.WriteLine("✓ 创建了一个 A4 尺寸的页面\n");

            // ============================================
            // 步骤 3: 创建网站链接
            // ============================================
            Console.WriteLine("步骤 3: 在页面顶部创建网站链接...");
            var websiteBounds = new PdfRectangle(50, 750, 300, 30);
            using var websiteLink = PdfLinkAnnotation.CreateExternal(
                page,
                websiteBounds,
                "https://github.com/casbin-net/pdfiumz"
            );
            Console.WriteLine("✓ 创建了指向 GitHub 项目的链接");
            Console.WriteLine($"  位置: X={websiteBounds.X}, Y={websiteBounds.Y}");
            Console.WriteLine($"  尺寸: {websiteBounds.Width}x{websiteBounds.Height}\n");

            // ============================================
            // 步骤 4: 创建邮箱链接
            // ============================================
            Console.WriteLine("步骤 4: 创建邮箱链接...");
            var emailBounds = new PdfRectangle(50, 700, 300, 30);
            using var emailLink = PdfLinkAnnotation.CreateExternal(
                page,
                emailBounds,
                "mailto:support@example.com"
            );
            Console.WriteLine("✓ 创建了邮箱链接 (mailto:support@example.com)\n");

            // ============================================
            // 步骤 5: 创建自定义颜色的链接
            // ============================================
            Console.WriteLine("步骤 5: 创建自定义颜色的链接...");
            var customColorBounds = new PdfRectangle(50, 650, 300, 30);
            uint redColor = 0xFF0000FF; // 不透明红色
            using var customColorLink = PdfLinkAnnotation.CreateExternal(
                page,
                customColorBounds,
                "https://example.com/custom",
                redColor
            );
            Console.WriteLine("✓ 创建了红色边框的链接");
            Console.WriteLine($"  颜色: ARGB=0x{redColor:X8}\n");

            // ============================================
            // 步骤 6: 创建多个链接
            // ============================================
            Console.WriteLine("步骤 6: 创建更多链接...");
            var link1Bounds = new PdfRectangle(50, 600, 200, 30);
            var link2Bounds = new PdfRectangle(50, 550, 200, 30);
            var link3Bounds = new PdfRectangle(50, 500, 200, 30);

            using var link1 = PdfLinkAnnotation.CreateExternal(
                page, link1Bounds, "https://www.google.com");
            using var link2 = PdfLinkAnnotation.CreateExternal(
                page, link2Bounds, "https://www.microsoft.com");
            using var link3 = PdfLinkAnnotation.CreateExternal(
                page, link3Bounds, "https://www.github.com");

            Console.WriteLine("✓ 创建了 3 个额外的链接");
            Console.WriteLine("  - Google");
            Console.WriteLine("  - Microsoft");
            Console.WriteLine("  - GitHub\n");

            // ============================================
            // 步骤 7: 保存文档
            // ============================================
            Console.WriteLine("步骤 7: 保存 PDF 文档...");
            document.Save(outputPath);
            Console.WriteLine($"✓ 文档已保存到: {outputPath}\n");

            // ============================================
            // 步骤 8: 验证链接持久化
            // ============================================
            Console.WriteLine("步骤 8: 重新加载并验证链接...");
            using var loadedDoc = PdfDocument.Open(outputPath);
            using var loadedPage = loadedDoc.GetPage(0);

            var annotations = loadedPage.GetAnnotations().ToList();
            Console.WriteLine($"✓ 在加载的文档中发现 {annotations.Count} 个注解");

            var linkAnnotations = annotations.Where(a => a.Type == PdfAnnotationType.Link).ToList();
            Console.WriteLine($"✓ 其中 {linkAnnotations.Count} 个是链接注解\n");

            // ============================================
            // 完成
            // ============================================
            Console.WriteLine("=== 示例完成 ===");
            Console.WriteLine($"\n您可以打开 '{outputPath}' 查看创建的链接注解。");
            Console.WriteLine("使用 PDF 查看器时，点击链接将打开对应的网站或邮件客户端。");
        }
        finally
        {
            // ============================================
            // 步骤 9: 清理资源
            // ============================================
            Console.WriteLine("\n清理资源...");
            PdfiumLibrary.Shutdown();
            Console.WriteLine("✓ PDFium 库已关闭");
        }
    }
}
