namespace Dropbox.Common.DTOs.FolderDTOs
{
    public class CreateFolderDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid? ParentFolderId { get; set; }
    }
}

