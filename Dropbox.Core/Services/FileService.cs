using Microsoft.EntityFrameworkCore;
using Dropbox.Common.Interfaces;
using Dropbox.Core.Data;
using Dropbox.Common.DTOs.FileDtos;

namespace Dropbox.Core.Services;

public class FileService : IFileService
{
    private readonly FileSystemDbContext _context;

    public FileService(FileSystemDbContext context)
    {
        _context = context;
    }

    public async Task<FileDto> CreateFileAsync(CreateFileDto createFileDto)
    {
        var targetFolderId = createFileDto.FolderId;

        // If no folder ID is provided or its empty, use the root folder
        if (Guid.Empty.Equals(targetFolderId))
        {
            var rootFolder = await _context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == null);
            
            if (rootFolder is null)
            {
                throw new InvalidOperationException("No root folder found in the system");
            }

            targetFolderId = rootFolder.Id;
        }
        else
        {
            var folderExists = await _context.Folders.AnyAsync(f => f.Id.Equals(targetFolderId));
            
            // If folder doesn't exist, use the root folder
            if (!folderExists)
            {
                var rootFolder = await _context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == null);
                
                if (rootFolder is null)
                {
                    throw new InvalidOperationException("No root folder found in the system");
                }

                targetFolderId = rootFolder.Id;
            }
        }

        // Check if file with the same name already exists in the target folder
        var duplicateExists = await _context.Files.AnyAsync(f => f.Name.Equals(createFileDto.Name) && f.FolderId.Equals(targetFolderId));
        
        if (duplicateExists)
        {
            throw new InvalidOperationException("A file with this name already exists in the target folder");
        }

        var file = new Common.Models.File
        {
            Name = createFileDto.Name,
            FolderId = targetFolderId
        };

        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        return new FileDto
        {
            Id = file.Id,
            Name = file.Name,
            FolderId = file.FolderId,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt
        };
    }

    public async Task<FileDto?> GetFileByIdAsync(Guid id)
    {
        var file = await _context.Files.FirstOrDefaultAsync(f => f.Id.Equals(id));

        if (file is null)
        {
            return null;
        }

        return new FileDto
        {
            Id = file.Id,
            Name = file.Name,
            FolderId = file.FolderId,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt
        };
    }

    public async Task<List<FileDto>> GetFilesByFolderIdAsync(Guid folderId)
    {
        var files = await _context.Files.Where(f => f.FolderId == folderId).ToListAsync();

        return files.Select(file => new FileDto
        {
            Id = file.Id,
            Name = file.Name,
            FolderId = file.FolderId,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt
        }).ToList();
    }

    public async Task<bool> DeleteFileAsync(Guid id)
    {
        var file = await _context.Files.FirstOrDefaultAsync(f => f.Id.Equals(id));

        if (file is null)
        {
            return false;
        }

        _context.Files.Remove(file);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<SearchResultDto> SearchFilesByNameAsync(string searchTerm, Guid? folderId = null)
    {
        var query = _context.Files.AsQueryable();

        if (folderId.HasValue)
        {
            query = query.Where(f => f.FolderId.Equals(folderId.Value));
        }

        // Exact name match
        var files = await query
            .Where(f => f.Name == searchTerm)
            .ToListAsync();

        var fileDtos = files.Select(file => new FileDto
        {
            Id = file.Id,
            Name = file.Name,
            FolderId = file.FolderId,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt
        }).ToList();

        return new SearchResultDto
        {
            Files = fileDtos,
            TotalCount = fileDtos.Count
        };
    }

    public async Task<List<FileDto>> GetTopFilesStartingWithAsync(string searchTerm, int count = 10)
    {
        var files = await _context.Files
            .Where(f => f.Name.StartsWith(searchTerm))
            .OrderBy(f => f.Name)
            .Take(count)
            .ToListAsync();

        return files.Select(file => new FileDto
        {
            Id = file.Id,
            Name = file.Name,
            FolderId = file.FolderId,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt
        }).ToList();
    }
}
