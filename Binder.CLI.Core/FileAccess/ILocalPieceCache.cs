using System.IO;

namespace Binder.API.Region.Foundation.FileAccess
{
    public interface ILocalPieceCache
    {
        bool ContainsPiece(string pieceHash);
        void PutPieceInCache(string pieceHash, Stream data);
        Stream GetPieceFromCache(string pieceHash);
    }
}
