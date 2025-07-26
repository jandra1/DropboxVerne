using Microsoft.AspNetCore.Mvc;
using Dropbox.Common.DTOs;
using Dropbox.Common.Interfaces;

namespace Dropbox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FoldersController : ControllerBase
{
    private readonly IFolderService _folderService;

    public FoldersController(IFolderService folderService)
    {
        _folderService = folderService;
    }

    /// <summary>
    /// Create a new folder
    /// </summary>
    /// <param name="createFolderDto">Folder creation details</param>
    /// <returns>Created folder information</returns>
    [HttpPost]
    public async Task<ActionResult<FolderDto>> CreateFolder([FromBody] CreateFolderDto createFolderDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createFolderDto.Name))
            {
                return BadRequest("Folder name cannot be empty");
            }

            var folder = await _folderService.CreateFolderAsync(createFolderDto);
            return CreatedAtAction(nameof(GetFolderById), new { id = folder.Id }, folder);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get folder by ID including its subfolders and files
    /// </summary>
    /// <param name="id">Folder ID</param>
    /// <returns>Folder information with contents</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<FolderDto>> GetFolderById(Guid id)
    {
        var folder = await _folderService.GetFolderByIdAsync(id);
        if (folder is null)
        {
            return NotFound($"Folder with ID {id} not found");
        }

        return Ok(folder);
    }

    /// <summary>
    /// Get the root folder with its contents
    /// </summary>
    /// <returns>Root folder information</returns>
    [HttpGet("root/folder")]
    public async Task<ActionResult<FolderDto>> GetRootFolder()
    {
        var rootFolder = await _folderService.GetRootFolderAsync();
        if (rootFolder is null)
        {
            return NotFound("Root folder not found");
        }

        return Ok(rootFolder);
    }

    /// <summary>
    /// Get all root folders
    /// </summary>
    /// <returns>List of root folders</returns>
    [HttpGet("root")]
    public async Task<ActionResult<List<FolderDto>>> GetRootFolders()
    {
        var folders = await _folderService.GetRootFoldersAsync();
        return Ok(folders);
    }

    /// <summary>
    /// Get complete folder tree structure
    /// </summary>
    /// <returns>Hierarchical folder tree</returns>
    [HttpGet("tree")]
    public async Task<ActionResult<FolderTreeDto>> GetFolderTree()
    {
        try
        {
            var tree = await _folderService.GetFolderTreeAsync();
            return Ok(tree);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete a folder (must be empty)
    /// </summary>
    /// <param name="id">Folder ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFolder(Guid id)
    {
        try
        {
            var deleted = await _folderService.DeleteFolderAsync(id);
            if (!deleted)
            {
                return NotFound($"Folder with ID {id} not found");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
