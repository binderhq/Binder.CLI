using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using Flurl;

namespace Binder.CLI.Core
{
    public class GetFolderOperation : GetOperation
    {
        private readonly BinderSession _session;

        public GetFolderOperation(Site site, string folderPath, BinderSession session) : base(GetUrlFor(site,folderPath, session))
        {
        }

        private static string GetUrlFor(Site site, string folderPath, BinderSession session)
        {

            var url =
                site.Region.EndpointUrl.AppendPathSegment("SiteNavigator")
                    .AppendPathSegment(site.Subdomain)
                    .AppendPathSegment("Folder")
                    .SetQueryParam("path", folderPath)
                    .SetQueryParam("api_key", session.SessionToken);



            return url;

        }
    }
}
