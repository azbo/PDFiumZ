using PDFiumZ;
using PDFiumZ.HighLevel;
using System;

class ChineseFontVerify
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== PDFiumZ 中文字体支持验证 ===\n");

        // 初始化 PDFium 库
        Console.WriteLine("正在初始化 PDFium 库...");
        PdfiumLibrary.Initialize();
        Console.WriteLine("✓ PDFium 库初始化成功\n");

        try
        {
            // 测试 1: 使用标准字体（中文会显示为乱码）
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("测试 1: 标准字体（不支持中文）");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            using var doc1 = PdfDocument.CreateNew();
            using var page1 = doc1.CreatePage(595, 842);
            using var font1 = PdfFont.Load(doc1, PdfStandardFont.Helvetica);
            using var editor1 = page1.BeginEdit();

            editor1
                .WithFont(font1)
                .WithFontSize(16)
                .WithTextColor(PdfColor.Black)
                .Text("标准字体: 中文显示为乱码 Standard Font", 100, 750)
                .Text("ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz", 100, 720)
                .WithFontSize(12)
                .WithTextColor(PdfColor.DarkRed)
                .Text("⚠️ 标准字体只支持 ASCII 字符，不支持中文", 100, 690)
                .Commit();

            doc1.Save("test-standard-font.pdf");
            Console.WriteLine("✓ 已保存: test-standard-font.pdf");
            Console.WriteLine("  预期: 中文显示为乱码或方框\n");

            // 测试 2: 使用中文字体
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("测试 2: 中文字体（SimHei 黑体）");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // 查找可用的中文字体
            string[] chineseFonts = new[]
            {
                @"C:\Windows\Fonts\simhei.ttf",    // 黑体
                @"C:\Windows\Fonts\simsun.ttc",    // 宋体 (TTC)
                @"C:\Windows\Fonts\msyh.ttc",      // 微软雅黑 (TTC)
            };

            string fontPath = null;
            foreach (var path in chineseFonts)
            {
                if (System.IO.File.Exists(path))
                {
                    fontPath = path;
                    Console.WriteLine($"✓ 找到字体: {System.IO.Path.GetFileName(path)}");
                    break;
                }
            }

            if (fontPath == null)
            {
                Console.WriteLine("✗ 未找到中文字体文件");
                Console.WriteLine("  请确保系统已安装中文字体");
                return;
            }

            using var doc2 = PdfDocument.CreateNew();
            using var page2 = doc2.CreatePage(595, 842);

            Console.WriteLine($"正在加载字体: {fontPath}");
            using var font2 = PdfFont.Load(doc2, fontPath, isCidFont: true);
            Console.WriteLine($"✓ 字体加载成功 (IsCustomFont: {font2.IsCustomFont})");

            using var editor2 = page2.BeginEdit();

            Console.WriteLine("正在添加中文文本...");
            editor2
                .WithFont(font2)
                .WithFontSize(24)
                .WithTextColor(PdfColor.DarkBlue)
                .Text("欢迎使用 PDFiumZ！", 100, 750)

                .WithFontSize(18)
                .WithTextColor(PdfColor.Black)
                .Text("中文字体测试成功", 100, 710)

                .WithFontSize(14)
                .Text("支持简体中文和繁体字", 100, 680)

                .WithFontSize(12)
                .WithTextColor(PdfColor.DarkGreen)
                .Text("特性列表：", 100, 640)
                .Text("• 完整的 CJK 字符支持", 120, 620)
                .Text("• 支持简体中文（你好）", 120, 605)
                .Text("• 支持繁體中文（你好）", 120, 590)
                .Text("• 支持日文（こんにちは）", 120, 575)
                .Text("• 支持韩文（안녕하세요）", 120, 560)
                .Text("• 完整的 Unicode 支持", 120, 545)

                .WithFontSize(14)
                .WithTextColor(PdfColor.DarkRed)
                .Text("混合文本：中文 English 日本語 한국어", 100, 500)

                .WithFontSize(12)
                .WithTextColor(PdfColor.Black)
                .Text("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 100, 470)
                .Text("abcdefghijklmnopqrstuvwxyz", 100, 455)
                .Text("0123456789 !@#$%^&*()", 100, 440)

                .Commit();

            doc2.Save("test-chinese-font.pdf");
            Console.WriteLine("✓ 已保存: test-chinese-font.pdf\n");

            // 测试 3: 混合使用标准字体和中文字体
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("测试 3: 混合使用标准字体和中文字体");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            using var doc3 = PdfDocument.CreateNew();
            using var page3 = doc3.CreatePage(595, 842);
            using var stdFont = PdfFont.Load(doc3, PdfStandardFont.HelveticaBold);
            using var chineseFont = PdfFont.Load(doc3, fontPath, isCidFont: true);
            using var editor3 = page3.BeginEdit();

            editor3
                // 标题用中文
                .WithFont(chineseFont)
                .WithFontSize(20)
                .WithTextColor(PdfColor.DarkBlue)
                .Text("混合字体文档示例", 100, 750)

                // 英文用标准字体
                .WithFont(stdFont)
                .WithFontSize(14)
                .WithTextColor(PdfColor.Black)
                .Text("Mixed Font Document Example", 100, 720)

                // 中文内容
                .WithFont(chineseFont)
                .WithFontSize(12)
                .Text("这个文档同时使用了标准字体和中文字体。", 100, 680)

                // 英文内容
                .WithFont(stdFont)
                .Text("This document uses both standard and custom fonts.", 100, 660)

                .WithFont(chineseFont)
                .WithFontSize(11)
                .WithTextColor(PdfColor.DarkGray)
                .Text("标准字体: Hello World! 123", 100, 630)
                .Text("中文字体: 你好世界！１２３", 100, 615)

                .Commit();

            doc3.Save("test-mixed-fonts.pdf");
            Console.WriteLine("✓ 已保存: test-mixed-fonts.pdf\n");

            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("✓ 所有测试完成！");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("\n生成的文件:");
            Console.WriteLine("  1. test-standard-font.pdf  - 标准字体（中文乱码）");
            Console.WriteLine("  2. test-chinese-font.pdf   - 中文字体（正确显示）");
            Console.WriteLine("  3. test-mixed-fonts.pdf    - 混合字体");
            Console.WriteLine("\n请打开 PDF 文件查看效果！");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ 错误: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
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
