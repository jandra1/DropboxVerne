namespace Dropbox.Common.DTOs.FileDtos
{
    public class SearchResultDto
    {
        public List<FileDto> Files { get; set; } = new();
        public int TotalCount { get; set; }
    }
}

