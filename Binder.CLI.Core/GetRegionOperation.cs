using Flurl;

namespace Binder.CLI.Core
{
    public class GetRegionOperation : GetOperation
    {
        public GetRegionOperation(BinderEcosystem ecosystem, string regionId) : base(GetUrlFor(ecosystem,regionId))
        {
        }

        private static string GetUrlFor(BinderEcosystem ecosystem, string regionId)
        {
            string url = ecosystem.CatalogUrl.AppendPathSegment("Regions").AppendPathSegment(regionId);
            return url;
        }
    }
}
