using System;

namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF 渲染标志
/// </summary>
[Flags]
public enum PdfRenderFlags
{
    /// <summary>渲染注释</summary>
    RenderAnnotations = 0x01,

    /// <summary>LCD 文本优化</summary>
    LcdTextOptimization = 0x02,

    /// <summary>不使用原生文本输出</summary>
    NoNativeText = 0x04,

    /// <summary>灰度输出</summary>
    Grayscale = 0x08,

    /// <summary>限制图像缓存大小</summary>
    LimitImageCacheSize = 0x200,

    /// <summary>强制使用半色调</summary>
    ForceHalftone = 0x400,

    /// <summary>打印模式渲染</summary>
    RenderForPrinting = 0x800,

    /// <summary>禁用文本抗锯齿</summary>
    DisableTextAntialiasing = 0x1000,

    /// <summary>禁用图像抗锯齿</summary>
    DisableImageAntialiasing = 0x2000,

    /// <summary>禁用路径抗锯齿</summary>
    DisablePathAntialiasing = 0x4000
}
