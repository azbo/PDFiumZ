git clean -fxd
dotnet build src/PDFiumZBindingsGenerator/PDFiumZBindingsGenerator.csproj -c Release
dotnet ./src/PDFiumZBindingsGenerator/bin/Release/net8.0/PDFiumZBindingsGenerator.dll latest true
dotnet pack ./src/PDFiumZ/PDFiumZ.csproj -c Release -o ./artifacts/
dotnet test ./src/PDFiumZ.Tests/PDFiumZ.Tests.csproj
pause