using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace PDFiumZ.HighLevel;

/// <summary>
/// Represents an ink annotation (freehand drawing) in a PDF document.
/// </summary>
public sealed unsafe class PdfInkAnnotation : PdfAnnotation
{
    /// <summary>
    /// Creates a new ink annotation on the specified page.
    /// </summary>
    /// <param name="page">The page to add the annotation to.</param>
    /// <param name="paths">A collection of paths, where each path is a collection of points (X, Y).</param>
    /// <param name="color">The ink color in ARGB format (default: solid red).</param>
    /// <param name="width">The stroke width in points (default: 1.0).</param>
    /// <returns>A new <see cref="PdfInkAnnotation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">page or paths is null.</exception>
    /// <exception cref="PdfException">Failed to create annotation.</exception>
    public static PdfInkAnnotation Create(
        PdfPage page,
        IEnumerable<IEnumerable<(double X, double Y)>> paths,
        uint color = 0xFFFF0000,
        double width = 1.0)
    {
        if (page is null)
            throw new ArgumentNullException(nameof(page));
        if (paths is null)
            throw new ArgumentNullException(nameof(paths));

        page.ThrowIfDisposed();

        // Create ink annotation (type 15)
        var handle = fpdf_annot.FPDFPageCreateAnnot(page._handle!, (int)PdfAnnotationType.Ink);
        if (handle is null || handle.__Instance == IntPtr.Zero)
        {
            throw new PdfException("Failed to create ink annotation.");
        }

        var annotation = new PdfInkAnnotation(handle, page, -1);

        try
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            bool hasPoints = false;

            foreach (var path in paths)
            {
                var pointsList = path.ToList();
                if (pointsList.Count == 0) continue;

                var pointSize = sizeof(FS_POINTF_.__Internal);
                var memory = Marshal.AllocHGlobal(pointSize * pointsList.Count);

                try
                {
                    var ptr = (FS_POINTF_.__Internal*)memory;
                    for (int i = 0; i < pointsList.Count; i++)
                    {
                        var (x, y) = pointsList[i];
                        ptr[i].x = (float)x;
                        ptr[i].y = (float)y;

                        // Update bounds
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                        hasPoints = true;
                    }

                    // Create a wrapper for the call
                    // FS_POINTF_.__CreateInstance with native pointer creates a wrapper that does NOT own the memory (if we use the pointer ctor)
                    // But we need to check which method to use.
                    // __CreateInstance(__IntPtr native, bool skipVTables) calls new FS_POINTF_(native.ToPointer(), skipVTables)
                    // protected FS_POINTF_(void* native, bool skipVTables) sets __Instance = new __IntPtr(native);
                    // It does NOT set __ownsNativeInstance = true.
                    
                    var pointsWrapper = FS_POINTF_.__CreateInstance(new IntPtr(memory));
                    fpdf_annot.FPDFAnnotAddInkStroke(handle, pointsWrapper, (ulong)pointsList.Count);
                }
                finally
                {
                    Marshal.FreeHGlobal(memory);
                }
            }

            // Set color
            annotation.Color = color;

            // Set border width
            fpdf_annot.FPDFAnnotSetBorder(annotation._handle!, 0, 0, (float)width);

            // Set bounds
            if (hasPoints)
            {
                var padding = width / 2.0;
                annotation.Bounds = new PdfRectangle(
                    minX - padding,
                    minY - padding,
                    (maxX - minX) + width,
                    (maxY - minY) + width);
            }

            return annotation;
        }
        catch
        {
            annotation.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfInkAnnotation"/> class.
    /// Internal constructor - use factory methods to create annotations.
    /// </summary>
    internal PdfInkAnnotation(FpdfAnnotationT handle, PdfPage page, int index)
        : base(handle, page, PdfAnnotationType.Ink, index)
    {
    }

    /// <summary>
    /// Gets the paths (strokes) of the ink annotation.
    /// </summary>
    /// <returns>A list of paths, where each path is a list of points.</returns>
    public List<List<(double X, double Y)>> GetPaths()
    {
        ThrowIfDisposed();

        var paths = new List<List<(double X, double Y)>>();
        var pathCount = fpdf_annot.FPDFAnnotGetInkListCount(_handle!);

        for (uint i = 0; i < pathCount; i++)
        {
            // First call with null buffer to get the number of points in the path
            // Wait, FPDFAnnotGetInkListPath takes buffer and length (number of points).
            // But how do we know the number of points?
            // "Returns the number of points in the ink stroke." - documentation usually says this.
            
            // Let's verify signature return value logic.
            // public static uint FPDFAnnotGetInkListPath(..., buffer, length)
            
            var pointCount = fpdf_annot.FPDFAnnotGetInkListPath(_handle!, i, null!, 0);
            if (pointCount == 0) continue;

            var currentPath = new List<(double X, double Y)>((int)pointCount);
            
            var pointSize = sizeof(FS_POINTF_.__Internal);
            var memory = Marshal.AllocHGlobal((int)(pointSize * pointCount));

            try
            {
                var pointsWrapper = FS_POINTF_.__CreateInstance(new IntPtr(memory));
                
                var retrievedCount = fpdf_annot.FPDFAnnotGetInkListPath(_handle!, i, pointsWrapper, pointCount);
                
                var ptr = (FS_POINTF_.__Internal*)memory;
                for (int j = 0; j < retrievedCount; j++)
                {
                    currentPath.Add((ptr[j].x, ptr[j].y));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(memory);
            }

            paths.Add(currentPath);
        }

        return paths;
    }
}
