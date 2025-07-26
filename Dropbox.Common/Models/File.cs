namespace Dropbox.Common.Models;

public class File : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid FolderId { get; set; }
    public virtual Folder Folder { get; set; } = null!;
}
