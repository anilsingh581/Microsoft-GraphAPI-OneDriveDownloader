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
        private static readonly string userId = ConfigurationManager.AppSettings["UserId"];
        private static readonly string folderId = ConfigurationManager.AppSettings["RootFolderId"];
        
        private static readonly string pendingFolderId = ConfigurationManager.AppSettings["PendingFolderId"];
        private static readonly string processedFolderId = ConfigurationManager.AppSettings["ProcessedFolderId"];
        private static readonly string notProcessedFolderId = ConfigurationManager.AppSettings["NotProcessedFolderId"];
        
        private static readonly string downloadDirectoryPath = ConfigurationManager.AppSettings["LocalFolderFilePath"];

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

            /*
            // Get Users Drive
             var users = graphClient.Users[userId].Drives.GetAsync().GetAwaiter().GetResult();

            // Get the file metadata
             var drives = graphClient.Drives[driveId].GetAsync().GetAwaiter().GetResult();
            */

            // Fetch folders in the drive root
            var childrenFolder = graphClient.Drives[driveId].Items[folderId].Children.GetAsync().GetAwaiter().GetResult();
            if (childrenFolder != null)
            {
                Console.WriteLine("📂 Folders in Drive Root:");
                foreach (var item in childrenFolder.Value)
                {
                    if (item.Folder != null) // Check if it's a folder
                    {
                        Console.WriteLine($"📁 Folder Name: {item.Name} (Folder ID: {item.Id})");
                    }
                }
            }

            /*
            var childrenNotProcessed = graphClient.Drives[driveId].Items[notProcessedFolderId].Children.GetAsync().GetAwaiter().GetResult();
            var childrenProcessed = graphClient.Drives[driveId].Items[processedFolderId].Children.GetAsync().GetAwaiter().GetResult();
            */

            // Fetch Files from Pending Folder in the drive root
            var childrenFile = graphClient.Drives[driveId].Items[pendingFolderId].Children.GetAsync().GetAwaiter().GetResult();

            if (childrenFile != null)
            {               
                // Download each files at your location
                Console.WriteLine($"Total file count: {childrenFile.Value.Count}");
                foreach (var item in childrenFile.Value)
                {
                    if (item.File != null)
                    {
                        string fileId = item.Id;

                        Console.WriteLine($"📁 File Name: {item.Name} (File ID: {item.Id}) File Web URL: {item.WebUrl}");

                        var driveItem = graphClient.Drives[driveId].Items[fileId].GetAsync().GetAwaiter().GetResult();
                        var filePath = Path.Combine(downloadDirectoryPath, driveItem.Name);

                        // Get file content
                        using (var stream = graphClient.Drives[driveId].Items[fileId].Content.GetAsync().GetAwaiter().GetResult())
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyToAsync(fileStream).GetAwaiter().GetResult();
                        }
                        Console.WriteLine($"File downloaded: {driveItem.Name} to {filePath}");

                        // Move file to backup folder before deletion
                        // This logic will moved the file from pending folder to Processed or NotProcessed folder
                        bool isProcessed = false; //Todo: As per your logic return value
                        if (isProcessed)
                        {
                            var moveNotProcessedItem = new DriveItem
                            {
                                ParentReference = new ItemReference { Id = notProcessedFolderId }
                            };

                            graphClient.Drives[driveId].Items[fileId].PatchAsync(moveNotProcessedItem).GetAwaiter().GetResult();
                            Console.WriteLine($"File moved to backup folder: {driveItem.Name} (File ID: {item.Id})");
                        }
                        else
                        {
                            //Here processedFolderId is a backupFolderId 
                            var moveProcessedItem = new DriveItem
                            {
                                ParentReference = new ItemReference { Id = processedFolderId }
                            };
                            graphClient.Drives[driveId].Items[fileId].PatchAsync(moveProcessedItem).GetAwaiter().GetResult();
                            Console.WriteLine($"File moved to backup folder: {driveItem.Name} (File ID: {item.Id})");
                        }

                        //var movedFile = graphClient.Drives[driveId].Items[item.Id].GetAsync().GetAwaiter().GetResult();
                        //Console.WriteLine($"New file location: {movedFile.ParentReference.Id}");
                        //If the ParentReference.Id is still the original folder, the move was unsuccessful.

                        // Delete file from the drive after successful download
                        //graphClient.Drives[driveId].Items[item.Id].DeleteAsync().GetAwaiter().GetResult();
                        Console.WriteLine($"File deleted from drive: {driveItem.Name} (File ID: {item.Id})");
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
