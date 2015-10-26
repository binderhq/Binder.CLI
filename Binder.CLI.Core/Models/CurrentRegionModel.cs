namespace Binder.API.Region.RegionServices.Models.Responses
{
    public class CurrentRegionModel
    {
        public string Id { get; set; }
        public string FileCompositionEndpoint { get; set; }
        public string FileRegistrationEndpoint { get; set; }
        public string PieceCheckerEndpoint { get; set; }
        public string EcosystemId { get; set; }

    }
}
