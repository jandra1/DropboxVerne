using Microsoft.EntityFrameworkCore;
using Dropbox.Core.Data;
using Dropbox.Core.Services;
using Dropbox.Common.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "File System API", Version = "v1" });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure database - use SQL Server if connection string is provided, otherwise use InMemory database
builder.Services.AddDbContext<FileSystemDbContext>(options => options.UseInMemoryDatabase("FileSystemDb"));

// Register services
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IFileService, FileService>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FileSystemDbContext>();
    // context.Database.EnsureCreated();
    
    // Ensure root folder exists for in-memory database
    if (!context.Folders.Any())
    {
        var rootFolder = new Dropbox.Common.Models.Folder
        {
            Id = Guid.NewGuid(),
            Name = "Root",
            ParentFolderId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Folders.Add(rootFolder);
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "File System API v1");
        c.RoutePrefix = string.Empty;
    });
    app.UseCors("DevelopmentCors");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
