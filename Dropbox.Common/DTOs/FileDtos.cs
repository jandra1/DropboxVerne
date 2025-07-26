namespace Dropbox.Common.DTOs;

public class CreateFileDto
{
    public string Name { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
}

public class FileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid FolderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SearchResultDto
{
    public List<FileDto> Files { get; set; } = new();
    public int TotalCount { get; set; }
}
