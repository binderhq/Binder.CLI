using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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


		const int fixedStorageZoneId = 1;
		public List<List<String>> csvData = new List<List<string>>();

		[ArgActionMethod, ArgDescription("Upload files or folders")]
		public void upload(UploadArguments uploadArguments)
		{
			if (uploadArguments.csv != null)
			{
				List<string> headerInfo = new List<string>();
				headerInfo.Add("Filename");
				headerInfo.Add("Path");
				headerInfo.Add("Size (B)");
				headerInfo.Add("Last time modified");
				headerInfo.Add("Upload status");
				csvData.Add(headerInfo);
			}

			var site = GetAuthorisedSite();
			var region = site.Region;
		   
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

		  
			StorageEngine storageEngine = StorageEngineFactory.Create(
				storageZone.HiggsUrl, 
				region.CurrentRegionModel.PieceCheckerEndpoint,
				region.CurrentRegionModel.FileCompositionEndpoint,
				region.CurrentRegionModel.FileRegistrationEndpoint,
				fixedStorageZoneId);


			var files = Directory.GetFiles(directory, pattern);
			Console.WriteLine(files.Length + " files");

			UploadFiles(files, site, storageEngine, uploadArguments.destination, uploadArguments.force, uploadArguments.csv != null);

			Console.WriteLine("Uploading recursively");

			if (uploadArguments.recursive)
			{
				foreach (var subDirectory in Directory.GetDirectories(Path.GetDirectoryName(uploadArguments.source)))
				{
					string subDirectoryName = new DirectoryInfo(subDirectory).Name;
					string directorySource = Path.GetDirectoryName(uploadArguments.source);
					UploadDirectory(storageEngine,site, subDirectoryName, directorySource, uploadArguments.destination, uploadArguments.force, uploadArguments.csv != null);
				}
			}

			if (uploadArguments.csv != null)
			{
				Console.WriteLine("Generating .csv file...");
				
				int length = csvData.Count;
				StringBuilder sb = new StringBuilder();
				
				for (int index = 0; index < length; index++)
					sb.AppendLine(string.Join(",", csvData[index]));
				try
				{
					File.WriteAllText(uploadArguments.csv, sb.ToString());
					Console.WriteLine(".csv file creating in directory " + uploadArguments.csv);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
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



		private void UploadDirectory(StorageEngine storageEngine, Site binderSite, string subDirectoryName, string directorySource, string directoryDestination, bool force, bool csv)
		{

			try
			{


				Console.WriteLine("Uploading directory " + directorySource + "\\" + subDirectoryName);

				var directoryDestinationModel =
					new GetFolderOperation(binderSite, directoryDestination, AuthorisedSession).ResponseMessage
						.Content<SiteFolderModel>();

				if (directoryDestinationModel.Folders.All(folder => folder.Name.ToLower() != subDirectoryName.ToLower()))
				{
					Console.WriteLine("Creating folder " + subDirectoryName);
					var createFolderOperation = new CreateFolderOperation(binderSite, directoryDestination,
						AuthorisedSession, subDirectoryName);

					if (!createFolderOperation.ResponseMessage.IsSuccessStatusCode)
					{
						throw new ApplicationException("Could not create folder " + subDirectoryName + " - " + createFolderOperation.ResponseMessage.StatusCode);
					}

				}


				string newSource = Path.Combine(directorySource, subDirectoryName);
				string newDestination = directoryDestination.TrimEnd('/') + "/" + subDirectoryName;


				var files = Directory.GetFiles(newSource, "*.*");
				Console.WriteLine(files.Length + " files");

				UploadFiles(files, binderSite, storageEngine, newDestination, force, csv);
				foreach (var newDirectory in Directory.GetDirectories(newSource))
				{
					string newDirectoryName = new DirectoryInfo(newDirectory).Name;                    
					UploadDirectory(storageEngine, binderSite, newDirectoryName, newSource, newDestination, force, csv);
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR: Could not upload directory " + subDirectoryName + " - " + ex.Message);
			}


		}

		private void UploadFiles(string[] files, Site binderSite, StorageEngine storageEngine, string destination, bool force, bool csv)
		{
			foreach (var file in files)
			{
				//string directoryName = Path.GetDirectoryName(file);
				string fileName = Path.GetFileName(file);
				string statusMessage = "";

				Console.Write(fileName.PadRight(45) + " ");

				var fileInfo = new FileInfo(file);

				try
				{   
					var site = GetAuthorisedSite();
					var folderResponseMessage =
					   new GetFolderOperation(site, destination, AuthorisedSession).ResponseMessage;
					var siteFolderModel = folderResponseMessage.Content<SiteFolderModel>();

					//find file with exact same filename
					var uploadedFile = siteFolderModel.Files.Where(s => s.Name == fileInfo.Name).FirstOrDefault();

					//if found, compare the tick rates. If they're close enough, assume they're the same and don't bother uploading the file
					if (uploadedFile != null && !force)
					{
						var tickDifference = fileInfo.LastWriteTimeUtc.Ticks - uploadedFile.LastWriteTimeUtc.Ticks;
						if (tickDifference < 5000000 && tickDifference > -5000000)
						{
							throw new ApplicationException("Files are identical.");
						}
					}

						
					using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
					{
					   
						var storageResponse = storageEngine.StoreFile(fileStream);
						//storageResponse.Length
						var options = new CreateSiteFileVersionOptions()
						{
							Length = fileInfo.Length,
							FileModifiedTimeUtc = fileInfo.LastWriteTimeUtc,
							HiggsFileId = storageResponse.HiggsFileID,
							Name = fileName,
							StorageZoneId = fixedStorageZoneId.ToString()
						};

						//if (uploadedOptions.LastWriteTimeUtc == options.FileModifiedTimeUtc)
						//    throw new ApplicationException("exists");

						var url = binderSite.Region.EndpointUrl
							.AppendPathSegment("SiteNavigator")
							.AppendPathSegment(binderSite.Subdomain)
							.AppendPathSegment("Folder")
							.AppendPathSegment("Files")
							.SetQueryParam("path", destination)
							.SetQueryParam("api_key", AuthorisedSession.SessionToken);

						//check rem dir and get dirrctory info

						var addFileResponseMessage = new PutOperation<CreateSiteFileVersionOptions>(url)
							.WithRequest(options)
							.ResponseMessage;
						
						var siteFileModel = addFileResponseMessage.Content<SiteFileModel>();

						if (!addFileResponseMessage.IsSuccessStatusCode)
							throw new ApplicationException("ERROR: Could not upload file - " + addFileResponseMessage.StatusCode);

						if (siteFileModel.Length != fileInfo.Length)
							throw new ApplicationException("ERROR: Uploaded file length does not match");

						statusMessage = addFileResponseMessage.StatusCode.ToString();
						Console.WriteLine(statusMessage);


					}
					
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					statusMessage = ex.Message;
				}
				if (csv)
				{
					List<string> uploadInfo = new List<string>();
					uploadInfo.Add(fileInfo.Name.ToString());
					uploadInfo.Add(fileInfo.DirectoryName.ToString());
					uploadInfo.Add(fileInfo.Length.ToString());
					uploadInfo.Add(fileInfo.LastWriteTime.ToString());
					uploadInfo.Add(statusMessage);
					csvData.Add(uploadInfo);
				}
			}
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
