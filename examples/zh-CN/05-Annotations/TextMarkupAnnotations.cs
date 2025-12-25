using PDFiumZ.HighLevel;

/// <summary>
/// PDFiumZ 文本标记注解示例
/// 演示如何在 PDF 中创建高亮、下划线和删除线注解
/// </summary>
class TextMarkupAnnotationsExample
{
    static void Main(string[] args)
    {
        // ============================================
        // 步骤 1: 初始化 PDFium 库
        // ============================================
        Console.WriteLine("=== PDFiumZ 文本标记注解示例 ===\n");
        Console.WriteLine("步骤 1: 初始化 PDFium 库...");
        PdfiumLibrary.Initialize();
        Console.WriteLine("✓ PDFium 库初始化成功\n");

        try
        {
            var outputPath = "text-markup-annotations-demo.pdf";

            // ============================================
            // 步骤 2: 创建新的 PDF 文档
            // ============================================
            Console.WriteLine("步骤 2: 创建新的 PDF 文档...");
            using var document = PdfDocument.CreateNew();
            using var page = document.CreatePage(595, 842); // A4 尺寸
            Console.WriteLine("✓ 创建了一个 A4 尺寸的页面\n");

            // ============================================
            // 步骤 3: 创建高亮注解
            // ============================================
            Console.WriteLine("步骤 3: 创建高亮注解...");
            var highlightBounds = new PdfRectangle(50, 750, 300, 30);
            using var highlight = PdfHighlightAnnotation.Create(
                page,
                highlightBounds
                // 默认颜色: 0x80FFFF00 (黄色，50% 不透明度)
            );
            Console.WriteLine("✓ 创建了黄色高亮注解");
            Console.WriteLine($"  位置: X={highlightBounds.X}, Y={highlightBounds.Y}");
            Console.WriteLine($"  尺寸: {highlightBounds.Width}x{highlightBounds.Height}");
            Console.WriteLine($"  颜色: ARGB=0x{highlight.Color:X8}\n");

            // ============================================
            // 步骤 4: 创建自定义颜色的高亮
            // ============================================
            Console.WriteLine("步骤 4: 创建自定义颜色的高亮...");
            var customHighlightBounds = new PdfRectangle(50, 700, 300, 30);
            uint greenHighlight = 0x8000FF00; // 绿色，50% 不透明度
            using var customHighlight = PdfHighlightAnnotation.Create(
                page,
                customHighlightBounds,
                greenHighlight
            );
            Console.WriteLine("✓ 创建了绿色高亮注解");
            Console.WriteLine($"  颜色: ARGB=0x{greenHighlight:X8}\n");

            // ============================================
            // 步骤 5: 创建下划线注解
            // ============================================
            Console.WriteLine("步骤 5: 创建下划线注解...");
            var underlineBounds = new PdfRectangle(50, 650, 300, 30);
            using var underline = PdfUnderlineAnnotation.Create(
                page,
                underlineBounds
                // 默认颜色: 0xFFFF0000 (红色，完全不透明)
            );
            Console.WriteLine("✓ 创建了红色下划线注解");
            Console.WriteLine($"  位置: X={underlineBounds.X}, Y={underlineBounds.Y}");
            Console.WriteLine($"  颜色: ARGB=0x{underline.Color:X8}\n");

            // ============================================
            // 步骤 6: 创建自定义颜色的下划线
            // ============================================
            Console.WriteLine("步骤 6: 创建自定义颜色的下划线...");
            var customUnderlineBounds = new PdfRectangle(50, 600, 300, 30);
            uint blueUnderline = 0xFF0000FF; // 蓝色，完全不透明
            using var customUnderline = PdfUnderlineAnnotation.Create(
                page,
                customUnderlineBounds,
                blueUnderline
            );
            Console.WriteLine("✓ 创建了蓝色下划线注解");
            Console.WriteLine($"  颜色: ARGB=0x{blueUnderline:X8}\n");

            // ============================================
            // 步骤 7: 创建删除线注解
            // ============================================
            Console.WriteLine("步骤 7: 创建删除线注解...");
            var strikeoutBounds = new PdfRectangle(50, 550, 300, 30);
            using var strikeout = PdfStrikeOutAnnotation.Create(
                page,
                strikeoutBounds
                // 默认颜色: 0xFFFF0000 (红色，完全不透明)
            );
            Console.WriteLine("✓ 创建了红色删除线注解");
            Console.WriteLine($"  位置: X={strikeoutBounds.X}, Y={strikeoutBounds.Y}");
            Console.WriteLine($"  颜色: ARGB=0x{strikeout.Color:X8}\n");

            // ============================================
            // 步骤 8: 创建自定义颜色的删除线
            // ============================================
            Console.WriteLine("步骤 8: 创建自定义颜色的删除线...");
            var customStrikeoutBounds = new PdfRectangle(50, 500, 300, 30);
            uint blackStrikeout = 0xFF000000; // 黑色，完全不透明
            using var customStrikeout = PdfStrikeOutAnnotation.Create(
                page,
                customStrikeoutBounds,
                blackStrikeout
            );
            Console.WriteLine("✓ 创建了黑色删除线注解");
            Console.WriteLine($"  颜色: ARGB=0x{blackStrikeout:X8}\n");

            // ============================================
            // 步骤 9: 创建多个混合注解
            // ============================================
            Console.WriteLine("步骤 9: 创建多个混合注解...");

            using var highlight1 = PdfHighlightAnnotation.Create(
                page, new PdfRectangle(50, 450, 200, 30), 0x80FFFF00);
            using var underline1 = PdfUnderlineAnnotation.Create(
                page, new PdfRectangle(50, 400, 200, 30), 0xFFFF0000);
            using var strikeout1 = PdfStrikeOutAnnotation.Create(
                page, new PdfRectangle(50, 350, 200, 30), 0xFF000000);

            Console.WriteLine("✓ 创建了 3 个额外的注解\n");

            // ============================================
            // 步骤 10: 保存文档
            // ============================================
            Console.WriteLine("步骤 10: 保存 PDF 文档...");
            document.Save(outputPath);
            Console.WriteLine($"✓ 文档已保存到: {outputPath}\n");

            // ============================================
            // 步骤 11: 验证注解持久化
            // ============================================
            Console.WriteLine("步骤 11: 重新加载并验证注解...");
            using var loadedDoc = PdfDocument.Open(outputPath);
            using var loadedPage = loadedDoc.GetPage(0);

            var annotations = loadedPage.GetAnnotations().ToList();
            Console.WriteLine($"✓ 在加载的文档中发现 {annotations.Count} 个注解");

            var highlightCount = annotations.Count(a => a.Type == PdfAnnotationType.Highlight);
            var underlineCount = annotations.Count(a => a.Type == PdfAnnotationType.Underline);
            var strikeoutCount = annotations.Count(a => a.Type == PdfAnnotationType.StrikeOut);

            Console.WriteLine($"  - 高亮: {highlightCount}");
            Console.WriteLine($"  - 下划线: {underlineCount}");
            Console.WriteLine($"  - 删除线: {strikeoutCount}\n");

            // ============================================
            // 完成
            // ============================================
            Console.WriteLine("=== 示例完成 ===");
            Console.WriteLine($"\n您可以打开 '{outputPath}' 查看创建的文本标记注解。");
            Console.WriteLine("使用 PDF 查看器时，这些注解将高亮、下划线或删除线显示指定的区域。");
        }
        finally
        {
            // ============================================
            // 步骤 12: 清理资源
            // ============================================
            Console.WriteLine("\n清理资源...");
            PdfiumLibrary.Shutdown();
            Console.WriteLine("✓ PDFium 库已关闭");
        }
    }
}
