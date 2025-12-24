# PDFiumZ 开发计划 v146.0.0

> 制定时间: 2025-12-24
> 基于版本: v145.0.7592.2
> 目标版本: v146.0.0

## 📊 项目现状分析

### 已完成功能 (v145.0.7592.2)

#### 核心文档操作 ✅
- 创建、打开、保存 PDF 文档
- PDF 合并、拆分（Merge, Split）
- 页面管理（创建、删除、旋转）
- 水印功能（文本水印，支持透明度和旋转）

#### 内容渲染与提取 ✅
- 高性能 PDF 页面渲染为图像
- 文本提取（纯文本和带位置信息）
- 图像提取（从 PDF 页面提取嵌入图像）
- 文本搜索功能

#### 注解系统 ✅ (10+ 类型)
- 文本标记：Highlight, Underline, StrikeOut
- 形状：Square, Circle, Line
- 文本：Text (便签), FreeText (文本框)
- 绘图：Ink (手绘)
- 其他：Stamp (图章)

#### 表单与交互 ✅
- 读取表单字段
- 填写表单字段值
- 表单字段类型识别

#### 内容创建 ✅
- 低级 API：PdfContentEditor（文本、图像、形状、路径）
- 高级 API：Fluent API（类 QuestPDF 声明式文档生成）
- HTML 转 PDF（基础 HTML/CSS 支持）
- 表格生成器

#### 安全与权限 ✅
- 读取加密状态
- 读取文档权限（打印、修改、复制等）
- **限制**：PDFium 不支持写入加密和密码保护

#### 书签与导航 🔶 (部分完成)
- ✅ 读取书签层次结构
- ✅ 获取书签标题和目的地
- ❌ 创建和修改书签
- ❌ 书签操作（添加、删除、移动）

#### 元数据 🔶 (只读)
- ✅ 读取标准元数据（标题、作者、主题、关键词、创建者、创建日期等）
- ❌ 写入/修改元数据

#### 链接 🔶 (基础支持)
- ✅ 读取页面链接
- ✅ 获取链接目的地
- ❌ 创建内部/外部链接
- ❌ 页面跳转链接

---

## 🎯 迫切功能需求分析

基于对 .NET PDF 处理场景的分析和 ROADMAP 规划，以下是按优先级排序的功能需求：

### 优先级 P0 - 核心缺失功能（高频使用场景）

#### 1. 书签功能完善 ⭐⭐⭐
**用户场景**：
- 企业文档管理：自动生成目录书签
- 报告生成：章节导航
- 电子书制作：目录结构

**当前状态**：只读支持
**需求**：
- ✅ 读取书签（已完成）
- ❌ 创建书签
- ❌ 添加/删除书签
- ❌ 修改书签标题和目的地
- ❌ 设置书签层级结构

**工作量评估**：3-4 天
**技术难度**：中
**用户价值**：★★★★★

#### 2. 元数据读写 ⭐⭐⭐
**用户场景**：
- 文档管理系统：自动标记文档属性
- 归档系统：元数据搜索和分类
- 合规性：添加作者、版本信息

**当前状态**：只读支持
**需求**：
- ✅ 读取元数据（已完成）
- ❌ 写入标准元数据字段
- ❌ 修改现有元数据
- ❌ 自定义元数据字段

**工作量评估**：2-3 天
**技术难度**：低-中
**用户价值**：★★★★★

#### 3. 链接创建功能 ⭐⭐⭐
**用户场景**：
- 交互式文档：目录链接跳转
- 在线文档：添加外部链接
- 参考文献：快速跳转

**当前状态**：只读支持
**需求**：
- ✅ 读取链接（已完成）
- ❌ 创建内部页面跳转链接
- ❌ 创建外部 URL 链接
- ❌ 设置链接样式（边框、高亮）
- ❌ 删除/修改链接

**工作量评估**：3-4 天
**技术难度**：中
**用户价值**：★★★★☆

### 优先级 P1 - 增强功能（提升易用性）

#### 4. 页面标签 (Page Labels) ⭐⭐
**用户场景**：
- 多部分文档：前言用罗马数字，正文用阿拉伯数字
- 复杂报告：不同章节使用不同页码格式

**需求**：
- 读取页面标签
- 设置页面标签格式
- 支持罗马数字、字母、阿拉伯数字等

**工作量评估**：2-3 天
**技术难度**：中
**用户价值**：★★★☆☆

#### 5. 附件功能 ⭐⭐
**用户场景**：
- 档案管理：PDF 中嵌入原始文件
- 技术文档：附加源代码文件
- 法律文件：附加证据材料

**需求**：
- 读取嵌入附件列表
- 提取附件文件
- 添加附件到 PDF
- 删除附件

**工作量评估**：3-4 天
**技术难度**：中
**用户价值**：★★★☆☆

#### 6. 页面缩略图 ⭐⭐
**用户场景**：
- PDF 查看器：快速预览导航
- 文档管理：缩略图展示
- 页面选择器

**需求**：
- 生成页面缩略图
- 自定义缩略图尺寸
- 批量生成缩略图
- 缩略图缓存优化

**工作量评估**：2-3 天
**技术难度**：低-中
**用户价值**：★★★★☆

### 优先级 P2 - 专业功能（长期目标）

#### 7. 数字签名 ⭐
**用户场景**：
- 电子合同：数字签名验证
- 公文管理：签名认证
- 合规性：签名验证

**限制**：PDFium 对数字签名支持有限
**需求**：
- 读取签名信息
- 验证签名有效性
- ⚠️ 添加数字签名（可能需要第三方库）

**工作量评估**：7-10 天
**技术难度**：高
**用户价值**：★★★☆☆

#### 8. PDF/A 合规性 ⭐
**用户场景**：
- 长期归档
- 合规性要求
- 政府文档

**需求**：
- 检测 PDF/A 合规性
- 转换为 PDF/A 格式
- 合规性报告

**工作量评估**：5-7 天
**技术难度**：高
**用户价值**：★★☆☆☆

#### 9. 文档比较 ⭐
**用户场景**：
- 版本对比
- 差异检测
- 审计跟踪

**需求**：
- 文本差异比较
- 视觉差异比较
- 差异报告生成

**工作量评估**：5-7 天
**技术难度**：高
**用户价值**：★★★☆☆

### 优先级 P3 - 性能与质量优化

#### 10. 性能优化 ⭐⭐
**需求**：
- 大文件处理优化
- 内存使用优化
- 渲染性能提升
- 并发处理支持

**工作量评估**：持续进行
**技术难度**：中-高
**用户价值**：★★★★☆

#### 11. 测试覆盖率提升 ⭐⭐
**当前状态**：60+ 单元测试
**目标**：80% 覆盖率

**需求**：
- 增加边缘情况测试
- 集成测试
- 性能回归测试
- 跨平台测试

**工作量评估**：持续进行
**技术难度**：中
**用户价值**：★★★★☆

---

## 📋 v146.0.0 开发计划

### 第一阶段：核心功能补全 (3-4 周)

#### Sprint 1: 书签功能完善 (1-1.5 周)
**目标**：完整的书签读写支持

**任务清单**：
1. 调研 PDFium 书签 API
   - `FPDFBookmark_Create`
   - `FPDFBookmark_SetTitle`
   - `FPDFBookmark_SetDestination`
   - `FPDFBookmark_Delete`

2. 实现书签创建 API
   ```csharp
   // 设计草案
   public class PdfBookmark
   {
       // 现有只读属性
       public string Title { get; }
       public PdfDestination Destination { get; }
       public PdfBookmark? FirstChild { get; }
       public PdfBookmark? NextSibling { get; }

       // 新增写入方法
       public static PdfBookmark Create(PdfDocument document, string title, PdfDestination destination);
       public void SetTitle(string title);
       public void SetDestination(PdfDestination destination);
       public PdfBookmark AddChild(string title, PdfDestination destination);
       public void Delete();
   }
   ```

3. 书签辅助类
   ```csharp
   public class PdfBookmarkBuilder
   {
       public PdfBookmarkBuilder AddBookmark(string title, int pageIndex);
       public PdfBookmarkBuilder AddChildBookmark(string title, int pageIndex);
       public PdfBookmarkBuilder Parent();
       public void Apply(PdfDocument document);
   }
   ```

4. 单元测试
   - 创建书签
   - 修改书签
   - 删除书签
   - 层级结构测试
   - 大文档书签性能测试

5. 文档和示例
   - 更新 docs/README.md
   - 添加书签示例代码

**交付物**：
- ✅ 完整书签读写 API
- ✅ 单元测试覆盖
- ✅ 文档和示例
- ✅ CHANGELOG 更新

#### Sprint 2: 元数据读写 (0.5-1 周)
**目标**：完整的元数据读写支持

**任务清单**：
1. 调研 PDFium 元数据 API
   - `FPDF_GetMetaText`
   - `FPDF_SetMetaText`

2. 扩展 PdfMetadata 类
   ```csharp
   public class PdfMetadata
   {
       // 现有只读属性
       public string Title { get; }
       public string Author { get; }
       // ...

       // 新增写入方法
       public void SetTitle(string title);
       public void SetAuthor(string author);
       public void SetSubject(string subject);
       public void SetKeywords(string keywords);
       public void SetCreator(string creator);
       public void SetProducer(string producer);

       // 自定义元数据
       public string? GetCustomMetadata(string key);
       public void SetCustomMetadata(string key, string value);
   }
   ```

3. 单元测试
   - 设置标准元数据
   - 修改现有元数据
   - 自定义元数据
   - 元数据持久化测试

4. 文档更新

**交付物**：
- ✅ 元数据读写 API
- ✅ 单元测试
- ✅ 文档更新

#### Sprint 3: 链接创建功能 (1-1.5 周)
**目标**：支持创建内部和外部链接

**任务清单**：
1. 调研 PDFium 链接 API
   - `FPDFLink_Create`
   - `FPDFLink_SetAction`
   - `FPDFLink_SetBorderStyle`

2. 扩展 PdfLink 类
   ```csharp
   public class PdfLink
   {
       // 现有只读属性
       public PdfRectangle Bounds { get; }
       public PdfLinkDestination? Destination { get; }

       // 新增创建方法
       public static PdfLink CreateInternalLink(
           PdfPage page,
           PdfRectangle bounds,
           int targetPageIndex);

       public static PdfLink CreateExternalLink(
           PdfPage page,
           PdfRectangle bounds,
           string url);

       public void SetBorderStyle(
           PdfColor color,
           float width,
           PdfBorderStyle style);

       public void Delete();
   }
   ```

3. 链接构建器
   ```csharp
   public class PdfLinkBuilder
   {
       public PdfLinkBuilder AddInternalLink(PdfRectangle bounds, int pageIndex);
       public PdfLinkBuilder AddUrlLink(PdfRectangle bounds, string url);
       public PdfLinkBuilder WithBorder(PdfColor color, float width);
       public void Apply(PdfPage page);
   }
   ```

4. 单元测试
   - 创建内部链接
   - 创建外部链接
   - 设置链接样式
   - 链接导航测试

5. 文档和示例

**交付物**：
- ✅ 链接创建 API
- ✅ 单元测试
- ✅ 文档和示例

### 第二阶段：增强功能 (2-3 周)

#### Sprint 4: 页面标签 (0.5-1 周)
**任务清单**：
1. 调研 `FPDFPageLabel` API
2. 实现 `PdfPageLabel` 类
3. 支持多种页码格式
4. 单元测试和文档

#### Sprint 5: 附件功能 (1-1.5 周)
**任务清单**：
1. 调研 `FPDFAttachment` API
2. 实现附件读取
3. 实现附件写入
4. 单元测试和文档

#### Sprint 6: 页面缩略图 (0.5-1 周)
**任务清单**：
1. 扩展现有渲染 API
2. 添加缩略图生成方法
3. 优化性能（批量生成、缓存）
4. 单元测试和文档

### 第三阶段：质量提升 (持续)

#### 性能优化
- 大文件处理优化
- 内存管理优化
- 并发支持

#### 测试覆盖
- 增加单元测试
- 添加集成测试
- 性能基准测试

#### 文档完善
- API 参考文档
- 最佳实践指南
- 常见问题解答

---

## 🎯 里程碑与时间表

### v146.0.0-beta.1 (4 周后)
**核心功能**：
- ✅ 书签读写
- ✅ 元数据读写
- ✅ 链接创建

### v146.0.0-beta.2 (6 周后)
**增强功能**：
- ✅ 页面标签
- ✅ 附件功能
- ✅ 页面缩略图

### v146.0.0 正式版 (8 周后)
**正式发布**：
- ✅ 所有功能完成
- ✅ 测试覆盖率 > 80%
- ✅ 完整文档
- ✅ 性能优化

---

## 📊 成功指标

### 质量指标
- ✅ 单元测试覆盖率 > 80%
- ✅ 所有测试通过
- ✅ 无已知严重 Bug
- ✅ API 设计一致性审查通过

### 功能指标
- ✅ 书签完整读写支持
- ✅ 元数据完整读写支持
- ✅ 链接创建支持
- ✅ 3 项增强功能完成

### 文档指标
- ✅ API 参考文档完整
- ✅ 所有新功能有示例代码
- ✅ CHANGELOG 更新
- ✅ README 更新

### 社区指标
- ⭐ NuGet 月下载量 > 1000
- ⭐ GitHub Stars > 100
- ⭐ Issue 响应时间 < 48h

---

## 🚧 技术风险与挑战

### 风险 1: PDFium API 限制
**描述**：某些功能可能受 PDFium 底层 API 限制
**缓解措施**：
- 提前调研 PDFium API 文档
- 验证可行性原型
- 准备替代方案

### 风险 2: 向后兼容性
**描述**：API 变更可能影响现有用户
**缓解措施**：
- 遵循语义化版本控制
- 标记 Obsolete 而非直接删除
- 提供迁移指南

### 风险 3: 跨平台兼容性
**描述**：不同平台 PDFium 行为可能不一致
**缓解措施**：
- 跨平台测试
- CI/CD 覆盖 Windows/Linux/macOS
- 平台特定测试用例

---

## 💡 下一步行动

### 立即开始 (本周)
1. ✅ 评审本开发计划
2. 🎯 创建 GitHub Issues 跟踪各 Sprint
3. 🎯 设置 v146.0.0-beta.1 里程碑
4. 🎯 开始 Sprint 1: 书签功能研究和设计

### 近期 (下周)
1. 🎯 完成书签 API 设计
2. 🎯 开始书签功能实现
3. 🎯 准备第一批单元测试

### 中期 (1 个月内)
1. 完成核心功能开发
2. 发布 beta.1 版本
3. 收集用户反馈

---

## 📝 附录

### 参考资源
- PDFium 官方文档: https://pdfium.googlesource.com/pdfium/
- PDFium API 参考: https://pdfium.googlesource.com/pdfium/+/refs/heads/main/public/
- QuestPDF (灵感来源): https://www.questpdf.com/

### 竞品分析
- iTextSharp (商业授权)
- PdfSharp (开源, MIT)
- Aspose.PDF (商业授权)

### 相关 Issue 和讨论
- 待创建 GitHub Issues
- 待收集社区反馈

---

**文档版本**: 1.0
**最后更新**: 2025-12-24
**下次审查**: 每 Sprint 结束后
