using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Binder.API.Region.CatalogServices.Models;
using Flurl;
using Newtonsoft.Json;

namespace Binder.CLI.Core
{
    public class BinderEcosystem
    {
        private readonly string _catalogUrl;
        public CurrentEcosystemModel CurrentEcosystem { get; private set; }
        public string CatalogUrl { get { return _catalogUrl; } }

        public BinderEcosystem(string catalogUrl)
        {
            _catalogUrl = catalogUrl;

            var client = new HttpClient();
            client.BaseAddress = new Uri(_catalogUrl);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(catalogUrl.AppendPathSegment("CurrentEcosystem")).Result;

            string str = response.Content.ReadAsStringAsync().Result;
            CurrentEcosystem = JsonConvert.DeserializeObject<CurrentEcosystemModel>(str);

            AuthenticationEndpoint = new AuthenticationEndpoint(CurrentEcosystem.AuthenticationEndpoint);

        }

        public AuthenticationEndpoint AuthenticationEndpoint { get; private set; }
    
       

    }
}
