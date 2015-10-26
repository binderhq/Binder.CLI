using System;
using System.Net;

namespace Binder.API.Region.Foundation.FileAccess.RequestClasses
{
    class CheckPieceRequest : IHiggsRequest
    {
        private readonly string _checkResource;
        private readonly string _pieceCheckerUrl;
        private readonly int _timeout;

        public CheckPieceRequest(string storageZoneId, string piecehash, string pieceCheckerUrl)
            : this(storageZoneId, piecehash, pieceCheckerUrl, -1)
        {
        }

        public CheckPieceRequest(string storageZoneId, string piecehash, string pieceCheckerUrl, int timeout)
        {
            _pieceCheckerUrl = pieceCheckerUrl;
            _checkResource = string.Format("/exists/{0}/{1}", storageZoneId, piecehash);

            if (timeout == -1)
            {
                _timeout = RequestDefault.DEFAULT_TIMEOUT;
            } else if (timeout <= 0)
            {
                throw new ArgumentException("timeout value must be greater than zero.");
            }
        }

        public WebRequest GetRequest()
        {
            var request = (HttpWebRequest) WebRequest.Create(_pieceCheckerUrl + _checkResource);
            request.Method = "GET";
            request.ContentType = "application/octet-stream";
            request.Timeout = _timeout;
            return request;
        }
    }
}
