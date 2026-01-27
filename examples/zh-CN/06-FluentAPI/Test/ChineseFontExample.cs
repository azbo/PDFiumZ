using PDFiumZ;
using PDFiumZ.HighLevel;
using PDFiumZ.Fluent.Document;
using System;

/// <summary>
/// PDFiumZ 中文字体使用示例
/// </summary>
class ChineseFontExample
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== PDFiumZ 中文字体使用示例 ===\n");

        PdfiumLibrary.Initialize();

        try
        {
            // 示例 1: 基础中文文本
            Console.WriteLine("示例 1: 基础中文文本");
            CreateBasicChineseDocument();

            // 示例 2: 多语言混合文档
            Console.WriteLine("\n示例 2: 多语言混合文档");
            CreateMultiLanguageDocument();

            // 示例 3: 使用 FontHelper
            Console.WriteLine("\n示例 3: 使用 FontHelper");
            CreateWithFontHelper();

            Console.WriteLine("\n=== 所有示例完成 ===");
            Console.WriteLine("\n生成的文件:");
            Console.WriteLine("  - example-basic-chinese.pdf");
            Console.WriteLine("  - example-multi-language.pdf");
            Console.WriteLine("  -example-with-helper.pdf");
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
    /// 示例 1: 创建基础中文文档
    /// </summary>
    static void CreateBasicChineseDocument()
    {
        using var doc = PdfDocument.CreateNew();
        using var page = doc.CreatePage(595, 842); // A4

        // 加载中文字体
        using var font = PdfFont.Load(doc, @"C:\Windows\Fonts\simhei.ttf", isCidFont: true);
        using var editor = page.BeginEdit();

        editor
            .WithFont(font)
            .WithFontSize(24)
            .WithTextColor(PdfColor.DarkBlue)
            .Text("PDFiumZ 中文支持示例", 100, 750)

            .WithFontSize(14)
            .WithTextColor(PdfColor.Black)
            .Text("这是一个包含中文的 PDF 文档。", 100, 700)

            .WithFontSize(12)
            .Text("主要特性：", 100, 660)
            .Text("• 完整的 CJK 字符支持", 120, 640)
            .Text("• 简体中文、繁体中文、日文、韩文", 120, 625)
            .Text("• Unicode 完整支持", 120, 610)

            .Commit();

        doc.Save("example-basic-chinese.pdf");
        Console.WriteLine("✓ 已保存: example-basic-chinese.pdf");
    }

    /// <summary>
    /// 示例 2: 创建多语言混合文档
    /// </summary>
    static void CreateMultiLanguageDocument()
    {
        using var doc = PdfDocument.CreateNew();
        using var page = doc.CreatePage(595, 842);

        // 加载中文字体和标准字体
        using var chineseFont = PdfFont.Load(doc, @"C:\Windows\Fonts\simhei.ttf", isCidFont: true);
        using var stdFont = PdfFont.Load(doc, PdfStandardFont.HelveticaBold);

        using var editor = page.BeginEdit();

        // 中文标题
        editor
            .WithFont(chineseFont)
            .WithFontSize(20)
            .WithTextColor(PdfColor.DarkBlue)
            .Text("多语言文档示例", 100, 750)

            // 英文标题
            .WithFont(stdFont)
            .WithFontSize(16)
            .WithTextColor(PdfColor.DarkRed)
            .Text("Multi-Language Document Example", 100, 720)

            // 中文内容
            .WithFont(chineseFont)
            .WithFontSize(12)
            .WithTextColor(PdfColor.Black)
            .Text("简体中文: 你好世界", 100, 680)
            .Text("繁體中文: 你好世界", 100, 660)
            .Text("日文: こんにちは世界", 100, 640)
            .Text("韩文: 안녕하세요 세계", 100, 620)

            // 混合文本
            .WithFontSize(11)
            .WithTextColor(PdfColor.DarkGray)
            .Text("混合文本: 中文 English 日本語 한국어", 100, 590)

            .Commit();

        doc.Save("example-multi-language.pdf");
        Console.WriteLine("✓ 已保存: example-multi-language.pdf");
    }

    /// <summary>
    /// 示例 3: 使用 FontHelper 辅助类
    /// </summary>
    static void CreateWithFontHelper()
    {
        using var doc = PdfDocument.CreateNew();
        using var page = doc.CreatePage(595, 842);

        // 使用 FontHelper 自动加载中文字体
        using var font = FontHelper.LoadChineseFont(doc);
        using var editor = page.BeginEdit();

        editor
            .WithFont(font)
            .WithFontSize(18)
            .WithTextColor(PdfColor.DarkGreen)
            .Text("使用 FontHelper 加载字体", 100, 750)

            .WithFontSize(12)
            .WithTextColor(PdfColor.Black)
            .Text("FontHelper 会自动检测系统中的中文字体", 100, 710)
            .Text("无需手动指定字体路径", 100, 695)
            .Text("支持 Windows、macOS 和 Linux", 100, 680)

            .Commit();

        doc.Save("example-with-helper.pdf");
        Console.WriteLine("✓ 已保存: example-with-helper.pdf");
    }
}
