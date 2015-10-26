
namespace Binder.API.Region.CatalogServices.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CurrentEcosystemModel
    {
       

        /// <summary>
        /// A short string identifying the ecosystem. "Production" is the live Edocx ecosystem.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AuthenticationEndpoint { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TransitionApiEndpoint { get; set; }
    }
}
