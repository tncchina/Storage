using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using ApiApp.Models;
using Microsoft.WindowsAzure.Storage;
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
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public string Post([FromBody]AnimalImage animalImage)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.AppConfiguration.GetStorageConnectionString());
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("test123");

            // Create a new container, if it does not exist
            container.CreateIfNotExists();

            CloudBlockBlob blockblob = container.GetBlockBlobReference(animalImage.Id ?? "123");
            string sas = blockblob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1)
                });

            return $"sasurl for image{animalImage.Id}'s blob {blockblob.Uri.AbsoluteUri + sas}";
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
