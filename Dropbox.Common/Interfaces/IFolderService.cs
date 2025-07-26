using Dropbox.Common.DTOs.FolderDTOs;

namespace Dropbox.Common.Interfaces;

public interface IFolderService
{
    Task<FolderDto> CreateFolderAsync(CreateFolderDto createFolderDto);
    Task<FolderDto?> GetFolderByIdAsync(Guid id);
    Task<FolderDto?> GetRootFolderAsync();
    Task<List<FolderDto>> GetRootFoldersAsync();
    Task<bool> DeleteFolderAsync(Guid id);
    Task<FolderTreeDto> GetFolderTreeAsync();
}
