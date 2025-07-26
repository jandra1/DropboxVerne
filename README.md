# File System API

A large-scale browser-based file system API built with .NET 6.
This API provides folder and file management capabilities with a hierarchical structure.

## Features

- **Folder Management**
  - Create folders and subfolders
  - Get folder contents (files and subfolders)
  - Delete empty folders
  - Browse folder hierarchy

- **File Management**
  - Create files in folders
  - Delete files
  - Get files by folder

- **Search Functionality**
  - Search files by exact name within a parent folder or across all files
  - Autocomplete functionality - get top 10 files that start with a search string
  - Only "starts with" logic for autocomplete

## Architecture

- **MVC Architecture** with clear separation of concerns
- **Entity Framework Core** for data access
- **SQL Server Express LocalDB** for production or **In-Memory Database** for development
- **Swagger/OpenAPI** for API documentation
- **Clean Architecture** with separate projects for API, Core logic, and Common models

## Prerequisites

- .NET 6.0 SDK
- SQL Server Express LocalDB (optional - will use in-memory database if not available)

## Getting Started

### Using Visual Studio

1. **Clone or download the solution**
2. **Open `Dropbox.sln` in Visual Studio**
3. **Set `Dropbox` as the startup project** (right-click on Dropbox project → Set as StartUp Project)
4. **Press F5 to build and run**

## Database Configuration

### Development (Default)

The application automatically creates a "Root" folder on startup

The application uses **SQL Server Express LocalDB** in development mode with the connection string:
```
Server=(localdb)\\mssqllocaldb;Database=FileSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true
```

### Fallback
If LocalDB is not available, the application automatically falls back to **In-Memory Database** for testing purposes.

### Custom Database
To use a different database, update the connection string in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your connection string here"
  }
}
```

## API Endpoints

### Folders
- `POST /api/folders` - Create a new folder (if no parentFolderId is provided, it will be created in the root folder)
- `GET /api/folders/{id}` - Get folder by ID with contents
- `GET /api/folders/root/folder` - Get the root folder with its contents
- `GET /api/folders/root` - Get all root folders
- `GET /api/folders/tree` - Get complete folder hierarchy
- `DELETE /api/folders/{id}` - Delete an empty folder

### Files
- `POST /api/files` - Create a new file (if no folderId is provided or folder doesn't exist, file will be created in root folder)
- `GET /api/files/{id}` - Get file by ID
- `GET /api/files/folder/{folderId}` - Get all files in a folder
- `GET /api/files/search?name={name}&folderId={folderId}` - Search files by exact name
- `GET /api/files/autocomplete?searchTerm={term}&count={count}` - Get files starting with search term
- `DELETE /api/files/{id}` - Delete a file

## Error Handling

The API returns appropriate HTTP status codes:
- `200 OK` - Success
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid input data
- `404 Not Found` - Resource not found
- `409 Conflict` - Duplicate name in same location
- `500 Internal Server Error` - Server error

## Validation Rules

- **Folder names** must be unique within the same parent folder
- **File names** must be unique within the same folder
- **Empty folders** can be deleted, but folders with contents cannot
- **Parent folder** must exist when creating subfolders (if not provided, will use root folder)
- **Target folder** must exist when creating files (if not provided or invalid, will use root folder)
- **Root folder fallback**: When creating folders without a parent or files without a valid folder, they will automatically be placed in the root folder