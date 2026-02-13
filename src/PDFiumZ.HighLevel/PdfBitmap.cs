using System;
using System.IO;
using PDFiumZ;
using SkiaSharp;

namespace PDFiumZ.HighLevel;

/// <summary>
/// 表示渲染的 PDF 位图（使用 SkiaSharp）
/// </summary>
public class PdfBitmap : IDisposable
{
    private readonly FpdfBitmapT _handle;

    /// <summary>
    /// 位图宽度（像素）
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 位图高度（像素）
    /// </summary>
    public int Height { get; }

    internal PdfBitmap(FpdfBitmapT handle, int width, int height)
    {
        _handle = handle;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// 获取原始像素数据
    /// </summary>
    public ReadOnlySpan<byte> GetData()
    {
        int stride = Width * 4; // BGRA = 4 bytes per pixel
        IntPtr buffer = fpdfview.FPDFBitmapGetBuffer(_handle);

        if (buffer == IntPtr.Zero)
            return ReadOnlySpan<byte>.Empty;

        unsafe
        {
            byte* ptr = (byte*)buffer;
            return new ReadOnlySpan<byte>(ptr, stride * Height);
        }
    }

    /// <summary>
    /// 保存为 PNG 文件
    /// </summary>
    public void SaveAsPng(string filePath)
    {
        using SKBitmap bitmap = ToSKBitmap();
        using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(filePath, data.ToArray());
    }

    /// <summary>
    /// 保存为 PNG 流
    /// </summary>
    public void SaveAsPng(Stream stream)
    {
        using SKBitmap bitmap = ToSKBitmap();
        using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        byte[] dataArray = data.ToArray();
        stream.Write(dataArray, 0, dataArray.Length);
    }

    /// <summary>
    /// 保存为 JPEG 文件
    /// </summary>
    public void SaveAsJpeg(string filePath, int quality = 90)
    {
        using SKBitmap bitmap = ToSKBitmap();
        using SKData data = bitmap.Encode(SKEncodedImageFormat.Jpeg, quality);
        File.WriteAllBytes(filePath, data.ToArray());
    }

    /// <summary>
    /// 保存为 JPEG 流
    /// </summary>
    public void SaveAsJpeg(Stream stream, int quality = 90)
    {
        using SKBitmap bitmap = ToSKBitmap();
        using SKData data = bitmap.Encode(SKEncodedImageFormat.Jpeg, quality);
        byte[] dataArray = data.ToArray();
        stream.Write(dataArray, 0, dataArray.Length);
    }

    /// <summary>
    /// 保存为 BMP 文件
    /// </summary>
    public void SaveAsBmp(string filePath)
    {
        using SKBitmap bitmap = ToSKBitmap();
        using SKData data = bitmap.Encode(SKEncodedImageFormat.Bmp, 100);
        File.WriteAllBytes(filePath, data.ToArray());
    }

    /// <summary>
    /// 保存为 BMP 流
    /// </summary>
    public void SaveAsBmp(Stream stream)
    {
        using SKBitmap bitmap = ToSKBitmap();
        using SKData data = bitmap.Encode(SKEncodedImageFormat.Bmp, 100);
        byte[] dataArray = data.ToArray();
        stream.Write(dataArray, 0, dataArray.Length);
    }

    /// <summary>
    /// 转换为 SkiaSharp.SKBitmap
    /// </summary>
    public SKBitmap ToSKBitmap()
    {
        ReadOnlySpan<byte> data = GetData();
        SKBitmap bitmap = new SKBitmap(new SKImageInfo
        {
            Width = Width,
            Height = Height,
            ColorType = SKColorType.Bgra8888,
            AlphaType = SKAlphaType.Premul
        });

        // 安装像素数据
        unsafe
        {
            fixed (byte* ptr = data)
            {
                bitmap.SetPixels((nint)ptr);
            }
        }

        return bitmap;
    }

    public void Dispose()
    {
        // SkiaSharp SKBitmap 会自动管理内存
        // PDFium 位图句柄由 PDFium 库管理
    }
}
