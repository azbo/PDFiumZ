using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// 表示 PDF 表单字段
/// </summary>
public class PdfFormField
{
    /// <summary>
    /// 字段类型
    /// </summary>
    public PdfFormFieldType FieldType { get; set; }

    /// <summary>
    /// 字段名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 字段值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 标志（只读、必填等）
    /// </summary>
    public PdfFormFieldFlags Flags { get; set; }

    /// <summary>
    /// 创建一个新的表单字段
    /// </summary>
    public PdfFormField(PdfFormFieldType fieldType, string name)
    {
        FieldType = fieldType;
        Name = name;
    }
}
