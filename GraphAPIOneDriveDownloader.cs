using Azure.Identity;
using Microsoft.Graph;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Graph.Models;

namespace SelfBillInvoiceService.Utils.GraphAPI
{
    public class OneDriveDownloader
    {
        // Define required scopes
        private static readonly string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

        private readonly GraphServiceClient graphClient;

        // Read values from app.config
        private static readonly string clientId = ConfigurationManager.AppSettings["ClientId"];
        private static readonly string tenantId = ConfigurationManager.AppSettings["TenantId"];
        private static readonly string clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        private static readonly string driveId = ConfigurationManager.AppSettings["DriveId"];
        private static readonly string folderId = ConfigurationManager.AppSettings["FolderId"];
        private static readonly string userId = ConfigurationManager.AppSettings["UserId"];
        private static readonly string downloadDirectoryPath = ConfigurationManager.AppSettings["DownloadDirectoryFolderPath"];

        /// <summary>
        /// Constructor that initializes GraphServiceClient
        /// </summary>
        public OneDriveDownloader()
        {
            graphClient = GetGraphServiceClient();
        }

        /// <summary>
        /// Authenticate and return a GraphServiceClient instance.
        /// </summary>
        private static GraphServiceClient GetGraphServiceClient()
        {
            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            return new GraphServiceClient(clientSecretCredential, scopes);
        }

        /// <summary>
        /// Downloads a file from OneDrive using its file ID.
        /// </summary>
        /// <param name="fileId">The ID of the file in OneDrive.</param>
        /// <param name="downloadDirectory">Local directory to save the file.</param>
        public async Task DownloadFileAsync()
        {

            // Ensure directory exists
            Directory.CreateDirectory(downloadDirectoryPath);

            //var users = graphClient.Users[userId].Drives.GetAsync().GetAwaiter().GetResult();

            // Get the file metadata
            //var drives = graphClient.Drives[driveId].GetAsync().GetAwaiter().GetResult();

            // Fetch folders in the drive root
            var children = graphClient.Drives[driveId].Items[folderId].Children.GetAsync().GetAwaiter().GetResult();

            if (children != null)
            {
                Console.WriteLine("📂 Folders in Drive Root:");
                foreach (var item in children.Value)
                {
                    if (item.Folder != null) // Check if it's a folder
                    {
                        Console.WriteLine($"📁 Folder Name: {item.Name} (Folder ID: {item.Id})");
                    }
                }

                //Download each files at your location
                Console.WriteLine($"Total file count: {children.Value.Count}");
                foreach (var item in children.Value)
                {
                    if (item.File != null)
                    {
                        Console.WriteLine($"📁 File Name: {item.Name} (File ID: {item.Id}) File Web URL: {item.WebUrl}");

                        var driveItem = graphClient.Drives[driveId].Items[item.Id].GetAsync().GetAwaiter().GetResult();
                        var filePath = Path.Combine(downloadDirectoryPath, driveItem.Name);


                        // Get file content
                        using (var stream = graphClient.Drives[driveId].Items[item.Id].Content.GetAsync().GetAwaiter().GetResult())
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyToAsync(fileStream).GetAwaiter().GetResult();
                        }

                        Console.WriteLine($"File downloaded: {driveItem.Name} to {filePath}");
                    }
                }
            }
        }


        /// <summary>
        /// List all drives available
        /// </summary>
        /// <returns></returns>
        public async Task ListDrivesAsync()
        {
            var drives = await graphClient.Me.Drives.GetAsync();
            foreach (var drive in drives.Value)
            {
                Console.WriteLine($"Drive ID: {drive.Id} - {drive.Name}");
            }
        }


        /// <summary>
        /// List all files in the root folder
        /// </summary>
        /// <param name="driveId"></param>
        /// <returns></returns>
        public async Task ListFilesAsync(string driveId)
        {
            var files = await graphClient.Drives[driveId].Items["root"].Children.GetAsync();
            if (files?.Value?.Count > 0)
            {
                foreach (var file in files.Value)
                {
                    Console.WriteLine($"📁 {file.Name} (ID: {file.Id})");
                }
            }
            else
            {
                Console.WriteLine("No files found in the root directory.");
            }
        }
    }
}
