using System;
using System.Collections.Generic;
using PDFiumZ;

namespace PDFiumZ.HighLevel;

/// <summary>
/// 表示 PDF 页面的文本内容，提供文本提取和搜索功能
/// </summary>
public class PdfTextPage : IDisposable
{
    private readonly FpdfTextpageT _handle;
    private bool _disposed;

    internal PdfTextPage(FpdfTextpageT handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// 页面中的字符总数
    /// </summary>
    public int CharCount => fpdf_text.FPDFTextCountChars(_handle);

    /// <summary>
    /// 获取页面中的所有文本
    /// </summary>
    public string GetAllText()
    {
        int charCount = CharCount;
        if (charCount <= 0)
            return string.Empty;

        // FPDFText_GetText 需要 (charCount + 1) 的缓冲区
        var buffer = new ushort[charCount + 1];
        int length = fpdf_text.FPDFTextGetText(_handle, 0, charCount, ref buffer[0]);

        if (length <= 0)
            return string.Empty;

        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = (char)buffer[i];

        return new string(chars, 0, length - 1); // 去掉末尾的 null 终止符
    }

    /// <summary>
    /// 获取指定矩形区域内的文本
    /// </summary>
    /// <param name="left">左边界（页面坐标）</param>
    /// <param name="top">上边界（页面坐标）</param>
    /// <param name="right">右边界（页面坐标）</param>
    /// <param name="bottom">下边界（页面坐标）</param>
    public string GetBoundedText(double left, double top, double right, double bottom)
    {
        // 先获取所需缓冲区大小
        int len = fpdf_text.FPDFTextGetBoundedText(_handle, left, top, right, bottom, ref _dummyUshort, 0);
        if (len <= 0)
            return string.Empty;

        var buffer = new ushort[len];
        len = fpdf_text.FPDFTextGetBoundedText(_handle, left, top, right, bottom, ref buffer[0], len);

        if (len <= 0)
            return string.Empty;

        var chars = new char[len];
        for (int i = 0; i < len; i++)
            chars[i] = (char)buffer[i];

        return new string(chars);
    }

    /// <summary>
    /// 在页面文本中搜索指定字符串
    /// </summary>
    /// <param name="text">搜索文本</param>
    /// <param name="startIndex">起始字符索引（默认 0）</param>
    /// <param name="matchCase">是否区分大小写</param>
    /// <param name="matchWholeWord">是否全词匹配</param>
    /// <returns>搜索结果列表</returns>
    public List<PdfSearchResult> Search(string text, int startIndex = 0, bool matchCase = false, bool matchWholeWord = false)
    {
        var results = new List<PdfSearchResult>();

        if (string.IsNullOrEmpty(text))
            return results;

        ulong flags = 0;
        if (matchCase)
            flags |= 0x00000001; // FPDF_MATCHCASE
        if (matchWholeWord)
            flags |= 0x00000002; // FPDF_MATCHWHOLEWORD

        var searchText = new ushort[text.Length + 1];
        for (int i = 0; i < text.Length; i++)
            searchText[i] = text[i];

        FpdfSchhandleT searchHandle = fpdf_text.FPDFTextFindStart(_handle, ref searchText[0], flags, startIndex);
        if (searchHandle == null)
            return results;

        try
        {
            while (fpdf_text.FPDFTextFindNext(searchHandle) != 0)
            {
                int index = fpdf_text.FPDFTextGetSchResultIndex(searchHandle);
                int count = fpdf_text.FPDFTextGetSchCount(searchHandle);
                results.Add(new PdfSearchResult(index, count));
            }
        }
        finally
        {
            fpdf_text.FPDFTextFindClose(searchHandle);
        }

        return results;
    }

    /// <summary>
    /// 获取指定字符的 Unicode 值
    /// </summary>
    public uint GetCharUnicode(int index)
    {
        return fpdf_text.FPDFTextGetUnicode(_handle, index);
    }

    /// <summary>
    /// 获取指定字符的字体大小
    /// </summary>
    public double GetCharFontSize(int index)
    {
        return fpdf_text.FPDFTextGetFontSize(_handle, index);
    }

    /// <summary>
    /// 获取指定字符的边界框
    /// </summary>
    public bool GetCharBox(int index, out double left, out double right, out double bottom, out double top)
    {
        left = right = bottom = top = 0;
        return fpdf_text.FPDFTextGetCharBox(_handle, index, ref left, ref right, ref bottom, ref top) != 0;
    }

    private static ushort _dummyUshort;

    public void Dispose()
    {
        if (!_disposed)
        {
            fpdf_text.FPDFTextClosePage(_handle);
            _disposed = true;
        }
    }
}

/// <summary>
/// 文本搜索结果
/// </summary>
public readonly struct PdfSearchResult
{
    /// <summary>
    /// 匹配文本的起始字符索引
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// 匹配文本的字符数
    /// </summary>
    public int Length { get; }

    internal PdfSearchResult(int index, int length)
    {
        Index = index;
        Length = length;
    }
}
