using System;
using PDFiumZ;

namespace PDFiumZ.HighLevel;

/// <summary>
/// 表示 PDF 文档，提供文档级别的操作
/// </summary>
public class PdfDocument : IDisposable
{
    private bool _isInitialized;
    private FpdfDocumentT? _handle;
    private PdfPageCollection? _pages;

    /// <summary>
    /// 从文件路径加载 PDF 文档
    /// </summary>
    /// <param name="filePath">PDF 文件路径</param>
    /// <param name="password">文档密码（如有加密）</param>
    public PdfDocument(string filePath, string? password = null)
    {
        InitializeLibrary();
        _handle = fpdfview.FPDF_LoadDocument(filePath, password);
        if (_handle == null)
            throw new InvalidOperationException($"Failed to load PDF document: {filePath}");
    }

    /// <summary>
    /// 从字节流加载 PDF 文档
    /// </summary>
    /// <param name="stream">包含 PDF 数据的流</param>
    /// <param name="password">文档密码（如有加密）</param>
    public PdfDocument(System.IO.Stream stream, string? password = null)
    {
        InitializeLibrary();

        using var ms = new System.IO.MemoryStream();
        stream.CopyTo(ms);
        byte[] data = ms.ToArray();

        unsafe
        {
            fixed (byte* ptr = data)
            {
                _handle = fpdfview.FPDF_LoadMemDocument(new IntPtr(ptr), data.Length, password);
            }
        }

        if (_handle == null)
            throw new InvalidOperationException("Failed to load PDF document from stream");
    }

    /// <summary>
    /// 从字节数组加载 PDF 文档
    /// </summary>
    /// <param name="data">PDF 数据</param>
    /// <param name="password">文档密码（如有加密）</param>
    public PdfDocument(byte[] data, string? password = null)
    {
        InitializeLibrary();

        unsafe
        {
            fixed (byte* ptr = data)
            {
                _handle = fpdfview.FPDF_LoadMemDocument(new IntPtr(ptr), data.Length, password);
            }
        }

        if (_handle == null)
            throw new InvalidOperationException("Failed to load PDF document from byte array");
    }

    private void InitializeLibrary()
    {
        if (!_isInitialized)
        {
            fpdfview.FPDF_InitLibrary();
            _isInitialized = true;
        }
    }

    internal FpdfDocumentT Handle
    {
        get
        {
            if (_handle == null)
                throw new ObjectDisposedException(nameof(PdfDocument));
            return _handle;
        }
    }

    /// <summary>
    /// 获取文档页数
    /// </summary>
    public int PageCount => (int)fpdfview.FPDF_GetPageCount(Handle);

    /// <summary>
    /// 获取文档所有页面的集合
    /// </summary>
    public PdfPageCollection Pages => _pages ??= new PdfPageCollection(this);

    /// <summary>
    /// 按索引获取页面
    /// </summary>
    public PdfPage this[int index] => Pages[index];

    // ============ 图像生成（QuestPDF 风格）============

    /// <summary>
    /// 将文档的所有页面生成为图像
    /// </summary>
    /// <returns>每页图像的字节数组枚举</returns>
    public System.Collections.Generic.IEnumerable<byte[]> GenerateImages()
    {
        return GenerateImages(new ImageGenerationSettings());
    }

    /// <summary>
    /// 使用指定设置生成图像
    /// </summary>
    /// <param name="settings">图像生成设置</param>
    /// <returns>每页图像的字节数组枚举</returns>
    public System.Collections.Generic.IEnumerable<byte[]> GenerateImages(ImageGenerationSettings settings)
    {
        for (int i = 0; i < PageCount; i++)
        {
            using PdfPage page = Pages[i];
            yield return page.GenerateImage(settings);
        }
    }

    /// <summary>
    /// 生成图像并保存到文件
    /// </summary>
    /// <param name="fileNameCallback">
    /// 用于生成文件名的回调函数，参数为页面索引
    /// 例如: index => $"page{index}.png"
    /// </param>
    public void GenerateImages(System.Func<int, string> fileNameCallback)
    {
        GenerateImages(fileNameCallback, new ImageGenerationSettings());
    }

    /// <summary>
    /// 使用指定设置生成图像并保存到文件
    /// </summary>
    /// <param name="fileNameCallback">文件名回调函数</param>
    /// <param name="settings">图像生成设置</param>
    public void GenerateImages(System.Func<int, string> fileNameCallback, ImageGenerationSettings settings)
    {
        for (int i = 0; i < PageCount; i++)
        {
            using PdfPage page = Pages[i];
            string fileName = fileNameCallback(i);
            string? directory = System.IO.Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            page.SaveAsImage(fileName, settings);
        }
    }

    /// <summary>
    /// 生成图像并保存到指定目录
    /// </summary>
    /// <param name="directory">目标目录</param>
    /// <param name="baseName">基础文件名（默认: "page"）</param>
    /// <param name="settings">可选的图像生成设置</param>
    public void GenerateImagesToDirectory(
        string directory,
        string baseName = "page",
        ImageGenerationSettings? settings = null)
    {
        settings ??= new ImageGenerationSettings();

        GenerateImages(
            index => System.IO.Path.Combine(directory, $"{baseName}{index}.{GetFileExtension(settings.ImageFormat)}"),
            settings
        );
    }

    private static string GetFileExtension(ImageFormat format) => format switch
    {
        ImageFormat.Png => "png",
        ImageFormat.Jpeg => "jpg",
        ImageFormat.Bmp => "bmp",
        _ => "png"
    };

    public void Dispose()
    {
        if (_handle != null)
        {
            _handle = null;
        }

        if (_isInitialized)
        {
            fpdfview.FPDF_DestroyLibrary();
            _isInitialized = false;
        }
    }
}
