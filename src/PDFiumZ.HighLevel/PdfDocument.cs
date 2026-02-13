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
    private PdfMetadata? _metadata;

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

    /// <summary>
    /// 获取文档元数据
    /// </summary>
    public PdfMetadata Metadata => _metadata ??= LoadMetadata();

    private PdfMetadata LoadMetadata()
    {
        return new PdfMetadata
        {
            Title = GetMetaText("Title"),
            Author = GetMetaText("Author"),
            Subject = GetMetaText("Subject"),
            Keywords = GetMetaText("Keywords"),
            Creator = GetMetaText("Creator"),
            Producer = GetMetaText("Producer"),
            CreationDate = GetMetaText("CreationDate"),
            ModDate = GetMetaText("ModDate")
        };
    }

    private string? GetMetaText(string tag)
    {
        // 首次调用获取所需缓冲区大小
        ulong len = fpdf_doc.FPDF_GetMetaText(Handle, tag, IntPtr.Zero, 0);
        if (len == 0)
            return null;

        // 分配缓冲区并获取实际数据
        byte[] buffer = new byte[(int)len];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                fpdf_doc.FPDF_GetMetaText(Handle, tag, new IntPtr(ptr), len);
            }
        }
        // UTF-16LE 编码，前两个字节是 BOM
        return System.Text.Encoding.Unicode.GetString(buffer, 2, (int)len - 2);
    }

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

    // ==================== 表单操作 ===================

    /// <summary>
    /// 获取所有表单字段
    /// </summary>
    public System.Collections.Generic.List<PdfFormField> GetFormFields()
    {
        var fields = new System.Collections.Generic.List<PdfFormField>();

        for (int i = 0; i < PageCount; i++)
        {
            using var page = Pages[i];
            FpdfPageT pageHandle = fpdfview.FPDF_LoadPage(Handle, i);
            if (pageHandle == null)
                continue;

            try
            {
                int fieldCount = fpdfview.FPDFPageGetFormFieldCount(pageHandle);

                for (int j = 0; j < fieldCount; j++)
                {
                    // 获取字段类型
                    int fieldType = fpdfview.FPDFPageGetFormFieldType(pageHandle, j);
                    string name = GetFormFieldName(pageHandle, j);

                    fields.Add(new PdfFormField((PdfFormFieldType)fieldType, name));
                }
            }
            finally
            {
                fpdfview.FPDF_ClosePage(pageHandle);
            }
        }

        return fields;
    }

    /// <summary>
    /// 获取表单字段名称
    /// </summary>
    private string GetFormFieldName(FpdfPageT pageHandle, int fieldIndex)
    {
        // 首先获取名称长度
        unsafe
        {
            ushort len = fpdfview.FPDFPageGetFormFieldFullName(pageHandle, fieldIndex, null, 0);
            if (len == 0)
                return string.Empty;

            // 分配缓冲区
            var buffer = new char[len];
            fixed (char* ptr = buffer)
            {
                fpdfview.FPDFPageGetFormFieldFullName(pageHandle, fieldIndex, new IntPtr(ptr), len);
            }

            return new string(buffer).TrimEnd('\0');
        }
    }

    /// <summary>
    /// 获取表单字段值
    /// </summary>
    private string? GetFormFieldValue(FpdfPageT pageHandle, int fieldIndex)
    {
        // 首先获取值长度
        unsafe
        {
            ushort len = fpdfview.FPDFPageGetFormFieldValue(pageHandle, fieldIndex, null, 0);
            if (len == 0)
                return null;

            // 分配缓冲区
            var buffer = new char[len];
            fixed (char* ptr = buffer)
            {
                fpdfview.FPDFPageGetFormFieldValue(pageHandle, fieldIndex, new IntPtr(ptr), len);
            }

            return new string(buffer).TrimEnd('\0');
        }
    }
}
