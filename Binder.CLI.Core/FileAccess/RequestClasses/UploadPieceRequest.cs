using System;
using System.Data;
using System.IO;
using System.Net;

namespace Binder.API.Region.Foundation.FileAccess.RequestClasses
{
    class UploadPieceRequest : IHiggsRequest
    {

        private readonly string _postResource;
        private readonly string _postDataResource;
        private readonly int _timeout;

        public UploadPieceRequest(string piecehash, string postDataResource)
            : this(piecehash, postDataResource, -1)
        {
        }

        public UploadPieceRequest(string piecehash, string postDataResource, int timeout)
        {
            _postDataResource = postDataResource;
            _postResource = string.Format("/piece/{0}", piecehash);
            _timeout = timeout;

            RequestStream = null;

            if (timeout == -1)
            {
                _timeout = RequestDefault.DEFAULT_TIMEOUT;
            } else if (timeout <= 0)
            {
                throw new ArgumentException("timeout value must be greater than zero.");
            }
        }

        public Stream RequestStream { get; set; }

        public WebRequest GetRequest()
        {
            if (RequestStream == null)
            {
                throw new NoNullAllowedException("The stream being sent has not been filled.");
            }
            var request = (HttpWebRequest)WebRequest.Create(_postDataResource + _postResource);
            request.Method = "POST";
            RequestStream.Position = 0;
            RequestStream.CopyTo(request.GetRequestStream());
            request.ContentType = "application/octet-stream";
            request.Timeout = _timeout;
            return request;
        }
    }
}
