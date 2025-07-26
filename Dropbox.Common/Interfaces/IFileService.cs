using Dropbox.Common.DTOs.FileDtos;

namespace Dropbox.Common.Interfaces;

public interface IFileService
{
    Task<FileDto> CreateFileAsync(CreateFileDto createFileDto);
    Task<FileDto?> GetFileByIdAsync(Guid id);
    Task<List<FileDto>> GetFilesByFolderIdAsync(Guid folderId);
    Task<bool> DeleteFileAsync(Guid id);
    Task<SearchResultDto> SearchFilesByNameAsync(string searchTerm, Guid? folderId = null);
    Task<List<FileDto>> GetTopFilesStartingWithAsync(string searchTerm, int count = 10);
}
