using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Binder.API.Region.RegionServices.Models.Requests;
using Flurl;

namespace Binder.CLI.Core
{
    public class CreateFolderOperation : PutOperation<CreateFolderRequest>
    {

        private readonly BinderSession _session;

        public CreateFolderOperation(Site site, string folderPath, BinderSession session, string newFolderName)
            : base(GetUrlFor(site, folderPath, session))
        {
            WithRequest(new CreateFolderRequest() {FolderName = newFolderName});
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