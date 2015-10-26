namespace Binder.API.Models.Region.SiteNavigator
{
	public class SiteFolderModel : SiteItemInfo
	{
		public class SubFolder : SiteItemInfo
		{

		}

		public SubFolder[] Folders { get; set; }

		public SiteFileModel[] Files { get; set; }

		public string UserPreferences { get; set; }

	}
}
