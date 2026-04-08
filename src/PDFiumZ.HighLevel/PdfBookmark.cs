using System;
using System.Collections;
using System.Collections.Generic;
using PDFiumZ;

namespace PDFiumZ.HighLevel;

/// <summary>
/// 表示 PDF 文档中的一个书签条目
/// </summary>
public class PdfBookmark
{
    private readonly FpdfBookmarkT _handle;
    private readonly PdfDocument _document;

    internal PdfBookmark(FpdfBookmarkT handle, PdfDocument document)
    {
        _handle = handle;
        _document = document;
    }

    /// <summary>
    /// 书签标题
    /// </summary>
    public string Title
    {
        get
        {
            ulong len = fpdf_doc.FPDFBookmarkGetTitle(_handle, IntPtr.Zero, 0);
            if (len <= 2)
                return string.Empty;

            var buffer = new byte[len];
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    fpdf_doc.FPDFBookmarkGetTitle(_handle, new IntPtr(ptr), len);
                }
            }

            // UTF-16LE 编码，跳过末尾 null 终止符
            return System.Text.Encoding.Unicode.GetString(buffer, 0, (int)len - 2);
        }
    }

    /// <summary>
    /// 书签目标的页面索引，如果无目标返回 -1
    /// </summary>
    public int DestPageIndex
    {
        get
        {
            FpdfDestT dest = fpdf_doc.FPDFBookmarkGetDest(_document.Handle, _handle);
            if (dest == null)
                return -1;

            return fpdf_doc.FPDFDestGetDestPageIndex(_document.Handle, dest);
        }
    }

    /// <summary>
    /// 子书签数量（仅直接子项）
    /// </summary>
    public int Count => fpdf_doc.FPDFBookmarkGetCount(_handle);

    /// <summary>
    /// 获取第一个子书签
    /// </summary>
    public PdfBookmark? FirstChild
    {
        get
        {
            FpdfBookmarkT child = fpdf_doc.FPDFBookmarkGetFirstChild(_document.Handle, _handle);
            return child != null ? new PdfBookmark(child, _document) : null;
        }
    }

    /// <summary>
    /// 获取下一个兄弟书签
    /// </summary>
    public PdfBookmark? NextSibling
    {
        get
        {
            FpdfBookmarkT sibling = fpdf_doc.FPDFBookmarkGetNextSibling(_document.Handle, _handle);
            return sibling != null ? new PdfBookmark(sibling, _document) : null;
        }
    }

    /// <summary>
    /// 递归遍历所有子书签（深度优先）
    /// </summary>
    public IEnumerable<PdfBookmark> GetChildren()
    {
        var child = FirstChild;
        while (child != null)
        {
            yield return child;
            foreach (var descendant in child.GetChildren())
                yield return descendant;
            child = child.NextSibling;
        }
    }
}

/// <summary>
/// PDF 文档书签集合，提供对文档大纲树的访问
/// </summary>
public class PdfBookmarkCollection : IEnumerable<PdfBookmark>
{
    private readonly PdfDocument _document;

    internal PdfBookmarkCollection(PdfDocument document)
    {
        _document = document;
    }

    /// <summary>
    /// 获取第一个顶层书签
    /// </summary>
    public PdfBookmark? FirstChild
    {
        get
        {
            FpdfBookmarkT root = fpdf_doc.FPDFBookmarkGetFirstChild(_document.Handle, null);
            return root != null ? new PdfBookmark(root, _document) : null;
        }
    }

    public IEnumerator<PdfBookmark> GetEnumerator()
    {
        var bookmark = FirstChild;
        while (bookmark != null)
        {
            yield return bookmark;
            bookmark = bookmark.NextSibling;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
