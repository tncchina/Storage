using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiApp.Common;
using ApiApp.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace ApiApp.Controllers
{
    public class BatchProcessor
    {
        public static async Task ProcessUploadBatch(AppConfiguration appConfiguration, string operationId)
        {
            var response = await appConfiguration.CosmosDBClient.ReadDocumentAsync(
                            UriFactory.CreateDocumentUri(appConfiguration.DatabaseId, appConfiguration.OperationResultCollectionId, operationId),
                            new RequestOptions() { PartitionKey = new PartitionKey(operationId) });

            var operationResult = (OperationResult)(dynamic)response.Resource;
            if (operationResult.Status.Code != OperationStage.InProgress)
            {
                return;
            }

            CloudBlobClient blobClient = appConfiguration.BlobStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(appConfiguration.AnimalImageMetadataContainer);

            CloudBlockBlob blockblob = container.GetBlockBlobReference(operationId);
            string lease = await blockblob.AcquireLeaseAsync(TimeSpan.FromMinutes(1));

            try
            {
                string blobData = await blockblob.DownloadTextAsync();
                List<AnimalImage> animalImages = JsonConvert.DeserializeObject<List<AnimalImage>>(blobData);

                animalImages.ForEach(a => appConfiguration.CosmosDBClient.UpsertDocumentAsync(appConfiguration.AnimalImageCollectionUri, a));

                operationResult.Status.Code = OperationStage.Success;
            }
            catch (Exception ex)
            {
                operationResult.Status.Code = OperationStage.Fail;
                operationResult.Status.Message = ex.ToString();
            }
            finally
            {
                await appConfiguration.CosmosDBClient.UpsertDocumentAsync(
                        appConfiguration.OperationResultCollectionUri,
                        operationResult);
            }
        }
    }
}