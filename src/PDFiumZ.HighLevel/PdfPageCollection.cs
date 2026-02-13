using System;
using System.Collections;
using System.Collections.Generic;

namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF 页面集合
/// </summary>
public class PdfPageCollection : IList<PdfPage>
{
    private readonly PdfDocument _document;
    private readonly List<PdfPage> _pages;

    internal PdfPageCollection(PdfDocument document)
    {
        _document = document;
        _pages = new List<PdfPage>();
    }

    public PdfPage this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return new PdfPage(_document, index);
        }
        set => throw new NotSupportedException();
    }

    public int Count => _document.PageCount;

    public bool IsReadOnly => true;

    public void Add(PdfPage item) => throw new NotSupportedException();
    public void Clear() => throw new NotSupportedException();
    public bool Contains(PdfPage item) => false;
    public void CopyTo(PdfPage[] array, int arrayIndex) { }
    public IEnumerator<PdfPage> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
            yield return this[i];
    }
    public int IndexOf(PdfPage item) => -1;
    public void Insert(int index, PdfPage item) => throw new NotSupportedException();
    public bool Remove(PdfPage item) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
