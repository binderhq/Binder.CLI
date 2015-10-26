using System;
using System.Net;

namespace Binder.API.Region.Foundation.FileAccess.RequestClasses
{
    class DownloadPieceRequest : IHiggsRequest
    {
        private readonly string _postResource;
        private readonly string _getDataResource;
        private readonly int _timeout;

        public DownloadPieceRequest(string piecehash, string getDataResource)
            : this(piecehash, getDataResource, -1)
        {
        }

        public DownloadPieceRequest(string piecehash, string getDataResource, int timeout)
        {

            _getDataResource = getDataResource;
            _postResource = string.Format("/piece/{0}", piecehash);

            if (timeout == -1)
            {
                _timeout = RequestDefault.DEFAULT_TIMEOUT;
            }
            else if (timeout <= 0)
            {
                throw new ArgumentException("timeout value must be greater than zero.");
            }

        }
        public WebRequest GetRequest()
        {
            var request = (HttpWebRequest)WebRequest.Create(_getDataResource + _postResource);
            request.Method = "GET";
            request.ContentType = "application/octet-stream";
            request.Timeout = _timeout;
            return request;
        }
    }
}
