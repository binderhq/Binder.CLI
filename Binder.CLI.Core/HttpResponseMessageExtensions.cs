using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Binder.CLI.Core
{
    public static class HttpResponseMessageExtensions
    {
        static public TResponse Content<TResponse>(this HttpResponseMessage responseMessage)
        {
            string str = responseMessage.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<TResponse>(str);
        }

    }
}
