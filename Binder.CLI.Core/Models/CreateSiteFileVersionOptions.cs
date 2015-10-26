using System;

namespace Binder.API.Region.RegionServices.Models.Requests
{
    public class CreateSiteFileVersionOptions
    {
        public string Name { get; set; }
        public string HiggsFileId { get; set; }
        public string StorageZoneId { get; set; }
        public DateTime FileModifiedTimeUtc { get; set; }
        public long Length { get; set; }
    }

}