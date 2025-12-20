git clean -fxd
dotnet build src/PDFiumZBindingsGenerator/PDFiumZBindingsGenerator.csproj -c Release
dotnet ./src/PDFiumZBindingsGenerator/bin/Release/net8.0/PDFiumZBindingsGenerator.dll latest true
dotnet pack ./src/PDFiumZ/PDFiumZ.csproj -c Release -o ./artifacts/
dotnet test ./src/PDFiumZ.Tests/PDFiumZ.Tests.csproj
if($?)
{	
    $xmlNode = Select-Xml -Path src/Directory.Build.props -XPath '/Project/PropertyGroup/Version' 
    $version = "v" + ($xmlNode.Node.InnerXML)
	$commitMessage = "PDFium version " + $version
	$confirmation = Read-Host "Do you want to commit and push the new tag? (y/n)"
	if ($confirmation -eq 'y') {
	    git commit -a -m $commitMessage
        git tag $version

	    git push origin
	    git push origin $version 
        echo "Pushed tag to origin."
	}
    else 
    {
        echo "Canceled tag & push."
    }
}
else 
{
	echo "Tests Failed."
}
pause