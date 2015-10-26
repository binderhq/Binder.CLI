using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Binder.CLI.Core
{
    public class PostOperation<TRequest> : OperationBase
    {
        private readonly string _url;

        public PostOperation(string url)
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

            string json = JsonConvert.SerializeObject(_request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = client.PostAsync(new Uri(url), content).Result;

            return response;

        }

        private TRequest _request = default(TRequest);

        public PostOperation<TRequest> WithRequest(TRequest request)
        {
            _request = request;
            return this;
        }
    }
}
