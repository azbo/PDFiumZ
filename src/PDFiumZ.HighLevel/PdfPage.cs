using System;
using System.IO;
using PDFiumZ;
using SkiaSharp;

namespace PDFiumZ.HighLevel;

/// <summary>
/// 表示 PDF 中的一个页面（简化版本）
/// </summary>
public class PdfPage : IDisposable
{
    private readonly PdfDocument _document;
    private readonly int _index;
    private FpdfPageT? _handle;

    internal PdfPage(PdfDocument document, int index)
    {
        _document = document;
        _index = index;
    }

    /// <summary>
    /// 获取页面索引
    /// </summary>
    public int Index => _index;

    /// <summary>
    /// 获取页面宽度（点）
    /// </summary>
    public float Width
    {
        get
        {
            double widthD = 0, heightD = 0;
            int result = fpdfview.FPDF_GetPageSizeByIndex(_document.Handle, _index, ref widthD, ref heightD);
            if (result == 0)
                throw new InvalidOperationException($"Failed to get page size for page {_index}");
            return (float)widthD;
        }
    }

    /// <summary>
    /// 获取页面高度（点）
    /// </summary>
    public float Height
    {
        get
        {
            double widthD = 0, heightD = 0;
            int result = fpdfview.FPDF_GetPageSizeByIndex(_document.Handle, _index, ref widthD, ref heightD);
            if (result == 0)
                throw new InvalidOperationException($"Failed to get page size for page {_index}");
            return (float)heightD;
        }
    }

    /// <summary>
    /// 渲染页面为位图
    /// </summary>
    public PdfBitmap Render(ImageGenerationSettings? settings = null)
    {
        settings ??= new ImageGenerationSettings();

        // 根据设置的 DPI 计算缩放比例
        float scale = settings.RasterDpi / 72.0f; // PDF 标准 DPI 是 72

        int width = (int)(Width * scale);
        int height = (int)(Height * scale);

        FpdfBitmapT bitmap = fpdfview.FPDFBitmapCreateEx(
            width, height,
            (int)FPDFBitmapFormat.BGRA,
            IntPtr.Zero, 0);

        if (bitmap == null)
            throw new InvalidOperationException("Failed to create bitmap");

        // 填充背景色 - SKColor 是 BGRA 格式
        if (settings.BackgroundColor.HasValue)
        {
            // 将 SKColor 转换为 BGRA uint 值
            var c = settings.BackgroundColor.Value;
            uint color = (uint)c.Blue | ((uint)c.Green << 8) | ((uint)c.Red << 16) | ((uint)c.Alpha << 24);
            fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, width, height, color);
        }

        // 设置变换矩阵
        using var matrix = new FS_MATRIX_();
        using var clipping = new FS_RECTF_();

        matrix.A = scale;
        matrix.B = 0;
        matrix.C = 0;
        matrix.D = scale;
        matrix.E = 0;
        matrix.F = 0;

        clipping.Left = 0;
        clipping.Right = width;
        clipping.Bottom = 0;
        clipping.Top = height;

        // 渲染页面 - 加载页面句柄
        FpdfPageT pageHandle = fpdfview.FPDF_LoadPage(_document.Handle, _index);
        if (pageHandle == null)
            throw new InvalidOperationException($"Failed to load page at index {_index}");

        fpdfview.FPDF_RenderPageBitmapWithMatrix(
            bitmap, pageHandle, matrix, clipping,
            (int)settings.RenderFlags);

        return new PdfBitmap(bitmap, width, height);
    }

    /// <summary>
    /// 生成图像字节流
    /// </summary>
    public byte[] GenerateImage()
    {
        return GenerateImage(new ImageGenerationSettings());
    }

    /// <summary>
    /// 使用设置生成图像字节流
    /// </summary>
    public byte[] GenerateImage(ImageGenerationSettings settings)
    {
        using PdfBitmap bitmap = Render(settings);

        using var stream = new MemoryStream();
        switch (settings.ImageFormat)
        {
            case ImageFormat.Png:
                bitmap.SaveAsPng(stream);
                break;
            case ImageFormat.Jpeg:
                bitmap.SaveAsJpeg(stream, (int)settings.ImageCompressionQuality);
                break;
            case ImageFormat.Bmp:
                bitmap.SaveAsBmp(stream);
                break;
        }

        return stream.ToArray();
    }

    /// <summary>
    /// 保存为图像文件
    /// </summary>
    public void SaveAsImage(string filePath)
    {
        SaveAsImage(filePath, new ImageGenerationSettings());
    }

    /// <summary>
    /// 使用设置保存为图像文件
    /// </summary>
    public void SaveAsImage(string filePath, ImageGenerationSettings settings)
    {
        using PdfBitmap bitmap = Render(settings);

        switch (settings.ImageFormat)
        {
            case ImageFormat.Png:
                bitmap.SaveAsPng(filePath);
                break;
            case ImageFormat.Jpeg:
                bitmap.SaveAsJpeg(filePath, (int)settings.ImageCompressionQuality);
                break;
            case ImageFormat.Bmp:
                bitmap.SaveAsBmp(filePath);
                break;
        }
    }

    /// <summary>
    /// 提取页面中的所有超链接
    /// </summary>
    public System.Collections.Generic.List<PdfLink> GetLinks()
    {
        var links = new System.Collections.Generic.List<PdfLink>();

        // 加载文本页面
        FpdfTextpageT textPageHandle = fpdf_text.FPDFTextLoadPage(_handle);
        if (textPageHandle == null)
            return links;

        try
        {
            // 加载链接
            IntPtr linkPageHandle = fpdf_text.FPDFLinkLoadWebLinks(textPageHandle);
            if (linkPageHandle == IntPtr.Zero)
                return links;

            try
            {
                int linkCount = fpdf_text.FPDFLinkCountWebLinks(linkPageHandle);

                for (int i = 0; i < linkCount; i++)
                {
                    // 获取 URL
                    int urlLen = fpdf_text.FPDFLinkGetURL(linkPageHandle, i, IntPtr.Zero, 0);
                    if (urlLen == 0)
                        continue;

                    unsafe
                    {
                        var urlBuffer = stackalloc ushort[urlLen];
                        fpdf_text.FPDFLinkGetURL(linkPageHandle, i, urlBuffer, urlLen);

                        // UTF-16LE 转 string
                        char[] charBuffer = new char[urlLen];
                        for (int j = 0; j < urlLen; j++)
                        {
                            charBuffer[j] = (char)urlBuffer[j];
                        }
                        string url = new string(charBuffer);

                        links.Add(new PdfLink { Url = url });
                    }
                }
            }
            finally
            {
                fpdf_text.FPDFLinkCloseWebLinks(linkPageHandle);
            }
        }
        finally
        {
            fpdf_text.FPDFTextClosePage(textPageHandle);
        }

        return links;
    }

    public void Dispose()
    {
        if (_handle != null)
        {
            fpdfview.FPDF_ClosePage(_handle);
            _handle = null;
        }
    }
}
