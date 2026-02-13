namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF 注释类型
/// </summary>
public enum PdfAnnotationType
{
    /// <summary>未知类型</summary>
    Unknown = 0,

    /// <summary>文本注释</summary>
    Text = 1,

    /// <summary>链接注释</summary>
    Link = 2,

    /// <summary>自由文本注释</summary>
    FreeText = 3,

    /// <summary>线条注释</summary>
    Line = 4,

    /// <summary>方形注释</summary>
    Square = 5,

    /// <summary>圆形注释</summary>
    Circle = 6,

    /// <summary>多边形注释</summary>
    Polygon = 7,

    /// <summary>折线注释</summary>
    PolyLine = 8,

    /// <summary>高亮注释</summary>
    Highlight = 9,

    /// <summary>下划线注释</summary>
    Underline = 10,

    /// <summary>波浪线注释</summary>
    Squiggly = 11,

    /// <summary>删除线注释</summary>
    StrikeOut = 12,

    /// <summary>印章注释</summary>
    Stamp = 13,

    /// <summary>手写注释</summary>
    Caret = 14,

    /// <summary>附注注释</summary>
    Ink = 15,

    /// <summary>弹出窗口注释</summary>
    Popup = 16,

    /// <summary>文件附件注释</summary>
    FileAttachment = 17,

    /// <summary>声音注释</summary>
    Sound = 18,

    /// <summary>电影注释</summary>
    Movie = 19,

    /// <summary>表单字段注释</summary>
    Widget = 20,

    /// <summary>屏幕注释</summary>
    Screen = 21,

    /// <summary>打印机标记注释</summary>
    PrinterMark = 22,

    /// <summary>陷印网络注释</summary>
    TrapNet = 23,

    /// <summary>编辑注释</summary>
    Redact = 24,

    /// <summary>水印注释</summary>
    Watermark = 25,

    /// <summary>3D 注释</summary>
    ThreeD = 26,

    /// <summary>几何字符串注释</summary>
    GeomString = 27,

    /// <summary>富媒体注释</summary>
    RichMedia = 28,

    /// <summary>文本区域注释</summary>
    TextArea = 29
}
