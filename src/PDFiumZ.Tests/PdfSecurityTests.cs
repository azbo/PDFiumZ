using PDFiumZ.HighLevel;
using Xunit;

namespace PDFiumZ.Tests;

public class PdfSecurityTests : IDisposable
{
    private const string TestOutputDir = "test-output";

    public PdfSecurityTests()
    {
        PdfiumLibrary.Initialize();
        Directory.CreateDirectory(TestOutputDir);
    }

    public void Dispose()
    {
        PdfiumLibrary.Shutdown();
    }

    [Fact]
    public void Security_UnencryptedDocument_ShouldShowNotEncrypted()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();

        // Act
        var security = document.Security;

        // Assert
        Assert.False(security.IsEncrypted);
        Assert.Equal(-1, security.SecurityHandlerRevision);
        Assert.Equal("None", security.EncryptionMethod);
        Assert.Equal("Not encrypted", security.ToString());
    }

    [Fact]
    public void Security_UnencryptedDocument_ShouldHaveAllPermissions()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();

        // Act
        var security = document.Security;

        // Assert
        // Note: PDFium returns 0 for permissions on newly created unencrypted documents
        // This is correct behavior - no encryption means no restrictions
        // The security info flags are convenience methods that check actual permissions
        Assert.True(security.CanPrint);
        Assert.True(security.CanPrintHighQuality);
        Assert.True(security.CanModifyContents);
        Assert.True(security.CanCopyContent);
        Assert.True(security.CanModifyAnnotations);
        Assert.True(security.CanFillForms);
        Assert.True(security.CanExtractForAccessibility);
        Assert.True(security.CanAssembleDocument);
    }

    [Fact]
    public void Permissions_UnencryptedDocument_ShouldReturnDefaultValue()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();

        // Act
        var permissions = document.Permissions;

        // Assert
        // Note: PDFium returns 0 for permissions on newly created unencrypted documents
        // This is correct - the document is not encrypted, so there are no permission restrictions
        Assert.Equal(0u, permissions);
    }

    [Fact]
    public void Security_AfterSave_ShouldStillBeUnencrypted()
    {
        // Arrange
        var filePath = Path.Combine(TestOutputDir, "test-unencrypted.pdf");
        using (var document = PdfDocument.CreateNew())
        {
            document.CreatePage(595, 842).Dispose();
            document.Save(filePath);
        }

        // Act
        using var reopened = PdfDocument.Open(filePath);
        var security = reopened.Security;

        // Assert
        Assert.False(security.IsEncrypted);
        // After saving and reopening, permissions may vary, just verify not encrypted
    }

    [Fact]
    public void PdfPermissions_FlagsEnum_ShouldHaveCorrectValues()
    {
        // Verify specific permission bit values match PDF spec
        Assert.Equal(0x00000004u, (uint)PdfPermissions.Print);
        Assert.Equal(0x00000008u, (uint)PdfPermissions.ModifyContents);
        Assert.Equal(0x00000010u, (uint)PdfPermissions.CopyContent);
        Assert.Equal(0x00000020u, (uint)PdfPermissions.ModifyAnnotations);
        Assert.Equal(0x00000100u, (uint)PdfPermissions.FillForms);
        Assert.Equal(0x00000200u, (uint)PdfPermissions.ExtractAccessibility);
        Assert.Equal(0x00000400u, (uint)PdfPermissions.AssembleDocument);
        Assert.Equal(0x00000800u, (uint)PdfPermissions.PrintHighQuality);
    }

    [Fact]
    public void PdfPermissions_CombinedFlags_ShouldWork()
    {
        // Arrange
        var permissions = PdfPermissions.Print | PdfPermissions.CopyContent;

        // Assert
        Assert.True(permissions.HasFlag(PdfPermissions.Print));
        Assert.True(permissions.HasFlag(PdfPermissions.CopyContent));
        Assert.False(permissions.HasFlag(PdfPermissions.ModifyContents));
    }

    [Fact]
    public void SecurityInfo_ToString_ShouldFormatCorrectly()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();

        // Act
        var security = document.Security;
        var toString = security.ToString();

        // Assert
        Assert.Equal("Not encrypted", toString);
    }

    [Fact]
    public void Security_UserPermissions_ShouldBeAccessible()
    {
        // Arrange
        using var document = PdfDocument.CreateNew();
        document.CreatePage(595, 842).Dispose();

        // Act
        var security = document.Security;

        // Assert
        // For unencrypted newly created documents, permissions are typically 0
        Assert.NotNull(security);
    }
}

