using PDFiumZ;
using PDFiumZ.HighLevel;
using PDFiumZ.Fluent.Document;
using PDFiumZ.Fluent;
using PDFiumZ.Fluent.Elements;
using System;
using System.IO;

namespace FluentApiExample;

/// <summary>
/// PDFiumZ Fluent API 完整示例
/// 演示 QuestPDF 风格的声明式 PDF 文档生成
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== PDFiumZ Fluent API 示例 ===\n");

        // 初始化 PDFium 库
        Console.WriteLine("初始化 PDFium 库...");
        PdfiumLibrary.Initialize();
        Console.WriteLine("✓ PDFium 库初始化成功\n");

        try
        {
            // ============================================
            // 示例 1: 简单文本文档
            // ============================================
            Console.WriteLine("示例 1: 创建简单文本文档...");
            CreateSimpleDocument();
            Console.WriteLine("✓ simple-document.pdf 已生成\n");

            // ============================================
            // 示例 2: 复杂布局文档
            // ============================================
            Console.WriteLine("示例 2: 创建复杂布局文档...");
            CreateComplexLayout();
            Console.WriteLine("✓ complex-layout.pdf 已生成\n");

            // ============================================
            // 示例 3: 样式化文档
            // ============================================
            Console.WriteLine("示例 3: 创建样式化文档...");
            CreateStyledDocument();
            Console.WriteLine("✓ styled-document.pdf 已生成\n");

            // ============================================
            // 示例 4: 表单和发票样式文档
            // ============================================
            Console.WriteLine("示例 4: 创建发票样式文档...");
            CreateInvoice();
            Console.WriteLine("✓ invoice.pdf 已生成\n");

            Console.WriteLine("=== 所有示例完成 ===");
            Console.WriteLine("\n生成的文件:");
            Console.WriteLine("  - simple-document.pdf");
            Console.WriteLine("  - complex-layout.pdf");
            Console.WriteLine("  - styled-document.pdf");
            Console.WriteLine("  - invoice.pdf");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n错误: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
        finally
        {
            // 清理资源
            Console.WriteLine("\n清理资源...");
            PdfiumLibrary.Shutdown();
            Console.WriteLine("✓ PDFium 库已关闭");
        }
    }

    /// <summary>
    /// 示例 1: 创建简单的文本文档
    /// </summary>
    static void CreateSimpleDocument()
    {
        using var document = new FluentDocument();

        document.Content(container =>
        {
            container.Column(column =>
            {
                // 标题
                column.Item().Text("欢迎使用 PDFiumZ Fluent API")
                    .WithFontSize(24)
                    .WithColor(0x1E3A8A); // 深蓝色

                // 副标题
                column.Item().Text("这是一个简单的示例文档")
                    .WithFontSize(14)
                    .WithColor(0x6B7280); // 灰色

                // 内容段落
                column.Item().Text(
                    "PDFiumZ 提供了类似 QuestPDF 的声明式 API，" +
                    "让您能够轻松创建专业的 PDF 文档。" +
                    "Fluent API 支持流式布局、容器嵌套、样式定制等功能。"
                ).WithFontSize(12).WithColor(0x000000);

                // 列表项
                column.Item().Text("主要特性:")
                    .WithFontSize(16)
                    .WithColor(0x1E3A8A);

                column.Item().Text("  • 声明式 API 设计")
                    .WithFontSize(12);
                column.Item().Text("  • 流式布局系统")
                    .WithFontSize(12);
                column.Item().Text("  • 丰富的样式选项")
                    .WithFontSize(12);
                column.Item().Text("  • 容器和嵌套支持")
                    .WithFontSize(12);
            });
        });

        document.Generate();
        document.Save("simple-document.pdf");
    }

    /// <summary>
    /// 示例 2: 创建复杂布局文档（列和行）
    /// </summary>
    static void CreateComplexLayout()
    {
        using var document = new FluentDocument();

        document.Content(container =>
        {
            container.Column(column =>
            {
                // 页眉
                column.Item().Row(row =>
                {
                    row.Item().Expand().Text("复杂布局示例")
                        .WithFontSize(20)
                        .WithColor(0xFFFFFF);

                    row.Item().Text($"{DateTime.Now:yyyy-MM-dd}")
                        .WithFontSize(12)
                        .WithColor(0xFFFFFF);
                }).Background(0x1E3A8A).Padding(20);

                column.Item().Text(" ").WithFontSize(10); // 空行

                // 两列布局
                column.Item().Row(row =>
                {
                    // 左列
                    row.Item().Column(col =>
                    {
                        col.Item().Text("左侧内容")
                            .WithFontSize(16)
                            .WithColor(0x1E3A8A)
                            .Background(0xE0F2FE)
                            .Padding(10);

                        col.Item().Text(
                            "这是左侧栏的内容。" +
                            "Fluent API 支持灵活的列和行布局，" +
                            "可以轻松创建多列文档结构。"
                        ).WithFontSize(12)
                        .Padding(10);
                    }).Width().Expand();

                    // 右列
                    row.Item().Column(col =>
                    {
                        col.Item().Text("右侧内容")
                            .WithFontSize(16)
                            .WithColor(0x1E3A8A)
                            .Background(0xE0F2FE)
                            .Padding(10);

                        col.Item().Text(
                            "这是右侧栏的内容。" +
                            "您可以自由调整列的宽度和间距，" +
                            "创建响应式布局。"
                        ).WithFontSize(12)
                        .Padding(10);
                    }).Width().Expand();
                });

                column.Item().Text(" ").WithFontSize(10); // 空行

                // 三列布局
                column.Item().Row(row =>
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        row.Item().Column(col =>
                        {
                            col.Item().Text($"列 {i}")
                                .WithFontSize(14)
                                .AlignCenter()
                                .Background(0xF3E8FF)
                                .Padding(10);

                            col.Item().Text(
                                $"这是第 {i} 列的内容。" +
                                "多列布局非常适合展示卡片、" +
                                "产品列表或统计数据。"
                            ).WithFontSize(10)
                            .Padding(10);
                        }).Width().Expand();
                    }
                });
            });
        });

        document.Generate();
        document.Save("complex-layout.pdf");
    }

    /// <summary>
    /// 示例 3: 创建样式化文档（边框、背景、填充）
    /// </summary>
    static void CreateStyledDocument()
    {
        using var document = new FluentDocument();

        document.Content(container =>
        {
            container.Column(20, column => // 20pt 间距
            {
                // 标题卡片 - 背景色 + 边框
                column.Item().Text("样式化文档示例")
                    .WithFontSize(24)
                    .WithColor(0xFFFFFF)
                    .Background(0x7C3AED) // 紫色背景
                    .Border(0x4C1D95, 2) // 深紫色边框
                    .Padding(30)
                    .AlignCenter();

                // 信息卡片 - 蓝色主题
                column.Item().Column(col =>
                {
                    col.Item().Text("信息卡片")
                        .WithFontSize(18)
                        .WithColor(0x1E3A8A);

                    col.Item().Text(
                        "这个卡片展示了如何组合使用背景、" +
                        "边框和填充来创建视觉层次。"
                    ).WithFontSize(12);
                })
                .Background(0xDBEAFE) // 浅蓝色背景
                .Border(0x3B82F6, 1) // 蓝色边框
                .Padding(20);

                // 警告卡片 - 黄色主题
                column.Item().Column(col =>
                {
                    col.Item().Text("⚠️ 注意事项")
                        .WithFontSize(18)
                        .WithColor(0x92400E);

                    col.Item().Text(
                        "Fluent API 提供了丰富的样式选项，" +
                        "包括颜色、边框、填充、对齐等，" +
                        "让您能够创建精美的文档。"
                    ).WithFontSize(12);
                })
                .Background(0xFEF3C7) // 浅黄色背景
                .Border(0xF59E0B, 2) // 橙色边框
                .Padding(20);

                // 成功卡片 - 绿色主题
                column.Item().Column(col =>
                {
                    col.Item().Text("✓ 成功提示")
                        .WithFontSize(18)
                        .WithColor(0x065F46);

                    col.Item().Text(
                        "您可以通过组合不同的样式元素，" +
                        "创建符合您品牌风格的文档模板。"
                    ).WithFontSize(12);
                })
                .Background(0xD1FAE5) // 浅绿色背景
                .Border(0x10B981, 2) // 绿色边框
                .Padding(20);

                // 错误卡片 - 红色主题
                column.Item().Column(col =>
                {
                    col.Item().Text("✗ 错误提示")
                        .WithFontSize(18)
                        .WithColor(0x991B1B);

                    col.Item().Text(
                        "样式可以应用于任何元素，" +
                        "包括文本、容器、布局等。"
                    ).WithFontSize(12);
                })
                .Background(0xFEE2E2) // 浅红色背景
                .Border(0xEF4444, 2) // 红色边框
                .Padding(20);
            });
        });

        document.Generate();
        document.Save("styled-document.pdf");
    }

    /// <summary>
    /// 示例 4: 创建发票样式文档
    /// </summary>
    static void CreateInvoice()
    {
        using var document = new FluentDocument();

        document.Content(container =>
        {
            container.Column(15, column =>
            {
                // ============================================
                // 页眉 - 公司名称和 Logo 占位
                // ============================================
                column.Item().Row(row =>
                {
                    row.Item().Column(col =>
                    {
                        col.Item().Text("ACME 公司")
                            .WithFontSize(28)
                            .WithColor(0x1E3A8A)
                            .Bold();

                        col.Item().Text("123 技术大街")
                            .WithFontSize(12)
                            .WithColor(0x6B7280);
                        col.Item().Text("创新区, 科技城 10001")
                            .WithFontSize(12)
                            .WithColor(0x6B7280);
                        col.Item().Text("电话: (555) 123-4567")
                            .WithFontSize(12)
                            .WithColor(0x6B7280);
                    });

                    row.Item().Column(col =>
                    {
                        col.Item().Text("发票")
                            .WithFontSize(32)
                            .WithColor(0xEF4444)
                            .AlignRight();

                        col.Item().Text($"发票编号: INV-2024-001")
                            .WithFontSize(12)
                            .WithColor(0x6B7280)
                            .AlignRight();
                        col.Item().Text($"日期: {DateTime.Now:yyyy年MM月dd日}")
                            .WithFontSize(12)
                            .WithColor(0x6B7280)
                            .AlignRight();
                    }).AlignRight();
                });

                column.Item().Text(" ").WithFontSize(10);

                // ============================================
                // 客户信息
                // ============================================
                column.Item().Column(col =>
                {
                    col.Item().Text("账单接收人:")
                        .WithFontSize(14)
                        .WithColor(0x1E3A8A)
                        .Bold();

                    col.Item().Text("张三")
                        .WithFontSize(12)
                        .WithColor(0x374151);
                    col.Item().Text("客户公司 Ltd.")
                        .WithFontSize(12)
                        .WithColor(0x374151);
                    col.Item().Text("商务路 456 号")
                        .WithFontSize(12)
                        .WithColor(0x374151);
                    col.Item().Text("商业区, 科技城 10002")
                        .WithFontSize(12)
                        .WithColor(0x374151);
                }).Background(0xF3F4F6).Padding(15);

                column.Item().Text(" ").WithFontSize(10);

                // ============================================
                // 发票明细表格
                // ============================================
                column.Item().Column(col =>
                {
                    // 表头
                    col.Item().Row(row =>
                    {
                        row.Item().Text("项目描述")
                            .WithFontSize(12)
                            .WithColor(0xFFFFFF)
                            .Bold()
                            .Padding(10).Expand();

                        row.Item().Text("数量")
                            .WithFontSize(12)
                            .WithColor(0xFFFFFF)
                            .Bold()
                            .Padding(10).Width(80);

                        row.Item().Text("单价")
                            .WithFontSize(12)
                            .WithColor(0xFFFFFF)
                            .Bold()
                            .Padding(10).Width(100);

                        row.Item().Text("金额")
                            .WithFontSize(12)
                            .WithColor(0xFFFFFF)
                            .Bold()
                            .Padding(10).Width(100);
                    }).Background(0x1E3A8A);

                    // 数据行 1
                    col.Item().Row(row =>
                    {
                        row.Item().Text("PDFiumZ 企业许可证 (年度)")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Padding(10).Expand();

                        row.Item().Text("1")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Padding(10).Width(80);

                        row.Item().Text("¥ 2,999.00")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Padding(10).Width(100);

                        row.Item().Text("¥ 2,999.00")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Bold()
                            .Padding(10).Width(100);
                    }).Background(row.Item().Index % 2 == 0 ? 0xFFFFFF : 0xF9FAFB);

                    // 数据行 2
                    col.Item().Row(row =>
                    {
                        row.Item().Text("技术支持服务 (10小时)")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Padding(10).Expand();

                        row.Item().Text("10")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Padding(10).Width(80);

                        row.Item().Text("¥ 150.00")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Padding(10).Width(100);

                        row.Item().Text("¥ 1,500.00")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Bold()
                            .Padding(10).Width(100);
                    }).Background(0xF9FAFB);

                    // 数据行 3
                    col.Item().Row(row =>
                    {
                        row.Item().Text("定制开发服务")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Padding(10).Expand();

                        row.Item().Text("1")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Padding(10).Width(80);

                        row.Item().Text("¥ 5,000.00")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Padding(10).Width(100);

                        row.Item().Text("¥ 5,000.00")
                            .WithFontSize(11)
                            .WithColor(0x374151)
                            .Bold()
                            .Padding(10).Width(100);
                    }).Background(0xFFFFFF);
                }).Border(0xE5E7EB, 1);

                column.Item().Text(" ").WithFontSize(10);

                // ============================================
                // 总计
                // ============================================
                column.Item().Row(row =>
                {
                    row.Item().Expand(); // 占位符

                    row.Item().Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.Item().Text("小计:")
                                .WithFontSize(12)
                                .WithColor(0x6B7280)
                                .Padding(5).Width(100);

                            r.Item().Text("¥ 9,499.00")
                                .WithFontSize(12)
                                .WithColor(0x374151)
                                .Padding(5).Width(100);
                        });

                        col.Item().Row(r =>
                        {
                            r.Item().Text("税费 (10%):")
                                .WithFontSize(12)
                                .WithColor(0x6B7280)
                                .Padding(5).Width(100);

                            r.Item().Text("¥ 949.90")
                                .WithFontSize(12)
                                .WithColor(0x374151)
                                .Padding(5).Width(100);
                        });

                        col.Item().Row(r =>
                        {
                            r.Item().Text("总计:")
                                .WithFontSize(14)
                                .WithColor(0xFFFFFF)
                                .Bold()
                                .Background(0x1E3A8A)
                                .Padding(10).Width(100);

                            r.Item().Text("¥ 10,448.90")
                                .WithFontSize(14)
                                .WithColor(0xFFFFFF)
                                .Bold()
                                .Background(0x1E3A8A)
                                .Padding(10).Width(100);
                        });
                    });
                });

                column.Item().Text(" ").WithFontSize(20);

                // ============================================
                // 页脚 - 备注
                // ============================================
                column.Item().Column(col =>
                {
                    col.Item().Text("备注:")
                        .WithFontSize(12)
                        .WithColor(0x1E3A8A)
                        .Bold();

                    col.Item().Text(
                        "1. 付款期限: 收到发票后 30 天内" + Environment.NewLine +
                        "2. 付款方式: 银行转账或在线支付" + Environment.NewLine +
                        "3. 如有疑问，请联系我们的客服团队"
                    ).WithFontSize(10)
                    .WithColor(0x6B7280);
                }).Background(0xFFF7ED).Border(0xFDBA74, 1).Padding(15);

                column.Item().Text(" ").WithFontSize(10);

                // 签名区域
                column.Item().Row(row =>
                {
                    row.Item().Column(col =>
                    {
                        col.Item().Text("_____________________")
                            .WithFontSize(12)
                            .WithColor(0x9CA3AF);

                        col.Item().Text("授权签名")
                            .WithFontSize(10)
                            .WithColor(0x6B7280);
                    });

                    row.Item().Expand();

                    row.Item().Column(col =>
                    {
                        col.Item().Text("_____________________")
                            .WithFontSize(12)
                            .WithColor(0x9CA3AF);

                        col.Item().Text("客户签名")
                            .WithFontSize(10)
                            .WithColor(0x6B7280);
                    });
                });
            });
        });

        document.Generate(pageWidth: 595, pageHeight: 842); // A4
        document.Save("invoice.pdf");
    }
}

/// <summary>
/// 文本元素扩展方法
/// </summary>
public static class TextExtensions
{
    public static TextElement Bold(this TextElement element)
    {
        // 在实际实现中，这里会设置粗体字体
        return element;
    }
}
