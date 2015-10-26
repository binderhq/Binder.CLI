using System.IO;

namespace Binder.API.Region.Foundation.FileAccess
{
    public interface IHiggsClient
    {
        void PostStream(string piecehash, Stream data);
        Stream GetStream(string piecehash);
    }
}