using Microsoft.EntityFrameworkCore;
using Dropbox.Common.DTOs;
using Dropbox.Common.Interfaces;
using Dropbox.Common.Models;
using Dropbox.Core.Data;
using Dropbox.Common.DTOs.FolderDTOs;
using Dropbox.Common.DTOs.FileDtos;

namespace Dropbox.Core.Services;

public class FolderService : IFolderService
{
    private readonly FileSystemDbContext _context;

    public FolderService(FileSystemDbContext context)
    {
        _context = context;
    }

    public async Task<FolderDto> CreateFolderAsync(CreateFolderDto createFolderDto)
    {
        // If no parent folder is specified, use the root folder
        if (!createFolderDto.ParentFolderId.HasValue)
        {
            var rootFolder = await _context.Folders.FirstOrDefaultAsync(f => f.ParentFolderId == null);
            
            if (rootFolder != null)
            {
                createFolderDto.ParentFolderId = rootFolder.Id;
            }
        }

        if (createFolderDto.ParentFolderId.HasValue)
        {
            var parentExists = await _context.Folders.AnyAsync(f => f.Id.Equals(createFolderDto.ParentFolderId.Value));
            
            if (!parentExists)
            {
                throw new ArgumentException("Parent folder does not exist");
            }
        }

        // Check if folder with same name already exists in the same parent
        var duplicateExists = await _context.Folders
            .AnyAsync(f => f.Name.Equals(createFolderDto.Name) && f.ParentFolderId.Equals(createFolderDto.ParentFolderId));
        
        if (duplicateExists)
            throw new InvalidOperationException("A folder with this name already exists in the specified location");

        var folder = new Folder
        {
            Name = createFolderDto.Name,
            ParentFolderId = createFolderDto.ParentFolderId
        };

        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();

        return await GetFolderByIdAsync(folder.Id) ?? throw new InvalidOperationException("Failed to retrieve created folder");
    }

    public async Task<FolderDto?> GetFolderByIdAsync(Guid id)
    {
        var folder = await _context.Folders
            .Include(f => f.SubFolders)
            .Include(f => f.Files)
            .FirstOrDefaultAsync(f => f.Id.Equals(id));

        if (folder is null)
        {
            return null;
        }

        return new FolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentFolderId = folder.ParentFolderId,
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt,
            SubFolders = folder.SubFolders.Select(sf => new FolderDto
            {
                Id = sf.Id,
                Name = sf.Name,
                ParentFolderId = sf.ParentFolderId,
                CreatedAt = sf.CreatedAt,
                UpdatedAt = sf.UpdatedAt
            }).ToList(),
            Files = folder.Files.Select(f => new FileDto
            {
                Id = f.Id,
                Name = f.Name,
                FolderId = f.FolderId,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            }).ToList()
        };
    }

    public async Task<FolderDto?> GetRootFolderAsync()
    {
        var rootFolder = await _context.Folders
            .Include(f => f.SubFolders)
            .Include(f => f.Files)
            .FirstOrDefaultAsync(f => f.ParentFolderId == null);

        if (rootFolder is null)
        {
            return null;

        }
          
        return new FolderDto
        {
            Id = rootFolder.Id,
            Name = rootFolder.Name,
            ParentFolderId = rootFolder.ParentFolderId,
            CreatedAt = rootFolder.CreatedAt,
            UpdatedAt = rootFolder.UpdatedAt,
            SubFolders = rootFolder.SubFolders.Select(sf => new FolderDto
            {
                Id = sf.Id,
                Name = sf.Name,
                ParentFolderId = sf.ParentFolderId,
                CreatedAt = sf.CreatedAt,
                UpdatedAt = sf.UpdatedAt
            }).ToList(),
            Files = rootFolder.Files.Select(f => new FileDto
            {
                Id = f.Id,
                Name = f.Name,
                FolderId = f.FolderId,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            }).ToList()
        };
    }

    public async Task<List<FolderDto>> GetRootFoldersAsync()
    {
        var rootFolders = await _context.Folders
            .Where(f => f.ParentFolderId == null)
            .Include(f => f.SubFolders)
            .Include(f => f.Files)
            .ToListAsync();

        return rootFolders.Select(folder => new FolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentFolderId = folder.ParentFolderId,
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt,
            SubFolders = folder.SubFolders.Select(sf => new FolderDto
            {
                Id = sf.Id,
                Name = sf.Name,
                ParentFolderId = sf.ParentFolderId,
                CreatedAt = sf.CreatedAt,
                UpdatedAt = sf.UpdatedAt
            }).ToList(),
            Files = folder.Files.Select(f => new FileDto
            {
                Id = f.Id,
                Name = f.Name,
                FolderId = f.FolderId,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            }).ToList()
        }).ToList();
    }

    public async Task<bool> DeleteFolderAsync(Guid id)
    {
        var folder = await _context.Folders
            .Include(f => f.SubFolders)
            .Include(f => f.Files)
            .FirstOrDefaultAsync(f => f.Id.Equals(id));

        if (folder is null)
        {
            return false;
        }

        // Check if folder has subfolders or files
        if (folder.SubFolders.Any() || folder.Files.Any())
        {
            // To be considered but for easier implementation this cant be done for now.
            throw new InvalidOperationException("Cant delete folder that contains subfolders or files");
        }

        _context.Folders.Remove(folder);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<FolderTreeDto> GetFolderTreeAsync()
    {
        var allFolders = await _context.Folders.ToListAsync();
        var rootFolders = allFolders.Where(f => f.ParentFolderId is null).ToList();

        if (!rootFolders.Any())
        {
            throw new InvalidOperationException("No root folder found");
        }

        var rootFolder = rootFolders.First();
        return BuildFolderTree(rootFolder, allFolders);
    }

    private static FolderTreeDto BuildFolderTree(Folder folder, List<Folder> allFolders)
    {
        var subFolders = allFolders.Where(f => f.ParentFolderId.Equals(folder.Id)).ToList();
        
        return new FolderTreeDto
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentFolderId = folder.ParentFolderId,
            SubFolders = subFolders.Select(sf => BuildFolderTree(sf, allFolders)).ToList()
        };
    }
}
