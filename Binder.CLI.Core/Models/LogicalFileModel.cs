using System;

namespace Binder.API.Models.Region.Filesystem
{
	public class LogicalFileModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string LogicalFolderId { get; set; }
		public long Length { get; set; }
		public long Sequence { get; set; }
		public string IconUrl { get; set; }
		public DateTime CreatedDTSZ { get; set; }
		public DateTime ModifiedDTSZ { get; set; }
		public string LastAccessedBySiteUserName { get; set; }
		public DateTime LastAccessedDTSZ { get; set; }
		public string CreatedByEdocxUserId { get; set; }
		public string CreatedDTSZString { get; set; }
		public string ModifiedDTSZString { get; set; }
		public string LastAccessedDTSZString { get; set; }
		public string LengthString { get; set; }
		public string CheckedOutFileId { get; set; }
        public string StorageZoneId { get; set; }
	}
}