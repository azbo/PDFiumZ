using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PDFiumZ;

namespace PDFiumZ.Tests;

/// <summary>
/// AsImgs 简洁 API 展示测试。
/// 与其它测试不同:这里生成的图片会【保留】到固定的 showcase 目录,
/// 方便人工查看渲染效果(不在测试结束后删除)。
///
/// 跑完测试后,图片位于:
///   {测试 bin 目录}/showcase/
/// 也可在测试输出(TestContext.Out)里看到每张图的绝对路径和尺寸。
/// </summary>
[TestFixture]
public class AsImgsShowcaseTests
{
    private const string TestPdfPath = "pdf-sample.pdf";

    /// <summary>
    /// showcase 输出目录(测试 bin 目录下的 showcase/)。
    /// 用 TestContext.CurrentContext.TestDirectory 保证无论从哪运行都能找到。
    /// </summary>
    private static string ShowcaseDir =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "showcase");

    [OneTimeSetUp]
    public void Setup()
    {
        // 每次运行前清空旧产物,确保看到的是本次生成的最新图片
        if (Directory.Exists(ShowcaseDir))
            Directory.Delete(ShowcaseDir, true);
        Directory.CreateDirectory(ShowcaseDir);

        TestContext.WriteLine($"=== showcase 输出目录: {ShowcaseDir} ===");
        TestContext.WriteLine($"=== 源 PDF: {Path.GetFullPath(TestPdfPath)} ===");
        using var doc = new PdfDocument(TestPdfPath);
        TestContext.WriteLine($"=== PDF 页数: {doc.PageCount} ===");
        TestContext.WriteLine();
    }

    /// <summary>
    /// 1. 最简调用:document.AsImgs() —— 所有页面 → 字节数组(默认 PNG / DPI 288)
    /// </summary>
    [Test]
    public void AsImgs_Default_ReturnsByteArrays()
    {
        using var document = new PdfDocument(TestPdfPath);

        IEnumerable<byte[]> images = document.AsImgs();

        int i = 0;
        foreach (byte[] bytes in images)
        {
            TestContext.WriteLine($"  [默认] 页面 {i}: {bytes.Length:N0} 字节");
            // 写到 showcase 目录供人工查看
            File.WriteAllBytes(Path.Combine(ShowcaseDir, $"1_default_page{i}.png"), bytes);
            Assert.That(bytes.Length, Is.GreaterThan(0));
            i++;
        }

        TestContext.WriteLine($"  -> 已生成 {i} 张图,见 showcase/1_default_page*.png");
        Assert.That(i, Is.EqualTo(document.PageCount));
    }

    /// <summary>
    /// 2. 带自定义设置:document.AsImgs(settings) —— JPEG / 高质量 / DPI 150
    /// </summary>
    [Test]
    public void AsImgs_WithSettings_Jpeg()
    {
        using var document = new PdfDocument(TestPdfPath);

        var settings = new ImageGenerationSettings
        {
            ImageFormat = ImageFormat.Jpeg,
            ImageCompressionQuality = ImageCompressionQuality.High,
            RasterDpi = 150
        };

        IEnumerable<byte[]> images = document.AsImgs(settings);

        int i = 0;
        foreach (byte[] bytes in images)
        {
            TestContext.WriteLine($"  [JPEG/DPI150] 页面 {i}: {bytes.Length:N0} 字节");
            File.WriteAllBytes(Path.Combine(ShowcaseDir, $"2_jpeg_dpi150_page{i}.jpg"), bytes);
            Assert.That(bytes.Length, Is.GreaterThan(0));
            i++;
        }

        TestContext.WriteLine($"  -> 已生成 {i} 张 JPEG,见 showcase/2_jpeg_dpi150_page*.jpg");
    }

    /// <summary>
    /// 3. 直接保存文件:document.AsImgs(i => $"image{i}.png")
    /// </summary>
    [Test]
    public void AsImgs_FileCallback_Png()
    {
        using var document = new PdfDocument(TestPdfPath);

        // 用回调命名,输出到 showcase 目录
        document.AsImgs(i => Path.Combine(ShowcaseDir, $"3_callback_image{i}.png"));

        // 验证文件确实生成了
        for (int i = 0; i < document.PageCount; i++)
        {
            string file = Path.Combine(ShowcaseDir, $"3_callback_image{i}.png");
            Assert.That(File.Exists(file), Is.True, $"未找到预期文件: {file}");
            TestContext.WriteLine($"  [回调保存] {file}  ({new FileInfo(file).Length:N0} 字节)");
        }
    }

    /// <summary>
    /// 4. 回调 + 自定义设置:document.AsImgs(i => path, settings)
    ///    演示不同 DPI 的效果对比
    /// </summary>
    [Test]
    public void AsImgs_FileCallback_WithSettings_DpiComparison()
    {
        using var document = new PdfDocument(TestPdfPath);

        // 低 DPI (72 = PDF 原始尺寸)
        document.AsImgs(
            i => Path.Combine(ShowcaseDir, $"4a_dpi72_page{i}.png"),
            new ImageGenerationSettings { RasterDpi = 72, ImageFormat = ImageFormat.Png });

        // 高 DPI (288 = 4x 原始尺寸,默认值)
        document.AsImgs(
            i => Path.Combine(ShowcaseDir, $"4b_dpi288_page{i}.png"),
            new ImageGenerationSettings { RasterDpi = 288, ImageFormat = ImageFormat.Png });

        // 对比第一页的文件大小(DPI 越高文件越大)
        string lowDpi = Path.Combine(ShowcaseDir, "4a_dpi72_page0.png");
        string highDpi = Path.Combine(ShowcaseDir, "4b_dpi288_page0.png");

        Assert.That(File.Exists(lowDpi) && File.Exists(highDpi), Is.True);
        long lowSize = new FileInfo(lowDpi).Length;
        long highSize = new FileInfo(highDpi).Length;
        TestContext.WriteLine($"  [DPI 对比] DPI72={lowSize:N0} 字节  vs  DPI288={highSize:N0} 字节  (比值 {highSize / (double)lowSize:F2}x)");
        Assert.That(highSize, Is.GreaterThan(lowSize), "高 DPI 应产生更大的文件");
    }

    /// <summary>
    /// 汇总:列出 showcase 目录所有产物,方便人工查看
    /// </summary>
    [OneTimeTearDown]
    public void Teardown()
    {
        TestContext.WriteLine();
        TestContext.WriteLine("=== showcase 目录最终产物 ===");
        if (Directory.Exists(ShowcaseDir))
        {
            foreach (string file in Directory.GetFiles(ShowcaseDir))
            {
                var info = new FileInfo(file);
                TestContext.WriteLine($"  {info.Name,-35} {info.Length,10:N0} 字节");
            }
            TestContext.WriteLine($"=== 完整路径: {ShowcaseDir} ===");
        }
    }
}
