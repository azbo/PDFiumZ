using PDFiumZ;
using PDFiumZ.HighLevel;
using System;

class ChineseTextExtractionTest
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== PDFiumZ 中文文本提取测试 ===\n");

        PdfiumLibrary.Initialize();

        try
        {
            // 创建包含中文的 PDF
            Console.WriteLine("创建包含中文的 PDF...");
            using var doc = PdfDocument.CreateNew();
            using var page = doc.CreatePage(595, 842);
            using var font = PdfFont.Load(doc, @"C:\Windows\Fonts\simhei.ttf", isCidFont: true);
            using var editor = page.BeginEdit();

            string testText = "欢迎使用 PDFiumZ！中文文本提取测试。";
            editor
                .WithFont(font)
                .WithFontSize(16)
                .WithTextColor(PdfColor.Black)
                .Text(testText, 100, 750)
                .Text("第二行文字", 100, 720)
                .Commit();

            doc.Save("test-extraction.pdf");
            Console.WriteLine("✓ 已保存: test-extraction.pdf");

            // 打开并提取文本
            Console.WriteLine("\n提取 PDF 文本...");
            using var openDoc = PdfDocument.Open("test-extraction.pdf");
            using var openPage = openDoc.GetPage(0);
            var extractedText = openPage.ExtractText();

            Console.WriteLine($"✓ 提取的文本 ({extractedText.Length} 字符):");
            Console.WriteLine("────────────────────────────────────");
            Console.WriteLine(extractedText);
            Console.WriteLine("────────────────────────────────────");

            // 验证提取的文本是否包含原始内容
            if (extractedText.Contains("欢迎使用") && extractedText.Contains("PDFiumZ"))
            {
                Console.WriteLine("\n✓ 文本提取成功！中文字符正确识别。");
            }
            else
            {
                Console.WriteLine("\n✗ 警告: 提取的文本可能不完整");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n错误: {ex.Message}");
            Console.ResetColor();
        }
        finally
        {
            PdfiumLibrary.Shutdown();
            Console.WriteLine("\n✓ 测试完成");
        }
    }
}
