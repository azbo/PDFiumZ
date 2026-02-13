namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF 文档元数据
/// </summary>
public class PdfMetadata
{
    /// <summary>
    /// 文档标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 文档作者
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 文档主题
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 文档关键词
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// 创建文档的应用程序
    /// </summary>
    public string? Creator { get; set; }

    /// <summary>
    /// PDF 生产器
    /// </summary>
    public string? Producer { get; set; }

    /// <summary>
    /// 创建日期
    /// </summary>
    public string? CreationDate { get; set; }

    /// <summary>
    /// 修改日期
    /// </summary>
    public string? ModDate { get; set; }
}
