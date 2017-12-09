using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ApiApp.Common;
using ApiApp.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage.Blob;
using Swashbuckle.Swagger.Annotations;

namespace ApiApp.Controllers
{
    public class AnimalImagesController : BaseController
    {
        // GET api/values
        [SwaggerOperation("GetAll")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<AnimalImage> Get(string id)
        {
            AnimalImage result;
            var response = await this.AppConfiguration.CosmosDBClient.ReadDocumentAsync(
                UriFactory.CreateDocumentUri(this.AppConfiguration.DatabaseId, this.AppConfiguration.AnimalImageCollectionId, id),
                new RequestOptions() { PartitionKey = new PartitionKey(id) });

            result = (AnimalImage)(dynamic)response.Resource;

            CloudBlobClient blobClient = this.AppConfiguration.BlobStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(this.AppConfiguration.AnimalImageContainer);
            CloudBlockBlob blockblob = container.GetBlockBlobReference(result.ImageBlob);

            result.DownloadBlobSASUrl = Utils.GenerateReadSasUrl(blockblob);

            return result;
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<AnimalImage> Post([FromBody]AnimalImage animalImage)
        {
            CloudBlobClient blobClient = this.AppConfiguration.BlobStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(this.AppConfiguration.AnimalImageContainer);

            // Create a new container, if it does not exist
            container.CreateIfNotExists();

            // TODO: validate input
            animalImage.Id = Guid.NewGuid().ToString().ToLowerInvariant();
            animalImage.ImageName = animalImage.ImageName.ToLowerInvariant();
            animalImage.FileFormat = animalImage.FileFormat.ToLowerInvariant();
            animalImage.ImageBlob = animalImage.Id + "/" + animalImage.ImageName +
                "." + animalImage.FileFormat;
            CloudBlockBlob blockblob = container.GetBlockBlobReference(animalImage.ImageBlob);

            await this.AppConfiguration.CosmosDBClient.UpsertDocumentAsync(
                this.AppConfiguration.AnimalImageCollectionUri,
                animalImage);

            animalImage.UploadBlobSASUrl = Utils.GenerateWriteSasUrl(blockblob);

            return animalImage;
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}
