using SkiaSharp;

namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF 页面图像生成设置
/// </summary>
public class ImageGenerationSettings
{
    /// <summary>
    /// 图像格式（默认: PNG）
    /// </summary>
    public ImageFormat ImageFormat { get; set; } = ImageFormat.Png;

    /// <summary>
    /// 图像压缩质量（默认: High）
    /// 仅对 JPEG 格式有效
    /// </summary>
    public ImageCompressionQuality ImageCompressionQuality { get; set; } =
        ImageCompressionQuality.High;

    /// <summary>
    /// 渲染 DPI（默认: 288）
    /// 控制输出图像的分辨率。更高的 DPI 产生更高质量的图像，
    /// 但会增加文件大小和处理时间
    /// </summary>
    public float RasterDpi { get; set; } = 288;

    /// <summary>
    /// 背景颜色（默认: 白色）
    /// </summary>
    public SKColor? BackgroundColor { get; set; } = SKColors.White;

    /// <summary>
    /// 渲染标志（默认: 渲染注释 + LCD 文本优化）
    /// </summary>
    public PdfRenderFlags RenderFlags { get; set; } =
        PdfRenderFlags.RenderAnnotations | PdfRenderFlags.LcdTextOptimization;

    /// <summary>
    /// 旋转角度（默认: 无旋转）
    /// </summary>
    public PdfRotation Rotation { get; set; } = PdfRotation.Rotate0;
}
