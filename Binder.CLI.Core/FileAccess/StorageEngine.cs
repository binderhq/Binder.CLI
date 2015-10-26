using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Binder.API.Region.Foundation.FileAccess
{
    /// <summary>
    /// Class which implements storage operations of a file from a client.
    /// </summary>
    public class StorageEngine
    {
        private readonly long _storageZoneId;
        private readonly string _pieceCheckerUrl;
        private readonly ILocalPieceCache _pieceCache;
        private const int Maxpiecesize = 2000000;
        private readonly string _fileCompositionHelperAddress;
        private readonly IHashTool _hash;
        private readonly string _higgsFileRegistrationAddress;
        private string _endpointUrl;

        public StorageEngine(string endpointUrl, string pieceCheckerUrl, string fileCompositionHelperAddress, string higgsFileRegistrationAddress,
            IHashTool hash, long storageZoneId)
            : this(endpointUrl, pieceCheckerUrl, fileCompositionHelperAddress, higgsFileRegistrationAddress,
            hash, storageZoneId, new NullPieceCache())
        {
        }

        /// <summary>
        /// Constructs a storage engine to use
        /// </summary>
        /// <param name="endpointUrl">URL of the endpoint to access</param>
        /// <param name="fileCompositionHelperAddress">Endpoint to which higgs file parts can be retrieved</param>
        /// <param name="higgsFileRegistrationAddress">Endpoint to which newly created Higgs files can be registered</param>
        /// <param name="hash">Hashing algorithm to use (SHA256)</param>
        /// <param name="iocKernel">Inversion of control kernel to use</param>
        /// <param name="storageZoneId">Storage zone to target</param>
        /// <param name="pieceCache">Local piece cache to check against</param>
        public StorageEngine(string endpointUrl, string pieceCheckerUrl, string fileCompositionHelperAddress, string higgsFileRegistrationAddress, IHashTool hash, long storageZoneId, ILocalPieceCache pieceCache)
        {
            EndpointUrl = endpointUrl;
            _fileCompositionHelperAddress = fileCompositionHelperAddress;
            _higgsFileRegistrationAddress = higgsFileRegistrationAddress;
            _hash = hash;
            _storageZoneId = storageZoneId;
            _pieceCache = pieceCache;
            _pieceCheckerUrl = pieceCheckerUrl;
        }

        public string EndpointUrl
        {
            get { return _endpointUrl; }
            set { _endpointUrl = value; }
        }

        /// <summary>
        /// Returns a file based on provided Higgs ID
        /// </summary>
        /// <param name="HiggsFileID">File ID to retireve</param>
        /// <returns></returns>
        public void GetFile(string HiggsFileID, Stream stream)
        {
            GetFile(HiggsFileID, l => { }, stream);
        }

        /// <summary>
        /// Gets a collection of piece hashes (in order) that compose a file
        /// </summary>
        /// <param name="HiggsFileID"></param>
        /// <returns></returns>
        public string[] GetPieceHashes(string HiggsFileID)
        {
            Debug.Assert(HiggsFileID.Length == 64);

            var resource = string.Format("/file/{0}/{1}", _storageZoneId, HiggsFileID);
            var client = (HttpWebRequest)HttpWebRequest.Create(_fileCompositionHelperAddress + resource);

            var response = client.GetResponse() as HttpWebResponse;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new UnauthorizedAccessException("Access to resource has been denied");
                    case HttpStatusCode.BadRequest:
                        throw new InvalidOperationException("Communication to this service has been made incorrectly.");
                    case HttpStatusCode.Forbidden:
                        throw new UnauthorizedAccessException("Access to this resource is forbidden.");
                    case HttpStatusCode.NotFound:
                        throw new EndpointNotFoundException("Resource could not be found.");
                    case HttpStatusCode.InternalServerError:
                        throw new Exception("FileComposition server has encountered an error.");
                    default:
                        throw new Exception(string.Format("Undefined Error has occured: {0}", response.StatusDescription));
                }
            }

            string[] deserialised;

            try
            {

                var serialiser = new XmlSerializer(typeof(string[]));
                deserialised = serialiser.Deserialize(response.GetResponseStream()) as string[];
            }
            catch (Exception e)
            {
                //TODO Handle special error cases
                throw new Exception("Error storing piece", e);
            }

            return deserialised;
        }

        public void GetFile(string HiggsFileID, Action<long> transferProgress, Stream stream)
        {
            Debug.Assert(HiggsFileID.Length == 64);
            var hashes = GetPieceHashes(HiggsFileID);
            long progress = 0;

            //TODO: at some point parallelise this
            for (int i = 0; i < hashes.Length; i++)
            {
                if (_pieceCache.ContainsPiece(hashes[i]))
                {
                    var data = _pieceCache.GetPieceFromCache(hashes[i]);
                    progress += data.Length;
                    data.CopyTo(stream);
                    transferProgress(progress);
                }
                else
                {
                    var higgsClient = new HiggsClient(EndpointUrl, _pieceCheckerUrl, _storageZoneId);
                    var data = higgsClient.GetStream(hashes[i]);
                    using (var myStream = new MemoryStream())
                    {
                        data.CopyTo(myStream);
                        byte[] streamBytes = myStream.ToArray();

                        progress += streamBytes.Length;
                        stream.Write(streamBytes, 0, streamBytes.Length);

                        transferProgress(progress);
                    }


                }
            }
        }

        public StoreFileResponse StoreFile(Stream file)
        {
            return StoreFile(file, l => { });
        }

        public StoreFileResponse StoreFile(Stream file, Action<long> transferProgress)
        {
            var hashes = new List<string>();
            var higgsClient = new HiggsClient(EndpointUrl, _pieceCheckerUrl, _storageZoneId);

            long read = 0;
            while (true)
            {
                var buffer = new byte[Maxpiecesize];
                int nRead;

                nRead = file.Read(buffer, 0, Maxpiecesize);


                if (nRead == 0)
                    break;

                if (nRead != Maxpiecesize)
                {
                    //If the bytes did not fully occupy the original buffer, resize the buffer through a copy.
                    var newBuffer = new byte[nRead];
                    Buffer.BlockCopy(buffer, 0, newBuffer, 0, nRead);
                    buffer = newBuffer;
                    //PeriodEnd of file has been reached if the buffer is not full.
                }

                var pieceHash = CalculatePieceHash(buffer);


                //Send to the store
                higgsClient.PostStream(pieceHash, new MemoryStream(buffer));
                read += buffer.Length;
                transferProgress(read);
                hashes.Add(pieceHash);
            }

            var higgsID = CalculateHiggsFileIDFromPieceHashes(hashes.ToArray());
            var response = new StoreFileResponse
            {
                PieceHashes = hashes.ToArray(),
                HiggsFileID = higgsID,
                WasAlreadyStored = false,
                Length = read
            };

            CreateHiggsFile(higgsID, hashes.ToArray(), read, _storageZoneId);

            return response;

        }

        public string CalculatePieceHash(byte[] buffer)
        {

            var hash = BitConverter.ToString(
                _hash.ComputeHash(buffer)).Replace("-", string.Empty);
            return hash;
        }

        private static int ALLOWED_TRIES = 3;

        public void CreateHiggsFile(string higgsId, string[] hashes, long length, long storageZoneId)
        {
            Debug.Assert(higgsId.Length == 64);

            var resource = string.Format("/file/{0}", higgsId);
            var client = (HttpWebRequest)HttpWebRequest.Create(new Uri(_higgsFileRegistrationAddress + resource));

            client.Headers.Add("edx-storageZoneID", storageZoneId.ToString());
            client.Headers.Add("edx-filesize", length.ToString());
            client.Method = "POST";
            client.ContentType = "text/xml";

            // WIN-480
            client.Timeout = -1;        

            var serialiser = new XmlSerializer(hashes.GetType());


            serialiser.Serialize(client.GetRequestStream(), hashes);


            try
            {
                var response = GetResponseWithRetryAsync(client, ALLOWED_TRIES).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("Request failed to process. Error code: {0} \n Error Message: {1}", (int)response.StatusCode, response.StatusDescription));
                }
            }

            catch (Exception e)
            {
                //TODO Handle special error cases
                throw new Exception("Error registering file", e);
            }
        }

        private static Task<HttpWebResponse> GetResponseWithRetryAsync(WebRequest request, int retries)
        {
            if (retries < 0)
                throw new ArgumentOutOfRangeException("Invalid number of retries to make");

            Func<Task<WebResponse>, Task<HttpWebResponse>> proceedToNextStep = null;

            Func<Task<HttpWebResponse>> doStep = () =>
            {
                var ds = Task.Factory.FromAsync(
                    (asyncCallback, state) => request.BeginGetResponse(asyncCallback, state),
                    (asyncResult) => request.EndGetResponse(asyncResult),
                    null
                );
                return ds.ContinueWith(proceedToNextStep).Unwrap();
            };

            proceedToNextStep = (prevTask) =>
            {
                if (prevTask.IsCanceled)
                    throw new TaskCanceledException();
                if (prevTask.IsFaulted && --retries > 0)
                    return doStep();

                var tcs = new TaskCompletionSource<HttpWebResponse>();
                tcs.SetResult((HttpWebResponse)prevTask.Result);
                return tcs.Task;
            };
            return doStep();
        }



        private string CalculateHiggsFileIDFromPieceHashes(IList<byte[]> pieceHashes)
        {
            var allPieceHashes = new byte[pieceHashes.Count * 32];
            for (var i = 0; i < pieceHashes.Count; i++)
            {
                var thisPieceHash = pieceHashes[i];
                Debug.Assert(thisPieceHash.Length == 32);
                Buffer.BlockCopy(thisPieceHash, 0, allPieceHashes, i * 32, 32);
            }

            return BitConverter.ToString(_hash.ComputeHash(allPieceHashes)).Replace("-", String.Empty);

        }

        public string CalculateHiggsFileIDFromPieceHashes(string[] pieceHashes)
        {
            var byteHashes = pieceHashes.Select(ConvertStringHashToBytes).ToList();
            return CalculateHiggsFileIDFromPieceHashes(byteHashes);

        }

        private static byte[] ConvertStringHashToBytes(string hash)
        {
            //check for null
            if (String.IsNullOrEmpty(hash))
                throw new ArgumentException("Attempt to convert empty hash to bytes");

            if (hash.Length != 64)
                throw new ArgumentException("Attempt to convert non-64 length hash to bytes");


            var result = new byte[32];

            try
            {
                for (int i = 0; i < 32; i++)
                {
                    string hexDoublet = hash.Substring(i * 2, 2);
                    byte thisByte = (byte)Int32.Parse(hexDoublet, NumberStyles.HexNumber);
                    result[i] = thisByte;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to convert hash '" + hash + "' to bytes", ex);
            }

            return result;
        }


    }

    /// <summary>
    /// Interface of which hashing is completed. Should be an SHA256-Based class
    /// </summary>
    public interface IHashTool
    {
        byte[] ComputeHash(byte[] buffer);
    }

    /// <summary>
    /// Response after a file has been stored
    /// </summary>
    public class StoreFileResponse
    {
        public string HiggsFileID { get; set; }
        public bool WasAlreadyStored { get; set; }
        public string[] PieceHashes { get; set; }
        public long Length { get; set; }
    }


}