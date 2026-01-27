# PDFiumZ Fluent API 示例

本目录包含 PDFiumZ Fluent API 的完整示例，演示如何使用声明式 API 创建专业的 PDF 文档。

## 📋 目录

- [简介](#简介)
- [示例概览](#示例概览)
- [运行示例](#运行示例)
- [示例说明](#示例说明)
- [Fluent API 核心概念](#fluent-api-核心概念)
- [进阶技巧](#进阶技巧)

## 简介

PDFiumZ Fluent API 是一个类似 QuestPDF 的声明式 PDF 文档生成 API。它提供了：

- ✅ **流式布局系统** - 使用 Column 和 Row 构建复杂布局
- ✅ **丰富的样式选项** - 颜色、边框、填充、对齐等
- ✅ **容器嵌套** - 无限层次的容器组合
- ✅ **链式调用** - 流畅的 API 设计
- ✅ **类型安全** - 完全的 C# 类型支持

## 示例概览

本示例包含 4 个完整的演示：

1. **简单文本文档** (`simple-document.pdf`)
   - 基础文本和样式
   - 列表和标题
   - 颜色和字体设置

2. **复杂布局文档** (`complex-layout.pdf`)
   - 多列布局
   - 行和列的嵌套
   - 页眉和内容区域

3. **样式化文档** (`styled-document.pdf`)
   - 背景色和边框
   - 内边距和外边距
   - 不同主题的卡片设计

4. **发票样式文档** (`invoice.pdf`)
   - 完整的商业发票模板
   - 表格布局
   - 对齐和间距控制
   - 页眉、页脚和签名区域

## 运行示例

### 前置要求

- .NET 8.0 SDK 或更高版本
- PDFiumZ NuGet 包

### 步骤

1. **克隆或下载项目**
   ```bash
   git clone https://github.com/your-repo/PDFiumZ.git
   cd PDFiumZ
   ```

2. **还原 NuGet 包**
   ```bash
   dotnet restore
   ```

3. **编译项目**
   ```bash
   dotnet build src/PDFiumZ.sln -c Release
   ```

4. **运行示例**
   ```bash
   # 英文版
   dotnet run --project examples/en-US/06-FluentAPI/FluentDemo.csproj

   # 中文版
   dotnet run --project examples/zh-CN/06-FluentAPI/FluentDemo.csproj
   ```

5. **查看输出**
   运行后会在当前目录生成 4 个 PDF 文件：
   - `simple-document.pdf`
   - `complex-layout.pdf`
   - `styled-document.pdf`
   - `invoice.pdf`

## 示例说明

### 示例 1: 简单文本文档

```csharp
using var document = new FluentDocument();

document.Content(container =>
{
    container.Column(column =>
    {
        // 标题
        column.Item().Text("欢迎使用 PDFiumZ Fluent API")
            .WithFontSize(24)
            .WithColor(0x1E3A8A);

        // 内容段落
        column.Item().Text("这是内容...")
            .WithFontSize(12);
    });
});

document.Generate();
document.Save("output.pdf");
```

**关键点**:
- 使用 `Content()` 开始定义文档内容
- 使用 `Column()` 创建垂直布局
- 使用 `Item()` 添加列中的项目
- 使用 `Text()` 添加文本
- 使用链式调用设置样式

### 示例 2: 复杂布局

```csharp
container.Column(column =>
{
    // 两列布局
    column.Item().Row(row =>
    {
        // 左列
        row.Item().Column(col => { /* ... */ }).Width().Expand();

        // 右列
        row.Item().Column(col => { /* ... */ }).Width().Expand();
    });

    // 三列布局
    column.Item().Row(row =>
    {
        for (int i = 1; i <= 3; i++)
        {
            row.Item().Column(col => { /* ... */ }).Width().Expand();
        }
    });
});
```

**关键点**:
- 使用 `Row()` 创建水平布局
- 使用 `Width().Expand()` 让列平均分配宽度
- 可以嵌套任意层级的列和行

### 示例 3: 样式化

```csharp
// 背景和边框
element
    .Background(0xDBEAFE)
    .Border(0x3B82F6, 2)
    .Padding(20);

// 对齐
element.AlignCenter();
element.AlignLeft();
element.AlignRight();

// 文本样式
text
    .WithFontSize(14)
    .WithColor(0x1E3A8A)
    .Bold();
```

**关键点**:
- 颜色使用十六进制格式 (0xRRGGBB)
- 边框可以指定颜色和宽度
- 内边距支持统一值或分别设置四个方向
- 提供多种对齐选项

### 示例 4: 发票模板

展示了如何构建一个完整的商业文档：
- 页眉: 公司信息和发票标题
- 客户信息卡片
- 明细表格
- 总计计算区域
- 页脚备注
- 签名区域

## Fluent API 核心概念

### 1. 容器 (Container)

所有内容都从 `Content()` 开始，传入一个 `Action<IContainer>`:

```csharp
document.Content(container =>
{
    // 在这里定义文档内容
});
```

### 2. 列 (Column)

垂直排列元素：

```csharp
container.Column(column =>
{
    column.Item().Text("第一行");
    column.Item().Text("第二行");
    column.Item().Text("第三行");
});
```

可以设置间距：

```csharp
container.Column(20, column => { /* 20pt 间距 */ });
```

### 3. 行 (Row)

水平排列元素：

```csharp
container.Row(row =>
{
    row.Item().Text("左侧");
    row.Item().Text("中间");
    row.Item().Text("右侧");
});
```

### 4. 元素 (Element)

任何可渲染的内容都是元素：

- **文本元素**: `Text()`
- **布局元素**: `Column()`, `Row()`, `Layers()`
- **样式元素**: `Background()`, `Border()`, `Padding()`
- **位置元素**: `Width()`, `Height()`, `Alignment()`

### 5. 链式调用

所有方法都返回对象本身，支持链式调用：

```csharp
text
    .WithFontSize(14)
    .WithColor(0x000000)
    .Background(0xFFFFFF)
    .Border(0x000000, 1)
    .Padding(10);
```

## 进阶技巧

### 1. 创建可重用组件

```csharp
static IElement CreateCard(string title, string content)
{
    return new ColumnElement
    {
        // 标题
        new TextElement(title).WithFontSize(18),
        // 内容
        new TextElement(content).WithFontSize(12)
    }
    .Background(0xF3F4F6)
    .Border(0xE5E7EB, 1)
    .Padding(15);
}

// 使用
column.Item().AddChild(CreateCard("标题", "内容"));
```

### 2. 动态内容生成

```csharp
column.Item().Row(row =>
{
    foreach (var item in items)
    {
        row.Item().Text(item.Name).Expand();
        row.Item().Text(item.Price.ToString("C"));
    }
});
```

### 3. 条件渲染

```csharp
if (showHeader)
{
    column.Item().Text("页眉").WithFontSize(24);
}

column.Item().Text("内容");
```

### 4. 自定义样式扩展

```csharp
public static class CustomStyles
{
    public static IContainer InfoCard(this IContainer container, string text)
    {
        return container
            .Column(col => col.Item().Text(text))
            .Background(0xDBEAFE)
            .Border(0x3B82F6, 1)
            .Padding(20);
    }
}

// 使用
container.InfoCard("这是一条信息");
```

### 5. 响应式布局

```csharp
column.Item().Row(row =>
{
    // 主内容区域 (70%)
    row.Item().Column(col => { /* 主内容 */ })
        .Width(0.7);

    // 侧边栏 (30%)
    row.Item().Column(col => { /* 侧边栏 */ })
        .Width(0.3);
});
```

## 性能优化建议

1. **复用文档对象**: 对于批量生成，重用 FluentDocument 实例
2. **避免过深嵌套**: 嵌套层级控制在 5 层以内
3. **使用间距参数**: 优先使用 Column(spacing, items) 而不是手动添加空白元素
4. **延迟渲染**: 只在需要时调用 `Generate()`

## 常见问题

### Q: 如何添加图片？

A: 当前版本可以使用 ImageElement:

```csharp
column.Item().AddChild(new ImageElement("path/to/image.png"));
```

### Q: 如何分页？

A: 每次调用 `Generate()` 会创建新页面:

```csharp
document.Generate(); // 第1页
// 添加更多内容
document.Generate(); // 第2页
```

### Q: 如何设置页边距？

A: 使用容器包装内容并设置 Padding:

```csharp
container.Column(column =>
{
    column.Item().Padding(50).Column(inner =>
    {
        // 内容带有 50pt 页边距
    });
});
```

### Q: 如何添加页码？

A: 在每页底部添加页码元素:

```csharp
column.Item().Text($"第 {currentPage} 页")
    .AlignCenter()
    .WithFontSize(10);
```

## 更多资源

- **完整文档**: `.workflow/docs/PDFiumZ/`
- **API 参考**: `.workflow/docs/PDFiumZ/src/PDFiumZ/API.md`
- **更多示例**: `examples/` 目录

## 贡献

欢迎提交问题和改进建议！

---

**作者**: PDFiumZ Team
**许可证**: Apache-2.0
**最后更新**: 2024-01-22
