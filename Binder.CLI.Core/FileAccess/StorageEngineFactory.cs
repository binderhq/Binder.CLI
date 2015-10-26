using System;
using System.Security.Cryptography;

namespace Binder.API.Region.Foundation.FileAccess
{
    public static class StorageEngineFactory
    {
        public static StorageEngine Create(string storageEndpoint, string pieceCheckerEndpoint, string fileCompositionEndpoint, string fileRegistrationEndpoint, long storageZoneId)
        {
            return new StorageEngine(storageEndpoint, pieceCheckerEndpoint, fileCompositionEndpoint, fileRegistrationEndpoint, new HashTool(), storageZoneId);
        }

        public static StorageEngine Create(string storageEndpoint, string pieceCheckerEndpoint, string fileCompositionEndpoint,
            string fileRegistrationEndpoint, long storageZoneId, ILocalPieceCache pieceCache)
        {
            return new StorageEngine(storageEndpoint, pieceCheckerEndpoint, fileCompositionEndpoint, fileRegistrationEndpoint, new HashTool(), storageZoneId, pieceCache);
        }
    }

    public class HashTool : IHashTool
    {
        public readonly HashAlgorithm _hashObject;
        public HashTool()
        {
            _hashObject = new SHA256Managed();
        }

        public byte[] ComputeHash(byte[] buffer)
        {
            try
            {
                return _hashObject.ComputeHash(buffer);
            }
            catch (ArgumentNullException e)
            {

                throw new ArgumentException("buffer", e);
            }
            catch (ObjectDisposedException e)
            {
                throw new Exception("Stream has already been disposed of.", e);
            }
        }
    }
}
