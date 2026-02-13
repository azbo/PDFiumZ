namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF 页面中的超链接
/// </summary>
public class PdfLink
{
    /// <summary>
    /// 链接 URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 链接文本的起始字符索引
    /// </summary>
    public int StartIndex { get; set; }

    /// <summary>
    /// 链接文本的字符数量
    /// </summary>
    public int CharCount { get; set; }

    /// <summary>
    /// 链接文本的结束字符索引（不包括）
    /// </summary>
    public int EndIndex => StartIndex + CharCount;
}
