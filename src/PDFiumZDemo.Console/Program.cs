using System;
using PDFiumZ.HighLevel;

class Program
{
    static void Main(string[] args)
    {
        // 尝试从参数获取 PDF 文件路径，否则使用默认测试文件
        string pdfPath = args.Length > 0 ? args[0] : "../PDFiumZ.Tests/pdf-sample.pdf";

        if (!System.IO.File.Exists(pdfPath))
        {
            Console.WriteLine($"错误: PDF 文件不存在: {pdfPath}");
            Console.WriteLine("用法: PDFiumZDemo.Console <pdf文件路径>");
            return;
        }

        Console.WriteLine($"================================================");
        Console.WriteLine($"PDF 文档信息输出工具");
        Console.WriteLine($"================================================");
        Console.WriteLine($"文件: {pdfPath}");
        Console.WriteLine();

        try
        {
            using var document = new PdfDocument(pdfPath);

            // 输出文档基本信息
            Console.WriteLine($"【文档基本信息】");
            Console.WriteLine($"  总页数: {document.PageCount}");
            Console.WriteLine();

            // 输出每页信息
            Console.WriteLine($"【页面详细信息】");
            for (int i = 0; i < document.PageCount; i++)
            {
                using var page = document.Pages[i];
                Console.WriteLine($"  页面 {i}:");
                Console.WriteLine($"    - 宽度: {page.Width:F2} 点");
                Console.WriteLine($"    - 高度: {page.Height:F2} 点");
                Console.WriteLine($"    - 尺寸: {page.Width:F2} x {page.Height:F2} 点");
                Console.WriteLine($"    - 尺寸 (毫米): {page.Width * 25.4 / 72:F2} x {page.Height * 25.4 / 72:F2} mm");
                Console.WriteLine();
            }

            // 渲染第一页为图像
            if (document.PageCount > 0)
            {
                Console.WriteLine($"【图像渲染测试】");
                using var firstPage = document.Pages[0];
                using var bitmap = firstPage.Render();

                Console.WriteLine($"  第一页渲染成功:");
                Console.WriteLine($"    - 位图宽度: {bitmap.Width} 像素");
                Console.WriteLine($"    - 位图高度: {bitmap.Height} 像素");
                Console.WriteLine();

                // 保存第一页为 PNG
                string outputPath = "page-0-output.png";
                firstPage.SaveAsImage(outputPath);
                Console.WriteLine($"  已保存第一页为: {outputPath}");
                Console.WriteLine($"  文件大小: {new System.IO.FileInfo(outputPath).Length} 字节");
            }

            Console.WriteLine();
            Console.WriteLine($"================================================");
            Console.WriteLine($"输出完成！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
            Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
        }
    }
}
