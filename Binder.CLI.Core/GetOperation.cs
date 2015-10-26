using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Binder.CLI.Core
{
    public class GetOperation : OperationBase
    {
        private readonly string _url;

        public GetOperation(string url)
        {
            _url = url;
        }

        protected override string GetUrl()
        {
            return _url;
        }

        protected override HttpResponseMessage Invoke(string url)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(url);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.GetAsync("").Result;
            return response;

        }
    }
}
