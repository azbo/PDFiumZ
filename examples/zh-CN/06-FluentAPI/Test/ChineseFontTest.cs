using PDFiumZ;
using PDFiumZ.HighLevel;
using System;

// 简单的中文字体测试程序
class ChineseFontTest
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== PDFiumZ 中文字体测试 ===\n");
        Console.WriteLine("注意: 当前 PDFiumZ 版本的 Fluent API 对自定义字体支持有限");
        Console.WriteLine("此测试演示了中文乱码问题和解决方案的需求\n");

        // 初始化 PDFium 库
        Console.WriteLine("正在初始化 PDFium 库...");
        PdfiumLibrary.Initialize();
        Console.WriteLine("✓ PDFium 库初始化成功\n");

        try
        {
            // 测试 1: 使用标准字体（不支持中文，会显示乱码）
            Console.WriteLine("测试 1: 使用标准字体（Helvetica）");
            using var doc1 = PdfDocument.CreateNew();
            using var page1 = doc1.CreatePage(595, 842);
            using var font1 = PdfFont.Load(doc1, PdfStandardFont.Helvetica);
            using var editor1 = page1.BeginEdit();

            editor1
                .WithFont(font1)
                .WithFontSize(14)
                .WithTextColor(PdfColor.Black)
                .Text("中文显示为乱码 - Chinese shows as garbage", 100, 750)
                .Text("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 100, 720)
                .Commit();

            doc1.Save("test-standard-font.pdf");
            Console.WriteLine("✓ 已保存: test-standard-font.pdf (中文会显示为乱码)\n");

            // 测试 2: 尝试加载中文字体（当前 API 限制）
            Console.WriteLine("测试 2: 尝试加载中文字体");
            try
            {
                using var doc2 = PdfDocument.CreateNew();
                using var page2 = doc2.CreatePage(595, 842);
                // 注意: 这会失败，因为 FPDFPageObjNewTextObj 不支持自定义字体
                using var font2 = PdfFont.Load(doc2, @"C:\Windows\Fonts\simhei.ttf", isCidFont: true);
                using var editor2 = page2.BeginEdit();

                editor2
                    .WithFont(font2)
                    .WithFontSize(14)
                    .Text("如果看到这段文字，说明中文字体支持已修复", 100, 750)
                    .Commit();

                doc2.Save("test-chinese-font.pdf");
                Console.WriteLine("✓ 中文字体测试成功！");
            }
            catch (PdfException pex)
            {
                Console.WriteLine($"⚠️ 预期的错误: {pex.Message}");
                Console.WriteLine("   原因: FPDFPageObjNewTextObj API 限制");
                Console.WriteLine("   解决方案: 需要修改底层实现使用 FPDFTextSetFont\n");
            }

            Console.WriteLine("=== 测试总结 ===");
            Console.WriteLine("当前问题:");
            Console.WriteLine("1. PDFium 的 FPDFPageObjNewTextObj 只支持 14 种标准字体");
            Console.WriteLine("2. 自定义字体（如中文字体）需要使用不同的 API 路径");
            Console.WriteLine("3. 需要修改 PdfContentEditor.AddTextInternal 方法");
            Console.WriteLine("\n建议的修复方向:");
            Console.WriteLine("• 使用 FPDFTextLoadFont 加载自定义字体");
            Console.WriteLine("• 创建文本对象后使用 FPDFTextObjSetFont 设置字体");
            Console.WriteLine("• 或使用 FPDFTextLoadCidType2Font 专门处理 CJK 字体");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n错误: {ex.Message}");
            Console.ResetColor();
        }
        finally
        {
            // 清理资源
            Console.WriteLine("\n正在清理资源...");
            PdfiumLibrary.Shutdown();
            Console.WriteLine("✓ PDFium 库已关闭");
        }
    }
}
