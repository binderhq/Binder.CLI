namespace Binder.API.Models.Region.Filesystem
{
    public class LogicalFolderModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string BoxId { get; set; }
        public string ParentFolderId { get; set; }
    }
}
