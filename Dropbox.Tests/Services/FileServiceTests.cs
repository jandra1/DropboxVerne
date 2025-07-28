using FluentAssertions;
using Dropbox.Core.Services;
using Dropbox.Common.DTOs;
using Dropbox.Tests.Infrastructure;
using Xunit;
using Dropbox.Common.DTOs.FileDtos;

namespace Dropbox.Tests.Services;

public class FileServiceTests : BaseServiceTest
{
    private readonly FileService _fileService;
    private readonly FolderService _folderService;

    public FileServiceTests()
    {
        _fileService = new FileService(Context);
        _folderService = new FolderService(Context);
    }

    [Fact]
    public async Task CreateFileAsync_WithValidData_ShouldCreateFile()
    {
        var createFileDto = new CreateFileDto
        {
            Name = "test.txt",
            FolderId = DocumentsFolderId
        };

        var result = await _fileService.CreateFileAsync(createFileDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("test.txt");
        result.FolderId.Should().Be(DocumentsFolderId);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateFileAsync_WithoutFolderId_ShouldCreateInRoot()
    {
        var createFileDto = new CreateFileDto
        {
            Name = "rootfile.txt",
            FolderId = Guid.Empty
        };

        var result = await _fileService.CreateFileAsync(createFileDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("rootfile.txt");
        
        var rootFolder = await _folderService.GetRootFolderAsync();
        rootFolder.Should().NotBeNull();
        result.FolderId.Should().Be(rootFolder!.Id);
    }

    [Fact]
    public async Task CreateFileAsync_WithEmptyGuidFolderId_ShouldCreateInRoot()
    {
        var createFileDto = new CreateFileDto
        {
            Name = "rootfile2.txt",
            FolderId = Guid.Empty
        };

        var result = await _fileService.CreateFileAsync(createFileDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("rootfile2.txt");
        
        var rootFolder = await _folderService.GetRootFolderAsync();
        result.FolderId.Should().Be(rootFolder!.Id);
    }

    [Fact]
    public async Task CreateFileAsync_WithNonExistentFolderId_ShouldCreateInRoot()
    {
        var createFileDto = new CreateFileDto
        {
            Name = "fallbackfile.txt",
            FolderId = Guid.NewGuid() // Non-existent folder
        };

        var result = await _fileService.CreateFileAsync(createFileDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("fallbackfile.txt");
        
        // Verify it's in the root folder
        var rootFolder = await _folderService.GetRootFolderAsync();
        result.FolderId.Should().Be(rootFolder!.Id);
    }

    [Fact]
    public async Task CreateFileAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        var createFileDto = new CreateFileDto
        {
            Name = "document.pdf", // Already exists in Documents folder
            FolderId = DocumentsFolderId
        };

        await _fileService.Invoking(x => x.CreateFileAsync(createFileDto))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetFileByIdAsync_WithValidId_ShouldReturnFile()
    {
        var result = await _fileService.GetFileByIdAsync(DocumentFileId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("document.pdf");
        result.FolderId.Should().Be(DocumentsFolderId);
        result.Id.Should().Be(DocumentFileId);
    }

    [Fact]
    public async Task GetFileByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var result = await _fileService.GetFileByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFilesByFolderIdAsync_WithValidFolderId_ShouldReturnFiles()
    {
        var result = await _fileService.GetFilesByFolderIdAsync(DocumentsFolderId);

        result.Should().HaveCount(2);
        result.Should().Contain(f => f.Name.Equals("document.pdf"));
        result.Should().Contain(f => f.Name.Equals("report.docx"));
        result.All(f => f.FolderId.Equals(DocumentsFolderId)).Should().BeTrue();
    }

    [Fact]
    public async Task GetFilesByFolderIdAsync_WithEmptyFolder_ShouldReturnEmptyList()
    {
        var result = await _fileService.GetFilesByFolderIdAsync(ProjectsFolderId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteFileAsync_WithValidId_ShouldDeleteFile()
    {
        var result = await _fileService.DeleteFileAsync(ReportFileId);

        result.Should().BeTrue();
        
        var deletedFile = await _fileService.GetFileByIdAsync(ReportFileId);
        deletedFile.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFileAsync_WithInvalidId_ShouldReturnFalse()
    {
        var result = await _fileService.DeleteFileAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchFilesByNameAsync_WithExactMatch_ShouldReturnMatchingFiles()
    {
        var result = await _fileService.SearchFilesByNameAsync("document.pdf");

        result.Should().NotBeNull();
        result.Files.Should().HaveCount(1);
        result.Files.First().Name.Should().Be("document.pdf");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchFilesByNameAsync_WithFolderScope_ShouldReturnMatchingFilesInFolder()
    {
        var result = await _fileService.SearchFilesByNameAsync("document.pdf", DocumentsFolderId);

        result.Should().NotBeNull();
        result.Files.Should().HaveCount(1);
        result.Files.First().Name.Should().Be("document.pdf");
        result.Files.First().FolderId.Should().Be(DocumentsFolderId);
    }

    [Fact]
    public async Task SearchFilesByNameAsync_WithNoMatch_ShouldReturnEmptyResult()
    {
        var result = await _fileService.SearchFilesByNameAsync("nonexistent.txt");

        result.Should().NotBeNull();
        result.Files.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetTopFilesStartingWithAsync_ShouldReturnMatchingFiles()
    {
        var result = await _fileService.GetTopFilesStartingWithAsync("re", 10);

        result.Should().HaveCount(2);
        result.Should().Contain(f => f.Name.Equals("readme.txt"));
        result.Should().Contain(f => f.Name.Equals("report.docx"));
        result.Should().BeInAscendingOrder(f => f.Name);
    }

    [Fact]
    public async Task GetTopFilesStartingWithAsync_WithCount_ShouldRespectLimit()
    {
        var result = await _fileService.GetTopFilesStartingWithAsync("re", 1);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("readme.txt"); // Should be first alphabetically
    }

    [Fact]
    public async Task GetTopFilesStartingWithAsync_WithNoMatch_ShouldReturnEmptyList()
    {
        var result = await _fileService.GetTopFilesStartingWithAsync("xyz", 10);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateFileAsync_WithNoRootFolder_ShouldThrowInvalidOperationException()
    {
        Context.Folders.RemoveRange(Context.Folders);
        await Context.SaveChangesAsync();

        var createFileDto = new CreateFileDto
        {
            Name = "test.txt",
            FolderId = Guid.Empty
        };

        await _fileService.Invoking(x => x.CreateFileAsync(createFileDto))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}
