namespace Dropbox.Common.DTOs.FileDtos
{
    public class FileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid FolderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

