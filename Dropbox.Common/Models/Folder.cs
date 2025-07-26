namespace Dropbox.Common.Models;

public class Folder : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public virtual Folder? ParentFolder { get; set; }
    public virtual ICollection<Folder> SubFolders { get; set; } = new List<Folder>();
    public virtual ICollection<File> Files { get; set; } = new List<File>();
}
