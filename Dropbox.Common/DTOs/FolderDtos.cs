namespace Dropbox.Common.DTOs;

public class CreateFolderDto
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
}

public class FolderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<FolderDto> SubFolders { get; set; } = new();
    public List<FileDto> Files { get; set; } = new();
}

public class FolderTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public List<FolderTreeDto> SubFolders { get; set; } = new();
}
