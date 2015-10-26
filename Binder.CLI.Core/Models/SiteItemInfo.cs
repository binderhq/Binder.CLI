namespace Binder.API.Models.Region.SiteNavigator
{
    public class SiteItemInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string[] Behaviours { get; set; }
        public string IconUrl { get; set; }
        public string ContainingPath { get; set; }
        public string[] Privileges { get; set; }
        public string _UnderlyingId { get; set; }
        public string ThumbnailUrl { get; set; }
        public string[] Tags { get; set; }
	
    }
}