using System;
using System.IO;
using System.Linq;
using Binder.API.Models.Region.SiteNavigator;
using Binder.API.Region.Foundation.FileAccess;
using Binder.API.Region.RegionServices.Data.DTOs;
using Binder.API.Region.RegionServices.Models.Requests;
using Binder.CLI.Core;
using Flurl;
using PowerArgs;

// ReSharper disable InconsistentNaming

namespace Binder.CLI
{
    [TabCompletion]
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class CommandLineArguments
    {

        private const string catalogUrl = "https://api.binder.works/catalog";

        private BinderEcosystem _binderEcosystem;

        public CommandLineArguments()
        {
            // !!! make sure this ends in /
            _binderEcosystem = new BinderEcosystem(catalogUrl); 
        }

        [HelpHook, ArgShortcut("?"), ArgDescription("Shows this help")]
        public bool help { get; set; }


       


        [ArgActionMethod, ArgDescription("List folder contents")]
        public void dir(DirArguments dirArguments)
        {
            var site = GetAuthorisedSite();


            var folderResponseMessage =
               new GetFolderOperation(site, dirArguments.source, AuthorisedSession).ResponseMessage;

            var siteFolderModel = folderResponseMessage.Content<SiteFolderModel>();

            foreach (var folder in siteFolderModel.Folders.ToArray().OrderBy(folder => folder.Name))
            {
                System.Console.WriteLine(folder.Name);
            }

            foreach (var file in siteFolderModel.Files)
            {
                Console.Write(file.Name.PadRight(45));
                Console.Write(file.LastWriteTimeUtc.ToLocalTime().ToShortDateString().PadRight(15));
                Console.Write(file.Length + " bytes");
                Console.WriteLine();
            }



        }

        private BinderSession AuthorisedSession { get; set; }

        private Site GetAuthorisedSite()
        {
            var authenticationEndpoint = _binderEcosystem.AuthenticationEndpoint;
            AuthorisedSession = authenticationEndpoint.CreateSession(this.username, this.password);
            if (AuthorisedSession == null)
            {
                throw new ApplicationException("Unable to login");
            }

            var region = new Region(_binderEcosystem, "au", AuthorisedSession);
            return region.GetSite(this.Subdomain);
        }


        [ArgActionMethod, ArgDescription("Upload files or folders")]
        public void upload(UploadArguments uploadArguments)
        {

            var site = GetAuthorisedSite();
            var region = site.Region;
            const int fixedStorageZoneId = 1;

            var storageZoneUrl =
                region.EndpointUrl.AppendPathSegment("StorageZones")
                                    .AppendPathSegment(fixedStorageZoneId.ToString())
                                    .SetQueryParam("api_key",AuthorisedSession.SessionToken);

            var storageZone = new GetOperation(storageZoneUrl).ResponseMessage.Content<StorageZoneDTO>();


            Console.WriteLine("Uploading to " + site + " " + uploadArguments.destination);

            var directory = Path.GetDirectoryName(uploadArguments.source);
            if (directory==null)
                throw new ApplicationException("Could not access directory in " + uploadArguments.source);
            var pattern = uploadArguments.source.Substring(uploadArguments.source.LastIndexOf('\\') + 1);

            var files = Directory.GetFiles(directory, pattern);
            Console.WriteLine(files.Length + " files");

            StorageEngine storageEngine = StorageEngineFactory.Create(
                storageZone.HiggsUrl, 
                region.CurrentRegionModel.PieceCheckerEndpoint,
                region.CurrentRegionModel.FileCompositionEndpoint,
                region.CurrentRegionModel.FileRegistrationEndpoint,
                fixedStorageZoneId);

            foreach (var file in files)
            {
                //string directoryName = Path.GetDirectoryName(file);
                string fileName = Path.GetFileName(file);

                Console.Write(fileName.PadRight(45) + " ");

                try
                {


                    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        var storageResponse = storageEngine.StoreFile(fileStream);

                        var fileInfo = new FileInfo(file);
                        var options = new CreateSiteFileVersionOptions()
                        {
                            Length = fileInfo.Length,
                            FileModifiedTimeUtc = fileInfo.LastWriteTimeUtc,
                            HiggsFileId = storageResponse.HiggsFileID,
                            Name = fileName,
                            StorageZoneId = fixedStorageZoneId.ToString()
                        };


                        var url = site.Region.EndpointUrl
                            .AppendPathSegment("SiteNavigator")
                            .AppendPathSegment(site.Subdomain)
                            .AppendPathSegment("Folder")
                            .AppendPathSegment("Files")
                            .SetQueryParam("path", uploadArguments.destination)
                            .SetQueryParam("api_key", AuthorisedSession.SessionToken);


                        var addFileResponseMessage = new PutOperation<CreateSiteFileVersionOptions>(url)
                            .WithRequest(options)
                            .ResponseMessage;

                        var siteFileModel = addFileResponseMessage.Content<SiteFileModel>();
                        if (siteFileModel.Length!=fileInfo.Length)
                            throw new ApplicationException("Uploaded file length does not match");

                        Console.WriteLine(addFileResponseMessage.StatusCode.ToString());

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                }

            }


/**
            Console.WriteLine("source=" + uploadArguments.source);
            Console.WriteLine("destination=" + uploadArguments.destination);
            Console.WriteLine("site=" + this.site);
            Console.WriteLine("username=" + this.username);
            Console.WriteLine("subdomain=" + this.Subdomain);
 * 
 * **/




        }


        [ArgRequired]
        public string site { get; set; }

        [ArgRequired]
        public string username { get; set; }

        [ArgRequired]
        public string password { get; set; }


        private string Subdomain
        {
            get
            {
                if (site.Contains("."))
                    return site.Substring(0, site.IndexOf('.'));
                else
                    return site;
            }
        }





    }



}
// ReSharper restore InconsistentNaming
