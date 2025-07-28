using Dropbox.Core.Data;
using Dropbox.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Dropbox.Tests.Infrastructure;

public abstract class BaseServiceTest : IDisposable
{
    protected FileSystemDbContext Context { get; private set; }

    protected BaseServiceTest()
    {
        var options = new DbContextOptionsBuilder<FileSystemDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new FileSystemDbContext(options);
        SeedData();
    }

    private void SeedData()
    {
        var rootFolder = new Folder
        {
            Id = RootFolderId,
            Name = "Root",
            ParentFolderId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var documentsFolder = new Folder
        {
            Id = DocumentsFolderId,
            Name = "Documents",
            ParentFolderId = rootFolder.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var projectsFolder = new Folder
        {
            Id = ProjectsFolderId,
            Name = "Projects",
            ParentFolderId = documentsFolder.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };        var file1 = new Dropbox.Common.Models.File
        {
            Id = ReadmeFileId,
            Name = "readme.txt",
            FolderId = rootFolder.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var file2 = new Dropbox.Common.Models.File
        {
            Id = DocumentFileId,
            Name = "document.pdf",
            FolderId = documentsFolder.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var file3 = new Dropbox.Common.Models.File
        {
            Id = ReportFileId,
            Name = "report.docx",
            FolderId = documentsFolder.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Context.Folders.AddRange(rootFolder, documentsFolder, projectsFolder);
        Context.Files.AddRange(file1, file2, file3);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Dispose();
    }

    protected Guid RootFolderId => new("11111111-1111-1111-1111-111111111111");
    protected Guid DocumentsFolderId => new("22222222-2222-2222-2222-222222222222");
    protected Guid ProjectsFolderId => new("33333333-3333-3333-3333-333333333333");
    protected Guid ReadmeFileId => new("44444444-4444-4444-4444-444444444444");
    protected Guid DocumentFileId => new("55555555-5555-5555-5555-555555555555");
    protected Guid ReportFileId => new("66666666-6666-6666-6666-666666666666");
}
