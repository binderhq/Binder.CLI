using System;
using Binder.API.Models.Region.Filesystem;

namespace Binder.API.Models.Region.CheckedFiles
{
	public class CheckedOutFileModel
	{
		public string Id { get; set; }
		public string EdocxUserId { get; set; }
		public string SiteUserId { get; set; }
		public DateTime CreatedDTSZ { get; set; }
		public string MachineName { get; set; }
		public string PathOnMachine { get; set; }
		public string BoxId { get; set; }
		public string Path { get; set; }
		public LogicalFileModel LogicalFileModel { get; set; }
	}
}