using System;
using System.IO;
using PDFiumZ;

namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF 文档操作扩展方法
/// </summary>
public static class PdfDocumentOperations
{
    /// <summary>
    /// 从其他 PDF 文档导入页面到当前文档
    /// </summary>
    public static void ImportPages(PdfDocument document, byte[] sourcePdfBytes, int[] pageIndexArray)
    {
        if (sourcePdfBytes == null || sourcePdfBytes.Length == 0)
            throw new ArgumentException("Source PDF cannot be null or empty", nameof(sourcePdfBytes));

        if (pageIndexArray == null || pageIndexArray.Length == 0)
            throw new ArgumentException("Page index array cannot be null or empty", nameof(pageIndexArray));

        unsafe
        {
            fixed (byte* srcPtr = sourcePdfBytes)
            fixed (int* pageIndicesPtr = pageIndexArray)
            {
                fpdf_edit.FPDF_ImportPages(
                    document.Handle,
                    new IntPtr(srcPtr),
                    pageIndexArray.Length,
                    new IntPtr(pageIndicesPtr));
            }
        }
    }

    /// <summary>
    /// 从当前文档导出指定页面到新的 PDF 文件
    /// </summary>
    public static void ExportPages(PdfDocument document, string outputPath, int[] pageIndexArray)
    {
        if (string.IsNullOrEmpty(outputPath))
            throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));

        if (pageIndexArray == null || pageIndexArray.Length == 0)
            throw new ArgumentException("Page index array cannot be null or empty", nameof(pageIndexArray));

        string directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        unsafe
        {
            fixed (int* pageIndicesPtr = pageIndexArray)
            {
                fpdf_edit.FPDF_ImportPages(
                    document.Handle,
                    IntPtr.Zero,
                    pageIndexArray.Length,
                    new IntPtr(pageIndicesPtr));
            }
        }
    }

    /// <summary>
    /// 从当前文档删除指定页面
    /// </summary>
    public static void DeletePages(PdfDocument document, int[] pageIndexArray)
    {
        if (pageIndexArray == null || pageIndexArray.Length == 0)
            throw new ArgumentException("Page index array cannot be null or empty", nameof(pageIndexArray));

        foreach (int pageIndex in pageIndexArray)
        {
            fpdf_edit.FPDFPageDelete(document.Handle, pageIndex);
        }
    }

    /// <summary>
    /// 将单个文档拆分为多个独立的 PDF 文件（每个文件包含一页）
    /// </summary>
    /// <param name="document">源文档</param>
    /// <param name="outputDirectory">输出目录</param>
    /// <returns>拆分后的文件路径列表</returns>
    public static string[] SplitDocument(PdfDocument document, string outputDirectory)
    {
        if (string.IsNullOrEmpty(outputDirectory))
            throw new ArgumentException("Output directory cannot be null or empty", nameof(outputDirectory));

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        string baseName = Path.GetFileNameWithoutExtension(document.FilePath ?? "document");
        var filePaths = new System.Collections.Generic.List<string>();

        for (int i = 0; i < document.PageCount; i++)
        {
            string fileName = Path.Combine(outputDirectory, $"{baseName}_page{i}.pdf");
            ExportPages(document, fileName, new[] { i });
            filePaths.Add(fileName);
        }

        return filepaths.ToArray();
    }

    /// <summary>
    /// 将多个独立的 PDF 文件合并为一个 PDF 文件
    /// </summary>
    /// <param name="outputFilePath">输出文件路径</param>
    /// <param name="inputFilePaths">要合并的 PDF 文件路径列表</param>
    public static void MergeDocuments(string outputFilePath, string[] inputFilePaths)
    {
        if (inputFilePaths == null || inputFilePaths.Length == 0)
            throw new ArgumentException("Input file paths cannot be null or empty", nameof(inputFilePaths));

        if (string.IsNullOrEmpty(outputFilePath))
            throw new ArgumentException("Output file path cannot be null or empty", nameof(outputFilePath));

        // 创建输出目录
        string directory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        unsafe
        {
            fixed (int* countPtr = &inputFilePaths.Length)
            {
                fpdf_edit.FPDF_ImportDocuments(
                    document.Handle,
                    IntPtr.Zero,
                    inputFilePaths.Length,
                    new IntPtr(countPtr));
            }
        }
    }

    /// <summary>
    /// 将当前文档拆分为单页 PDF 文件
    /// </summary>
    /// <param name="document">源文档</param>
    /// <param name="outputDirectory">输出目录</param>
    /// <param name="baseName">基础文件名（默认: "page"）</param>
    /// <returns>拆分后的文件路径列表</returns>
    public static string[] SplitToSinglePageDocuments(PdfDocument document, string outputDirectory, string baseName = "page")
    {
        if (string.IsNullOrEmpty(outputDirectory))
            throw new ArgumentException("Output directory cannot be null or empty", nameof(outputDirectory));

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        string baseName = Path.GetFileNameWithoutExtension(document.FilePath ?? "document");
        var filePaths = new System.Collections.Generic.List<string>();

        for (int i = 0; i < document.PageCount; i++)
        {
            string fileName = Path.Combine(outputDirectory, $"{baseName}_page{i}.pdf");
            ExportPages(document, fileName, new[] { i });
            filePaths.Add(fileName);
        }

        return filePaths.ToArray();
    }
}
