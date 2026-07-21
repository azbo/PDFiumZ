using System;
using System.IO;
using System.Runtime.InteropServices;
using PDFiumZ;

namespace PDFiumZ;

/// <summary>
/// PDF 文档操作扩展方法
/// </summary>
public static class PdfDocumentOperations
{
    /// <summary>
    /// 将文档保存到字节数组
    /// </summary>
    public static byte[] Save(this PdfDocument document)
    {
        using var ms = new MemoryStream();
        Save(document, ms);
        return ms.ToArray();
    }

    /// <summary>
    /// 将文档保存到流
    /// </summary>
    public static void Save(this PdfDocument document, Stream stream)
    {
        using var holder = new FileWriteHolder(stream);
        int result = fpdf_save.FPDF_SaveAsCopy(document.Handle, holder.FileWrite, 0);
        if (result == 0)
            throw new InvalidOperationException("Failed to save PDF document");
    }

    /// <summary>
    /// 将文档保存到文件
    /// </summary>
    public static void Save(this PdfDocument document, string filePath)
    {
        using var stream = File.Create(filePath);
        Save(document, stream);
    }

    /// <summary>
    /// 从源文档导入页面到当前文档
    /// </summary>
    public static void ImportPages(this PdfDocument document, PdfDocument source, string? pageRange = null, int insertIndex = -1)
    {
        int result = fpdf_ppo.FPDF_ImportPages(document.Handle, source.Handle, pageRange, insertIndex);
        if (result == 0)
            throw new InvalidOperationException("Failed to import pages");
    }

    /// <summary>
    /// 从源文档按索引导入指定页面
    /// </summary>
    public static void ImportPages(this PdfDocument document, PdfDocument source, int[] pageIndices, int insertIndex = -1)
    {
        int result = fpdf_ppo.FPDF_ImportPagesByIndex(document.Handle, source.Handle, ref pageIndices[0], (ulong)pageIndices.Length, insertIndex);
        if (result == 0)
            throw new InvalidOperationException("Failed to import pages by index");
    }

    /// <summary>
    /// 删除文档中的指定页面
    /// </summary>
    public static void DeletePage(this PdfDocument document, int pageIndex)
    {
        fpdf_edit.FPDFPageDelete(document.Handle, pageIndex);
    }

    /// <summary>
    /// 将文档拆分为单页 PDF 字节数组
    /// </summary>
    public static byte[][] SplitToPages(this PdfDocument document)
    {
        int count = document.PageCount;
        var pages = new byte[count][];

        for (int i = 0; i < count; i++)
        {
            FpdfDocumentT newDoc = fpdf_edit.FPDF_CreateNewDocument();
            if (newDoc == null)
                throw new InvalidOperationException("Failed to create new document");

            try
            {
                int[] indices = { i };
                int result = fpdf_ppo.FPDF_ImportPagesByIndex(newDoc, document.Handle, ref indices[0], 1, 0);
                if (result == 0)
                    throw new InvalidOperationException($"Failed to import page {i}");

                using var ms = new MemoryStream();
                using var holder = new FileWriteHolder(ms);
                fpdf_save.FPDF_SaveAsCopy(newDoc, holder.FileWrite, 0);
                pages[i] = ms.ToArray();
            }
            finally
            {
                fpdfview.FPDF_CloseDocument(newDoc);
            }
        }

        return pages;
    }

    /// <summary>
    /// 合并多个 PDF 文档为一个
    /// </summary>
    public static void MergeDocuments(string outputPath, string[] inputPaths)
    {
        if (inputPaths == null || inputPaths.Length == 0)
            throw new ArgumentException("Input paths cannot be empty", nameof(inputPaths));

        using var firstDoc = new PdfDocument(inputPaths[0]);

        for (int i = 1; i < inputPaths.Length; i++)
        {
            using var sourceDoc = new PdfDocument(inputPaths[i]);
            firstDoc.ImportPages(sourceDoc);
        }

        firstDoc.Save(outputPath);
    }

    /// <summary>
    /// 将多页文档 N-up 合并为单页
    /// </summary>
    public static PdfDocument ImportNPagesToOne(this PdfDocument document, float outputWidth, float outputHeight, ulong pagesOnX, ulong pagesOnY)
    {
        FpdfDocumentT newDoc = fpdf_ppo.FPDF_ImportNPagesToOne(document.Handle, outputWidth, outputHeight, pagesOnX, pagesOnY);
        if (newDoc == null)
            throw new InvalidOperationException("Failed to create N-up document");

        return new PdfDocument(newDoc);
    }
}

/// <summary>
/// 持有 FPDF_FILEWRITE_ 和回调委托，防止 GC 回收
/// </summary>
internal class FileWriteHolder : IDisposable
{
    private readonly Delegates.Func_int___IntPtr___IntPtr_ulong _callback;
    private bool _disposed;

    public FileWriteHolder(Stream stream)
    {
        _callback = (_, data, size) =>
        {
            try
            {
                byte[] buffer = new byte[(int)size];
                Marshal.Copy(data, buffer, 0, (int)size);
                stream.Write(buffer, 0, (int)size);
                return 1;
            }
            catch
            {
                return 0;
            }
        };

        FileWrite = new FPDF_FILEWRITE_();
        FileWrite.Version = 1;
        FileWrite.WriteBlock = _callback;
    }

    public FPDF_FILEWRITE_ FileWrite { get; }

    public void Dispose()
    {
        if (!_disposed)
        {
            FileWrite?.Dispose();
            _disposed = true;
        }
    }
}
