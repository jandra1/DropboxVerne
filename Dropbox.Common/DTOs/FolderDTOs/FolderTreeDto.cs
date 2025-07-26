namespace Dropbox.Common.DTOs.FolderDTOs
{
    public class FolderTreeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentFolderId { get; set; }
        public List<FolderTreeDto> SubFolders { get; set; } = new();
    }
}

