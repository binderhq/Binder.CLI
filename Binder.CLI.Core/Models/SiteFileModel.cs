using System;
using Binder.API.Models.Region.CheckedFiles;

namespace Binder.API.Models.Region.SiteNavigator
{
	public class SiteFileModel : SiteItemInfo
	{
		public string DownloadUrl { get; set; }
		public DateTime LastWriteTimeUtc { get; set; }
		public long Length { get; set; }
		public string ThumbnailUrl { get; set; }
		public string _LogicalFileId { get; set; }
		public CheckedOutFileModel CheckedOutInfo { get; set; }
	}
}
