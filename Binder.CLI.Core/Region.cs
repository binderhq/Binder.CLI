using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Binder.API.Region.CatalogServices.Models;
using Binder.API.Region.RegionServices.Models.Responses;
using Flurl;

namespace Binder.CLI.Core
{
    public class Region
    {
        private readonly BinderEcosystem _ecosystem;
        private readonly RegionModel _regionModel;
        private readonly BinderSession _session;
        private readonly CurrentRegionModel _currentRegionModel;


        public Region(BinderEcosystem ecosystem, string regionId, BinderSession session)
        {
            _ecosystem = ecosystem;
            _regionModel = new GetRegionOperation(ecosystem, regionId).ResponseMessage.Content<RegionModel>();
            _session = session;
            var currentRegionUrl = _regionModel.Endpoint.AppendPathSegment("CurrentRegion").SetQueryParam("api_key",session.SessionToken);
            _currentRegionModel = new GetOperation(currentRegionUrl).ResponseMessage.Content<CurrentRegionModel>();
        }

        public Site GetSite(string subdomain)
        {
            return new Site(this, subdomain, _session);
        }

        public string EndpointUrl
        {
            get { return _regionModel.Endpoint; }
        }

        public CurrentRegionModel CurrentRegionModel
        {
            get { return _currentRegionModel; }
        }
    }
}
