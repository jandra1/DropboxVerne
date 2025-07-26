namespace Dropbox.Common.DTOs.FileDtos
{
    public class CreateFileDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid FolderId { get; set; }
    }
}

