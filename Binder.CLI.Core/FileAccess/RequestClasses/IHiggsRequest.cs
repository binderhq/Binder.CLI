using System.Net;

namespace Binder.API.Region.Foundation.FileAccess.RequestClasses
{
    interface IHiggsRequest
    {
        WebRequest GetRequest();
    }
}
