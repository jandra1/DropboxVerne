using FluentAssertions;
using Dropbox.Core.Services;
using Dropbox.Common.DTOs;
using Dropbox.Tests.Infrastructure;
using Xunit;
using Dropbox.Common.DTOs.FolderDTOs;

namespace Dropbox.Tests.Services;

public class FolderServiceTests : BaseServiceTest
{
    private readonly FolderService _folderService;

    public FolderServiceTests()
    {
        _folderService = new FolderService(Context);
    }

    [Fact]
    public async Task CreateFolderAsync_WithValidData_ShouldCreateFolder()
    {
        var createFolderDto = new CreateFolderDto
        {
            Name = "NewFolder",
            ParentFolderId = DocumentsFolderId
        };

        var result = await _folderService.CreateFolderAsync(createFolderDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("NewFolder");
        result.ParentFolderId.Should().Be(DocumentsFolderId);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateFolderAsync_WithoutParentFolderId_ShouldCreateInRoot()
    {
        var createFolderDto = new CreateFolderDto
        {
            Name = "RootLevelFolder",
            ParentFolderId = null
        };

        var result = await _folderService.CreateFolderAsync(createFolderDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("RootLevelFolder");
        
        var rootFolder = await _folderService.GetRootFolderAsync();
        result.ParentFolderId.Should().Be(rootFolder!.Id);
    }

    [Fact]
    public async Task CreateFolderAsync_WithNonExistentParent_ShouldThrowArgumentException()
    {
        var createFolderDto = new CreateFolderDto
        {
            Name = "TestFolder",
            ParentFolderId = Guid.NewGuid() // Non-existent folder
        };

        await _folderService.Invoking(x => x.CreateFolderAsync(createFolderDto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Parent folder does not exist");
    }

    [Fact]
    public async Task CreateFolderAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        var createFolderDto = new CreateFolderDto
        {
            Name = "Documents", // Already exists
            ParentFolderId = RootFolderId
        };

        await _folderService.Invoking(x => x.CreateFolderAsync(createFolderDto))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetFolderByIdAsync_WithValidId_ShouldReturnFolderWithContents()
    {
        var result = await _folderService.GetFolderByIdAsync(DocumentsFolderId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Documents");
        result.ParentFolderId.Should().Be(RootFolderId);
        result.SubFolders.Should().HaveCount(1);
        result.SubFolders.First().Name.Should().Be("Projects");
        result.Files.Should().HaveCount(2);
        result.Files.Should().Contain(f => f.Name.Equals("document.pdf"));
        result.Files.Should().Contain(f => f.Name.Equals("report.docx"));
    }

    [Fact]
    public async Task GetFolderByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var result = await _folderService.GetFolderByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    } 
   
    [Fact]
    public async Task DeleteFolderAsync_WithEmptyFolder_ShouldDeleteSuccessfully()
    {
        var result = await _folderService.DeleteFolderAsync(ProjectsFolderId);

        result.Should().BeTrue();
        
        var deletedFolder = await _folderService.GetFolderByIdAsync(ProjectsFolderId);
        deletedFolder.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFolderAsync_WithNonExistentFolder_ShouldReturnFalse()
    {
        var result = await _folderService.DeleteFolderAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }
}
