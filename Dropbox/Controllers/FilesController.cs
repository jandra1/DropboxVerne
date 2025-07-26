using Microsoft.AspNetCore.Mvc;
using Dropbox.Common.DTOs;
using Dropbox.Common.Interfaces;
using Dropbox.Common.DTOs.FileDtos;

namespace Dropbox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Create a new file
    /// </summary>
    /// <param name="createFileDto">File creation details (if folderId is null or invalid, file will be created in root folder)</param>
    /// <returns>Created file information</returns>
    [HttpPost]
    public async Task<ActionResult<FileDto>> CreateFile([FromBody] CreateFileDto createFileDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createFileDto.Name))
            {
                return BadRequest("File name cannot be empty");
            }

            var file = await _fileService.CreateFileAsync(createFileDto);
            return CreatedAtAction(nameof(GetFileById), new { id = file.Id }, file);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get file by ID
    /// </summary>
    /// <param name="id">File ID</param>
    /// <returns>File information</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<FileDto>> GetFileById(Guid id)
    {
        var file = await _fileService.GetFileByIdAsync(id);
        if (file is null)
        {
            return NotFound($"File with ID {id} not found");
        }

        return Ok(file);
    }

    /// <summary>
    /// Get all files in a specific folder
    /// </summary>
    /// <param name="folderId">Folder ID</param>
    /// <returns>List of files in the folder</returns>
    [HttpGet("folder/{folderId}")]
    public async Task<ActionResult<List<FileDto>>> GetFilesByFolderId(Guid folderId)
    {
        var files = await _fileService.GetFilesByFolderIdAsync(folderId);
        return Ok(files);
    }

    /// <summary>
    /// Search files by exact name
    /// </summary>
    /// <param name="name">Exact file name to search for</param>
    /// <param name="folderId">Optional folder ID to limit search scope</param>
    /// <returns>Search results</returns>
    [HttpGet("search")]
    public async Task<ActionResult<SearchResultDto>> SearchFiles([FromQuery] string name, [FromQuery] Guid? folderId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Search term cannot be empty");
        }

        var results = await _fileService.SearchFilesByNameAsync(name, folderId);
        return Ok(results);
    }

    /// <summary>
    /// Get top 10 files that start with the given search term (for autocomplete)
    /// </summary>
    /// <param name="searchTerm">Search term to match file names that start with this string</param>
    /// <param name="count">Number of results to return (default: 10)</param>
    /// <returns>List of matching files</returns>
    [HttpGet("autocomplete")]
    public async Task<ActionResult<List<FileDto>>> GetFilesStartingWith([FromQuery] string searchTerm, [FromQuery] int count = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty");
        }

        if (count <= 0 || count > 50)
        {
            return BadRequest("Count must be between 1 and 50");
        }

        var files = await _fileService.GetTopFilesStartingWithAsync(searchTerm, count);
        return Ok(files);
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="id">File ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFile(Guid id)
    {
        var deleted = await _fileService.DeleteFileAsync(id);
        if (!deleted)
        {
            return NotFound($"File with ID {id} not found");
        }

        return NoContent();
    }
}
