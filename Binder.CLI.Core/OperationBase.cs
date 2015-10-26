using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Flurl;

namespace Binder.CLI.Core
{
    public interface IOperation
    {
        HttpResponseMessage ResponseMessage { get; }

    }

    public abstract class OperationBase : IOperation
    {

        protected abstract string GetUrl();
        protected abstract HttpResponseMessage Invoke(string url);




        protected HttpResponseMessage responseMessage = null;

        public HttpResponseMessage ResponseMessage
        {
            get
            {
                if (responseMessage == null)
                {
                    var url = GetUrl();

                    if (_session != null)
                    {
                        url.SetQueryParam("api_key", _session.SessionToken);
                    }
                    responseMessage = Invoke(url);
                }
                return responseMessage;
            }
        }

        protected BinderSession _session = null;

        public IOperation WithSession(BinderSession session)
        {
            session = _session;
            return this;
        }

    }

}
