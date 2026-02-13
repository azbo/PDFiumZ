using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF 表单字段标志
/// </summary>
[Flags]
public enum PdfFormFieldFlags
{
    /// <summary>
    /// 字段是只读的
    /// </summary>
    ReadOnly = 1 << 0,

    /// <summary>
    /// 字段是必填的
    /// </summary>
    Required = 1 << 1,

    /// <summary>
    /// 字段应高亮显示
    /// </summary>
    Highlight = 1 << 2
}
