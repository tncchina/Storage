using System;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace BatchUploadClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string containerName = "upload";
            string uploadFolder = folderBrowserDialog.ShowDialog() == DialogResult.OK ? folderBrowserDialog.SelectedPath : null;
            if (String.IsNullOrWhiteSpace(uploadFolder))
            {
                return;
            }

            string sasToken = CreateContainerSas(containerName);

            richTextBox.Text = richTextBox.Text + $"Obtained SAS from server: {sasToken}" + Environment.NewLine;

            StorageCredentials accountSAS = new StorageCredentials(sasToken);

            CloudStorageAccount accountWithSAS = new CloudStorageAccount(accountSAS, "photostemp", endpointSuffix: null, useHttps: true);
            CloudBlobClient blobClientWithSAS = accountWithSAS.CreateCloudBlobClient();
            CloudBlobContainer container = blobClientWithSAS.GetContainerReference(containerName);
            richTextBox.Text = richTextBox.Text + "List of items in blob:" + Environment.NewLine;
            foreach (var blob in container.ListBlobs())
            {
                richTextBox.Text = richTextBox.Text + blob.Uri.AbsoluteUri + Environment.NewLine;
            }

            richTextBox.Text = richTextBox.Text + $"Uploading from {uploadFolder}" + Environment.NewLine;

            int truncateLength = uploadFolder.Length + 1;
            int sucessCount = 0, failureCount = 0;

            foreach (var dirName in Directory.EnumerateDirectories(uploadFolder))
            {
                richTextBox.Text = richTextBox.Text + $"Dir: {dirName}" + Environment.NewLine;
                foreach (var fileName in Directory.EnumerateFiles(dirName))
                {
                    string blobName = fileName.Substring(truncateLength).ToLowerInvariant();
                    CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                    richTextBox.AppendText($"{fileName} => {blobName} . . . ");
                    try
                    {
                        blob.UploadFromFile(fileName);
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        richTextBox.Text = richTextBox.Text + "failed." + Environment.NewLine + ex.ToString() + Environment.NewLine;
                        continue;
                    }

                    sucessCount++;
                    richTextBox.AppendText("done." + Environment.NewLine);
                }
            }

            richTextBox.Text = richTextBox.Text + $"Batch upload complete. Sucess: {sucessCount}, failure: {failureCount}" + Environment.NewLine + Environment.NewLine;
        }

        private static string CreateContainerSas(string containerName)
        {
            const string ConnectionString = "";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return container.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7),
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List
            });
        }


    }
}
