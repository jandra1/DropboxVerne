using Dropbox.Common.DTOs.FileDtos;

namespace Dropbox.Common.DTOs.FolderDTOs
{
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
}

