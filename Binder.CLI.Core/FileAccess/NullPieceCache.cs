using System;
using System.IO;

namespace Binder.API.Region.Foundation.FileAccess
{
    class NullPieceCache : ILocalPieceCache
    {
        public bool ContainsPiece(string pieceHash)
        {
            return false;
        }

        public void PutPieceInCache(string pieceHash, Stream data)
        {
            //This method just runs. Who cares if anything is added?
        }

        public Stream GetPieceFromCache(string pieceHash)
        {
            throw new ArgumentException("This method should not be called when a null piece cache has been created.");
        }
    }
}
