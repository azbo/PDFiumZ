using System;
using System.IO;
using CppSharp;

namespace PDFiumZBindingsGenerator
{
    class GenerateLocal
    {
        static void Main(string[] args)
        {
            var rootDir = Path.GetFullPath(Path.Combine(@"C:\work\net\PDFiumZ"));
            var solutionDir = Path.GetFullPath(Path.Combine(rootDir, "src"));
            var pdfiumProjectDir = Path.GetFullPath(Path.Combine(solutionDir, "PDFiumZ"));
            var destinationCsPath = Path.GetFullPath(Path.Combine(pdfiumProjectDir, "PDFiumZ.cs"));
            var baseDirectory = Path.GetFullPath(Path.Combine(rootDir, "artifacts/libraries/pdfium-win-x64"));
            var includeDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "include/pdfium"));

            Console.WriteLine($"Base directory: {baseDirectory}");
            Console.WriteLine($"Include directory: {includeDirectory}");
            Console.WriteLine($"Output file: {destinationCsPath}");

            // Run CppSharp
            ConsoleDriver.Run(new PDFiumZLibrary(baseDirectory, includeDirectory));

            // Find generated file
            var generatedFiles = Directory.GetFiles(baseDirectory, "PDFiumZ.cs", SearchOption.AllDirectories);

            if (generatedFiles.Length > 0)
            {
                var generatedCsPath = generatedFiles[0];
                Console.WriteLine($"Found generated file: {generatedCsPath}");

                var fileContents = File.ReadAllText(generatedCsPath);

                using (var fs = new FileStream(destinationCsPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine("// Built from PDFium 146.0.7643 headers");
                    sw.WriteLine("// Generated on: " + DateTime.UtcNow.ToString("R"));
                    sw.WriteLine();
                    sw.Write(fileContents);
                }

                Console.WriteLine($"Bindings generated successfully: {destinationCsPath}");

                // Get file info
                var fileInfo = new FileInfo(destinationCsPath);
                Console.WriteLine($"Generated file size: {fileInfo.Length:N0} bytes");
            }
            else
            {
                Console.WriteLine($"Error: Generated file not found in {baseDirectory}");
            }
        }
    }
}
