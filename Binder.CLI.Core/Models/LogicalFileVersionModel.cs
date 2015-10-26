using System;

namespace Binder.API.Models.Region.Filesystem
{
	public class LogicalFileVersionModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int Sequence { get; set; }
		public string FileInstanceId { get; set; }
		public string LogicalFileId { get; set; }

		public string CreatedByUserId { get; set; }
		public string CreatedByUsername { get; set; }
		public string CreatedByNickname { get; set; }
		public long FileSizeBytes { get; set; }

		public string Label { get; set; }

		public string IconUrl { get; set; }
		public DateTime CreatedDTSZ { get; set; }
		public DateTime ModifiedDTSZ { get; set; }
		public DateTime LastAccessedDTSZ { get; set; }
	}
}
