using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Binder.API.Region.Foundation.FileAccess.RequestClasses;

namespace Binder.API.Region.Foundation.FileAccess
{
    public class HiggsClient : IHiggsClient
    {
        private const int ALLOWED_TRIES = 3;
        private const int DEFAULT_TIMEOUT = 300000; //5m
        private readonly string _higgsUrl;
        private readonly string _pieceCheckerUrl;

        private readonly long _storageZoneId;

        public HiggsClient(string higgsUrl, string pieceCheckerUrl, long storageZoneId)
            : this(higgsUrl, pieceCheckerUrl, storageZoneId, DEFAULT_TIMEOUT)
        {
        }

        public HiggsClient(string higgsUrl, string pieceCheckerUrl, long storageZoneId, int timeout)
        {
            _higgsUrl = higgsUrl;
            _pieceCheckerUrl = pieceCheckerUrl;
            _storageZoneId = storageZoneId;
            Timeout = timeout;
        }

        public int Timeout { get; private set; } //The timeout for request tasks in ms

        public void PostStream(string piecehash, Stream data)
        {
            if (PieceExists(piecehash)) 
                return;

            //PeriodStart the task
            var postData = new UploadPieceRequest(piecehash, _higgsUrl, Timeout);
            postData.RequestStream = data;
            string responseLiteral;
            using (HttpWebResponse postDataResponse = GetResponseWithRetryAsync(postData, ALLOWED_TRIES).Result)
            {
                using (var sr = new StreamReader(postDataResponse.GetResponseStream()))
                {
                    responseLiteral = sr.ReadToEnd();
                }
            }
        }

        public bool PieceExists(string piecehash)
        {
            var checkRequest = new CheckPieceRequest(_storageZoneId.ToString(), piecehash, _pieceCheckerUrl, Timeout);
            using (Task<HttpWebResponse> checkResponse = GetResponseWithRetryAsync(checkRequest, ALLOWED_TRIES))
            {
                using (var sr = new StreamReader(checkResponse.Result.GetResponseStream()))
                {
                    //If the piece exists, it returns true
                    if (bool.Parse(sr.ReadToEnd()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Stream GetStream(string piecehash)
        {
            var getResource = new DownloadPieceRequest(piecehash, _higgsUrl);


            using (Task<HttpWebResponse> getResponse = GetResponseWithRetryAsync(getResource, ALLOWED_TRIES))
            {
                try
                {
                    return getResponse.Result.GetResponseStream();
                }
                catch (Exception e)
                {
                    //TODO Handle special error cases
                    throw new Exception("Error retrieving piece", e);
                }
            }
        }

        private static Task<HttpWebResponse> GetResponseWithRetryAsync(IHiggsRequest higgsRequest, int retries)
        {
            if (retries < 0)
                throw new ArgumentOutOfRangeException("Invalid number of retries to make.");

            Func<Task<WebResponse>, Task<HttpWebResponse>> proceedToNextStep = null;

            Func<Task<HttpWebResponse>> doStep = () =>
            {
                try
                {
                    WebRequest request = higgsRequest.GetRequest();
                    Task<WebResponse> ds = Task.Factory.FromAsync<WebResponse>(
                        request.BeginGetResponse,
                        request.EndGetResponse,
                        null
                        );
                    return ds.ContinueWith(proceedToNextStep).Unwrap();
                }
                catch (IOException e)
                {
                    return proceedToNextStep(null); //Something to do with Mono SSL occasionally breaking down
                }
            };

            proceedToNextStep = prevTask =>
            {
                if (prevTask == null)
                {
                    if (--retries <= 0)
                    {
                        throw new Exception(
                            "There seems to be an issue with the connection to Binder. Please retry again later.");
                    }
                    return doStep();
                }
                if (prevTask.IsCanceled)
                    throw new TaskCanceledException();
                if (prevTask.IsFaulted && --retries > 0)
                    return doStep();

                var tcs = new TaskCompletionSource<HttpWebResponse>();
                try
                {
                    tcs.SetResult((HttpWebResponse) prevTask.Result);
                }
                catch (AggregateException ex)
                {
                    if (--retries > 0)
                        return doStep();
                    throw new HiggsClientException("The request has failed to process. Please try again", ex);
                }
                return tcs.Task;
            };
            return doStep();
        }
    }

    public class HiggsClientException : Exception
    {
        public HiggsClientException()
        {
        }

        public HiggsClientException(string message) : base(message)
        {
        }

        public HiggsClientException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}